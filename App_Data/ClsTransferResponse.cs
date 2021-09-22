using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsTransferResponse
    {
        public string statusRequest { get; set; }
        public string result { get; set; }
        public string descriptionStatus { get; set; }
        public int transactionId { get; set; }
        public ClsStatus statusResponse { get; set; }
    }
}
