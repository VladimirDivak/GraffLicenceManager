using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using GraffLicenceManager.DB;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;

namespace GraffLicenceManager.Hubs
{
    public class AdminHub : Hub
    {
        private readonly DatabaseService _databaseService;
        private readonly IHubContext<AdminHub> _hubContext;

        public AdminHub(DatabaseService service, IHubContext<AdminHub> hubContext)
        {
            _databaseService = service;
            _hubContext = hubContext;

            _databaseService.OnAddNewLicense += async license => await _hubContext.Clients.All.SendAsync("OnAddNewLicense", license);
            _databaseService.OnLicenseStatusChanged += async license => await _hubContext.Clients.All.SendAsync("OnLicenseStatusChanged", license);
            _databaseService.OnLicenseRemoved += async licenseRegDate => await _hubContext.Clients.All.SendAsync("OnLicenseRemoved", licenseRegDate);

            _databaseService.OnAddNewComputer += async computer => await _hubContext.Clients.All.SendAsync("OnAddNewComputer", computer);
            _databaseService.OnComputerStatusChanged += async computer => await _hubContext.Clients.All.SendAsync("OnComputerSatusChanged", computer);
            _databaseService.OnComputerRemoved += async computerRegDate => await _hubContext.Clients.All.SendAsync("OnComputerRemoved", computerRegDate);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task OnLicensesListRequest()
        {
            await Clients.Caller.SendAsync("OnLicensesListResponse", _databaseService.GetLicenses(), _databaseService.GetComputers());
        }

        public void OnComputerBanned(string hardwareId, bool banState)
        {
            var computer = _databaseService.GetComputer(hardwareId);
            computer.isBanned = banState;

            _databaseService.UpdateComputer(computer);
        }

        public void OnLicenseStatusChanged(string productName, bool licenseState)
        {
            var license = _databaseService.GetLicense(productName);
            license.status = licenseState;

            _databaseService.UpdateLicense(license);
        }

        public void OnAddNewLicense(License newLicense)
        {
            newLicense.registrationDate = DateTime.Now.ToString();
            _databaseService.CreateLicense(newLicense);
        }

        public void OnChangeLicense(License newLicense)
        {
            _databaseService.UpdateLicense(newLicense);
        }
    }
}