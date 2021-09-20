using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsTransfer
    {
        public string typeTransaction { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string noID { get; set; }
        public string accountNumber { get; set; }
        public decimal ammount { get; set; }
        public string bankCode { get; set; }
        public string concept { get; set; }
        public string nameTransffer { get; set; }
    }
}
