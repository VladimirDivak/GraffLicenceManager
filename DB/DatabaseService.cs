﻿using System;
using System.Linq;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;

namespace GraffLicenceManager.DB
{
    public class DatabaseService
    {
        public DatabaseService()
        {
            var client = new MongoClient("mongodb://GRAFF:G3432664499@192.168.1.163:27017");
            var database = client.GetDatabase("license_manager");

            _licenses = database.GetCollection<License>("license_data");
            _computers = database.GetCollection<Computer>("computer_data");
            _admins = database.GetCollection<Admin>("admins");

            var computers = GetComputers();
            foreach (var computer in computers)
            {
                if(computer.isActive)
                {
                    computer.isActive = false;
                    UpdateComputer(computer);
                }
            }

            var licenses = GetLicenses();
            foreach(var license in licenses)
            {
                var licComputers = GetComputers(license.productName);
                foreach (var computer in licComputers)
                {
                    RemoveDoublers(license, computer);
                }
            }
        }

        public event Action<Computer> OnAddNewComputer;
        public event Action<Computer> OnComputerStatusChanged;
        public event Action<string> OnComputerRemoved;

        public event Action<License> OnAddNewLicense;
        public event Action<License> OnLicenseStatusChanged;
        public event Action<string> OnLicenseRemoved;

        private readonly IMongoCollection<License> _licenses;
        private readonly IMongoCollection<Computer> _computers;
        private readonly IMongoCollection<Admin> _admins;

        public List<License> GetLicenses() => _licenses.Find(x => true).ToList();
        public List<Computer> GetComputers() => _computers.Find(x => true).ToList();
        public List<Computer> GetComputers(string productName) => _computers.Find(x => x.productName == productName).ToList();
        public License GetLicense(string projectName) => _licenses.Find(licence =>
            licence.productName == projectName)
            .FirstOrDefault();
        public Computer GetComputer(string hardwareId) => _computers
            .Find(computer => computer.hardwareId == hardwareId)
            .First();
        public Computer GetComputer(string hardwareId, string productName)
        {
            var computer = _computers.Find(computer => computer.hardwareId == hardwareId && computer.productName == productName)
                .ToList();

            if(computer.Count == 0) return null;
            else return computer.FirstOrDefault();
        }
        public License GetLicenseByRegistrationData(RegistrationData registrationData)
        {
            var data = _licenses.Find(licence =>
            licence.companyName == registrationData.companyName &&
            licence.productName == registrationData.productName)
            .FirstOrDefault();

            if(data == null) return null;
            else return data;
        }
        public License CreateLicense(License licence)
        {
            _licenses.InsertOne(licence);
            OnAddNewLicense?.Invoke(licence);

            return licence;
        }
        public Computer CreateComputer(Computer computer)
        {
            _computers.InsertOne(computer);
            OnAddNewComputer?.Invoke(computer);

            return computer;
        }
        public bool GetAdminValidation(string login, string password)
        {
            Admin admin = _admins.Find(x => x.login == login && x.password == password)
                .ToList()
                .LastOrDefault();

            if (admin != null) return true;
            else return false;
        }
        public void UpdateLicense(License license)
        {
            _licenses.DeleteOne(lic => lic.id == license.id);
            _licenses.InsertOne(license);

            OnLicenseStatusChanged?.Invoke(license);
        }
        public void UpdateComputer(Computer computer)
        {
            _computers.DeleteOne(comp => comp.hardwareId == computer.hardwareId);
            _computers.InsertOne(computer);

            OnComputerStatusChanged?.Invoke(GetComputer(computer.hardwareId, computer.productName));
        }
        public void RemoveLicense(License license)
        {
            _licenses.DeleteOne(lic => lic.id == license.id);
            OnLicenseRemoved?.Invoke(license.registrationDate);
        }
        public void RemoveLicense(string projectName)
        {
            OnLicenseRemoved?.Invoke(GetLicense(projectName).registrationDate);
            _licenses.DeleteOne(lic => lic.productName == projectName);
        }
        public void RemoveComputer(Computer computer)
        {
            OnComputerRemoved?.Invoke(computer.activationDate);
            _computers.DeleteOne(x => x.id == computer.id);
        }
        public void RemoveComputer(string hardwareID)
        {
            OnComputerRemoved?.Invoke(GetComputer(hardwareID).activationDate);
            _computers.DeleteOne(comp => comp.hardwareId == hardwareID);
        }
        public void RemoveComputers(string product) => _computers.DeleteMany(x => x.productName == product);
        public bool IsAllowedComputer(Computer computer, License license)
        {
            if (computer.isBanned) return true;

            var computers = GetComputers(license.productName);

            if (computers.Count <= license.licensesCounter) return true;
            if (computers.Count == 0) return true;

            var lastAllowedActivationDate = DateFormater.GetParseDate(computers[license.licensesCounter - 1].activationDate);
            return DateFormater.GetParseDate(computer.activationDate) < lastAllowedActivationDate;
        }
        public void RemoveDoublers(License license, Computer computer)
        {
            var computers = GetComputers(license.productName).Where(x => x.localUserName == computer.localUserName && x.machineName == computer.machineName).ToList();
            if (computers.Count > 1)
            {
                foreach (var doubler in computers)
                {
                    if (doubler.Equals(computer)) continue;
                    RemoveComputer(doubler.hardwareId);
                }
            }
        }
    }
}