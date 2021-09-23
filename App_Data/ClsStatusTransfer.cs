using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsStatusTransfer
    {
        public string idTransaccion { get; set; }
        public DateTime dateTransaction { get; set; }
        public string currency { get; set; }
        public string participanteIndirecto { get; set; }
        public string bankCodeSend { get; set; }
        public string accountNumber { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public string statusDescription { get; set; }
        public ClsStatus statusResponse { get; set; }
    }
}
