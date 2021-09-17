using CSF_PayACHServive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive
{
    public class ClsCore {

        Logs logs = new Logs();
        DataAccess da = new DataAccess();


        public string log(string mensaje)
        {
            Logs.Guarda_Log("Logs", mensaje);
            return "OK";
        }

    }
}
