using System;
using IpData;
using System.IO;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using GraffLicenceManager.DB;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace GraffLicenceManager.Hubs
{
    public class UserHub : Hub
    {
        private readonly MailSender mailSender;
        private readonly DatabaseService databaseService;
        private readonly IpDataClient ipDataClient;

        public UserHub(DatabaseService service, MailSender sender)
        {
            databaseService = service;
            mailSender = sender;
            ipDataClient = new IpDataClient("a6162964e4404f14e89accb08d97fe2ec852e20014ffa5de62758df9");
        }

        public override async Task OnConnectedAsync()
        {
            var ip = Context.GetHttpContext()
                .Connection
                .RemoteIpAddress
                .ToString();

            IpData.Models.IpInfo ipInfo = new IpData.Models.IpInfo();
            try
            {
                ipInfo = await ipDataClient.Lookup(ip.ToString());
            }
            catch
            {
                ip = "212.220.216.185";
                ipInfo = await ipDataClient.Lookup(ip.ToString());
            }

            Console.WriteLine($"[{DateTime.Now}] клиент {ip}({ipInfo.City}) появился в сети.");

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Computer computer = databaseService.GetComputers()
                .ToList()
                .Find(x => x.sessionId == Context.ConnectionId);

            if (computer != null)
            {
                computer.isActive = false;
                computer.sessionId = string.Empty;
                computer.lastActivity = DateTime.Now.ToString();

                databaseService.UpdateComputer(computer);

                Console.WriteLine($"[{DateTime.Now}] {computer.localUserName}({computer.productName}) отключился.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task OnInitializationRequest(RegistrationData registrationData)
        {
            var ip = Context
                .GetHttpContext()
                .Connection
                .RemoteIpAddress
                .ToString();

            IpData.Models.IpInfo ipInfo = new IpData.Models.IpInfo();
            try
            {
                ipInfo = await ipDataClient.Lookup(ip.ToString());
            }
            catch
            {
                ip = "212.220.216.185";
                ipInfo = await ipDataClient.Lookup(ip.ToString());
            }

            var license = databaseService.GetLicense(registrationData.productName);
            var computer = databaseService.GetComputer(registrationData.hardwareId, registrationData.productName);

            if (license != null)
            { 
                if (license.registrationDate == string.Empty)
                {
                    license.registrationDate = DateTime.Now.ToString();
                    license.status = true;
                    license.trialPeriod = 96;

                    databaseService.UpdateLicense(license);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{DateTime.Now}] лицензия для проекта {license.productName} ({license.companyName}) активирована.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                if (computer == null)
                {
                    Computer newComputer = new Computer() {
                        hardwareId = registrationData.hardwareId,
                        productName = registrationData.productName,
                        activationDate = DateTime.Now.ToString(),
                        machineName = registrationData.machineName,
                        localUserName = registrationData.userName,
                        lastActivity = DateTime.Now.ToString(),
                        geolocation = ipInfo.City
                    };

                    computer = databaseService.CreateComputer(newComputer);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.Now}] добавлен компьютер {computer.localUserName} ({computer.geolocation}) для проекта {license.productName}.");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (computer.isBanned)
                {
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Компьютер {computer.localUserName} ({computer.geolocation}) находится в бане.");
                    Console.ForegroundColor = ConsoleColor.White;
                    await Clients.Caller.SendAsync("OnInitializationResponse", false, computer.hardwareId, 0);
                    Aborted(computer, Context);

                    return;
                }

                computer.isActive = true;
                computer.sessionId = Context.ConnectionId;
                computer.geolocation = ipInfo.City;
                databaseService.UpdateComputer(computer);

                databaseService.RemoveDoublers(license, computer);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] Компьютер {computer.localUserName} ({computer.geolocation}) успешно авторизован. Проект - {computer.productName}.");
                Console.ForegroundColor = ConsoleColor.White;

                if (!databaseService.IsAllowedComputer(computer, license))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.Now}] Компьютер {computer.localUserName} ({computer.geolocation}), скорее всего, не находится в списке разрешённых.");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (!Context.GetHttpContext()
                        .Connection
                        .RemoteIpAddress
                        .ToString().Contains("192.168."))
                    await mailSender.SendWarningAsync($"лицензия {license.productName} | подозрительная активность", $"В общем, какой-то хер с именем {computer.localUserName} из {computer.geolocation} с адресом {Context.GetHttpContext().Connection.RemoteIpAddress} попытался запустить приложение.\nПредлагаю посмотреть данные о лицензии {license.productName} и заблакировать его к хуям собачьим.");
                }

                await Clients.Caller.SendAsync("OnInitializationResponse", true, computer.hardwareId, databaseService.GetLicense(computer.productName).trialPeriod);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] отказ в доступе - обращение к несуществующей лицензии.");
                Console.ForegroundColor = ConsoleColor.White;

                await Clients.Caller.SendAsync("OnInitializationResponse", false, computer.hardwareId, 0);
                Aborted(computer, Context);
            }
        }

        public async Task OnValidationRequest(string hardwareId, string productName)
        {
            Computer comp = databaseService.GetComputer(hardwareId, productName);
            License lic = databaseService.GetLicense(productName);

            if (lic.status == true && comp.isBanned == false) await Clients.Caller.SendAsync("OnValidationResponse", true, comp.hardwareId);
            else await Clients.Caller.SendAsync("OnValidationResponse", false, comp.hardwareId);
        }

        public void Aborted(Computer computer, HubCallerContext context)
        {
            if (computer != null)
            {
                computer.isActive = false;
                computer.sessionId = string.Empty;
                computer.lastActivity = DateTime.Now.ToString();

                databaseService.UpdateComputer(computer);
                context.Abort();

                Console.WriteLine($"[{DateTime.Now}] {computer.localUserName}({computer.productName}) отключился.");
            }
        }
    }
}