using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;

namespace CSF_PayACHServive
{
    /// <summary>
    /// GetData() reciibe un JSON diferenciado por cualquiera de las tres transacciones
    /// a.- Transfer: metodo utilizado para efectuar las transferencias 
    /// b.- account search: busqueda y validacion de la cuenta ingresada 
    /// c.- status operation: valida el status de una transaccion ya efectuada
    /// </summary>
    /// <param name="JSONRequest"></param>
    /// <returns></returns>
    [ServiceContract]
    public interface ICSF_PayACHServive
    {

        [OperationContract]
        string GetData(string JSONRequest);
    }
}
