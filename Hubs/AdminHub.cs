using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using GraffLicenceManager.DB;
using Newtonsoft.Json;

namespace GraffLicenceManager.Hubs
{
    public class AdminHub : Hub
    {
        private readonly DatabaseService databaseService;
        private readonly string password = "ffraG";

        public AdminHub(DatabaseService service)
        {
            databaseService = service;
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
            await Clients.Caller.SendAsync("OnLicensesListResponse", databaseService.GetLicenses(), databaseService.GetComputers());
        }
    }
}