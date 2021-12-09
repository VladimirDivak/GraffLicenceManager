using System;

namespace GraffLicenceManager.DB {

    [Serializable]
    public struct RegistrationData {
        public string   companyName   { get; set; }
        public string   productName   { get; set; }
        public string   hardwareId    { get; set; }
        public string   userName      { get; set; }
        public string   machineName   { get; set; }
    }
}