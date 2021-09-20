using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsStatus
    {
        public string StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string SubStatusCode { get; set; }
        public string SubStatusMessage { get; set; }
    }
}
