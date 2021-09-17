using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CSF_PayACHServive
{
    public class DataAccess {

        public string cadenaConexionOracle = "";

        public DataAccess() {
            cadenaConexionOracle = ConfigurationManager.ConnectionStrings["DbContext"].ConnectionString;
        }

    }
}
