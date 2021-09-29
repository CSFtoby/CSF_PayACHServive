using CSF_PayACHServive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSF_PayACHServive
{
    public class ClsCoreStatusRequest
    {
        public string typeTransaction { get; set; }
        public ClsCredencials credencials { get; set; }
        public string idTransaccion { get; set; }
    }
}
