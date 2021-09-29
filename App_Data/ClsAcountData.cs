using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive 
{
    public class ClsAcountData
    {
        public string tipeAcount { get; set; }
        public ClsPersonalData personalInfo { get; set; }
        public ClsStatus statusResponse { get; set; }
    }
}
