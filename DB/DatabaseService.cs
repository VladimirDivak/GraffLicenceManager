using System;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;

namespace GraffLicenceManager.DB {
    public class DatabaseService {
        public event Action<Computer> OnAddNewComputer;
        public event Action<Computer> OnComputerStatusChanged;
        public event Action<string> OnComputerRemoved;

        public event Action<License> OnAddNewLicense;
        public event Action<License> OnLicenseStatusChanged;
        public event Action<string> OnLicenseRemoved;

        private readonly IMongoCollection<License> _licenses;
        private readonly IMongoCollection<Computer> _computers;

        public DatabaseService() {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("GraffInteractive");

            _licenses = database.GetCollection<License>("LicenseDataNew");
            _computers = database.GetCollection<Computer>("ComputerDataNew");
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
            Console.WriteLine("database: сделана новая лицензия");
            OnAddNewLicense?.Invoke(licence);

            return licence;
        }

        public Computer CreateComputer(Computer computer) {
            _computers.InsertOne(computer);
            OnAddNewComputer?.Invoke(computer);

            return computer;
        }

        public void UpdateLicense(License licence) {
            _licenses.DeleteOne(lic => lic.companyName == licence.companyName && lic.productName == licence.productName);
            _licenses.InsertOne(licence);

            OnLicenseStatusChanged?.Invoke(licence);
        }

        public void UpdateComputer(Computer computer) {
            _computers.DeleteOne(comp => comp.hardwareId == computer.hardwareId);
            _computers.InsertOne(computer);

            OnComputerStatusChanged?.Invoke(computer);
        }

        public void RemoveLicense(License licence) {
            _licenses.DeleteOne(lic => lic.id == licence.id);
            OnLicenseRemoved?.Invoke(licence.registrationDate);
        }

        public void RemoveLicense(string projectName) {
            OnLicenseRemoved?.Invoke(GetLicense(projectName).registrationDate);
            _licenses.DeleteOne(lic => lic.productName == projectName);
        }
        public void RemoveComputer(Computer computer) {
            OnComputerRemoved?.Invoke(computer.activationDate);
            _computers.DeleteOne(comp => comp.Equals(computer));
        }
        public void RemoveComputer(string hardwareID) {
            OnComputerRemoved?.Invoke(GetComputer(hardwareID).activationDate);
            _computers.DeleteOne(comp => comp.hardwareId == hardwareID);
        }
        public void RemoveComputers(string product) => _computers.DeleteMany(x => x.productName == product);
    }
}