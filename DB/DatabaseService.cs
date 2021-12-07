using System;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;

namespace GraffLicenceManager.DB {
    public class DatabaseService {
        private readonly IMongoCollection<License> _licenses;
        private readonly IMongoCollection<Computer> _computers;

        public DatabaseService() {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("GraffInteractive");

            _licenses = database.GetCollection<License>("LicenceData");
            _computers = database.GetCollection<Computer>("ComputerData");
        }

        public List<License> GetLicenses() => _licenses.Find(x => true).ToList();
        public List<Computer> GetComputers() => _computers.Find(x => true).ToList();
        public List<Computer> GetComputers(string productName) => _computers.Find(x => x.productName == productName).ToList();

        public License GetLicense(string projectName) => _licenses.Find(licence =>
            licence.productName == projectName)
            .FirstOrDefault();
        public Computer GetComputer(string hardwareId) => _computers
            .Find(computer => computer.hardwareId == hardwareId)
            .First();

        public License GetLicenseByRegistrationData(RegistrationData registrationData) {
            var data = _licenses.Find(licence =>
            licence.companyName == registrationData.companyName &&
            licence.productName == registrationData.productName)
            .FirstOrDefault();

            if(data == null) return null;
            else return data;
        }
        
        public License CreateLicense(License licence) {
            _licenses.InsertOne(licence);
            return licence;
        }

        public Computer CreateComputer(Computer computer) {
            _computers.InsertOne(computer);
            return computer;
        }

        public void UpdateLicense(License licence) {
            _licenses.DeleteOne(lic => lic.companyName == licence.companyName && lic.productName == licence.productName);
            _licenses.InsertOne(licence);
        }
        public void UpdateComputer(Computer computer) {
            _computers.DeleteOne(comp => comp.hardwareId == computer.hardwareId);
            _computers.InsertOne(computer);
        }

        public void RemoveLicense(License licence) => _licenses.DeleteOne(lic => lic.id == licence.id);
        public void RemoveLicense(string projectName) => _licenses.DeleteOne(lic => lic.productName == projectName);
        public void RemoveComputer(Computer computer) => _computers.DeleteOne(comp => comp.Equals(computer));
        public void RemoveComputer(string hardwareID) => _computers.DeleteOne(comp => comp.hardwareId == hardwareID);
        public void RemoveComputers(string product) => _computers.DeleteMany(x => x.productName == product);
    }
}