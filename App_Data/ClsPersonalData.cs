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
        public string nameRecived { get; set; }
        public string idRecived { get; set; }
        public int idType { get; set; }
        public string acountType { get; set; }
        public string phone { get; set; }
    }
}
