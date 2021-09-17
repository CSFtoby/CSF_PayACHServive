using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsPersonalData
    {
        public string account_number { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string status_account { get; set; }
        public string id_number { get; set; }
        public string id_type { get; set; }
        public string addr { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country_name { get; set; }
        public string country_code { get; set; }
        public string phone_number { get; set; }
        public char sex { get; set; }
        public string date_of_birth { get; set; }
    }
}
