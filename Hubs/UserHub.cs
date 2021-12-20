using System;
using IpData;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Threading.Tasks;
using GraffLicenceManager.DB;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace GraffLicenceManager.Hubs {
    public class UserHub : Hub {
        private readonly DatabaseService databaseService;
        private readonly IpDataClient ipDataClient;

        public UserHub(DatabaseService service) {
            databaseService = service;
            ipDataClient = new IpDataClient("a6162964e4404f14e89accb08d97fe2ec852e20014ffa5de62758df9");
        }

        public override async Task OnConnectedAsync() {
            var ip = "212.220.216.185";
            var ipInfo = await ipDataClient.Lookup(ip.ToString());

            Console.WriteLine($"[{DateTime.Now}] клиент {ip}({ipInfo.City}) появился в сети.");

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception) {
            Computer computer = databaseService.GetComputers()
                .ToList()
                .Find(x => x.sessionId == Context.ConnectionId);

            if (computer != null) {
                computer.isActive = false;
                computer.sessionId = string.Empty;
                computer.lastActivity = DateTime.Now.ToString();

                databaseService.UpdateComputer(computer);

                Console.WriteLine($"[{DateTime.Now}] {computer.localUserName}({computer.productName}) отключился.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task OnInitializationRequest(RegistrationData registrationData) {
            var ip = "212.220.216.185";
            var ipInfo = await ipDataClient.Lookup(ip.ToString());

            var licence = databaseService.GetLicense(registrationData.productName);
            var computer = databaseService.GetComputers()
                .Find(x => x.productName == registrationData.productName &&
                x.hardwareId == registrationData.hardwareId);

            if (licence != null) { 
                if (licence.registrationDate == string.Empty) {
                    licence.registrationDate = DateTime.Now.ToString();
                    licence.status = true;
                    licence.trialPeriod = 7;

                    databaseService.UpdateLicense(licence);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[{DateTime.Now}] лицензия для проекта {licence.productName} ({licence.companyName}) активирована.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                if (computer == null) {
                    if (databaseService.GetComputers()
                        .Where(x => x.productName == licence.productName)
                        .Count() < licence.licensesCounter) {

                        Computer newComputer = new Computer() {
                            hardwareId = registrationData.hardwareId,
                            productName = registrationData.productName,
                            activationDate = DateTime.Now.ToString(),
                            machineName = registrationData.machineName,
                            localUserName = registrationData.userName,
                            lastActivity = DateTime.Now.ToString(),
                            geolocation = ipInfo.City
                        };

                        computer = newComputer;
                        computer = databaseService.CreateComputer(newComputer);
                        computer = databaseService.GetComputer(newComputer.hardwareId);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[{DateTime.Now}] добавлен компьютер {computer.localUserName} ({computer.geolocation}) для проекта {licence.productName}.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{DateTime.Now}] отказ в доступе - нет слотов в лицении для {licence.productName}.");
                        Console.ForegroundColor = ConsoleColor.White;

                        await Clients.Caller.SendAsync("OnInitializationResponse", false);
                        Aborted(computer, Context);
                        return;
                    }
                }

                if (licence.licensesCounter == databaseService
                    .GetComputers()
                    .Count(x => x.productName == licence.productName)) {

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[{DateTime.Now}] закончились слоты для лицензии {licence.productName}({licence.companyName}).");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (computer.isBanned) {
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Компьютер {computer.localUserName} ({computer.geolocation}) находится в бане.");
                    Console.ForegroundColor = ConsoleColor.White;
                    await Clients.Caller.SendAsync("OnInitializationResponse", false);
                    Aborted(computer, Context);

                    return;
                }

                computer.isActive = true;
                computer.sessionId = Context.ConnectionId;
                computer.geolocation = ipInfo.City;
                databaseService.UpdateComputer(computer);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now}] Компьютер {computer.localUserName} ({computer.geolocation}) успешно авторизован. Проект - {computer.productName}.");
                Console.ForegroundColor = ConsoleColor.White;
                await Clients.Caller.SendAsync("OnInitializationResponse", true);
            }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] отказ в доступе - обращение к несуществующей лицензии.");
                Console.ForegroundColor = ConsoleColor.White;

                await Clients.Caller.SendAsync("OnInitializationResponse", false);
                Aborted(computer, Context);
            }
        }
        public async Task OnValidationRequest(string hardwareId) {
            Computer comp = databaseService.GetComputer(hardwareId);
            License lic = databaseService.GetLicense(databaseService.GetComputer(hardwareId).productName);

            Console.WriteLine($"[{DateTime.Now}] {comp.localUserName} просит состояние лицензии {lic.productName}...");

            if (lic.status == true && comp.isBanned == false) await Clients.Caller.SendAsync("OnValidationResponse", true);
            else await Clients.Caller.SendAsync("OnValidationResponse", false);
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