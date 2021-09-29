using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Services;

namespace CSF_PayACHServive
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class CSF_PayACHServive : ICSF_PayACHServive
    {
        public string GetData(string JSONRequest)
        {
            ClsCore cre = new ClsCore();
            return cre.porcessRequest(JSONRequest);
        }
    }
}
