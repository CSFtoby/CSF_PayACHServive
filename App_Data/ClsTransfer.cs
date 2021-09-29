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
        public ClsCredencials credencials { get; set; }
        public string idRecived { get; set; }
        public int typeID { get; set; }
        public string idSend { get; set; }
        public string phone { get; set; }
        public string nameRecived { get; set; }
        public string codeBankRecived { get; set; }
        public string codeBankSend { get; set; }
        public string accountNumber { get; set; }
        public decimal amount { get; set; }
        public string concept { get; set; }
        public string nameTransffer { get; set; }
        public char currency_code { get; set; }
    }
}
