using CSF_PayACHServive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSF_PayACHServive
{
    public class ClsCore
    {

        Logs logs = new Logs();
        DataAccess da = new DataAccess();

        bool estado = false;
        bool error_interno = false;

        public string log(string mensaje)
        {
            Logs.Guarda_Log("Logs", mensaje);
            return "OK";
        }

        /// <summary>
        /// Proceso de transferencia, valida todos los elementos que hacen una cuenta valida
        /// </summary>
        /// <param name="JSONRequest"></param>
        /// <returns></returns>
        public string porcessRequest(string JSONRequest)
        {

            ClsCredencials credencials = new ClsCredencials();
            ClsStatus status = new ClsStatus();
            string transtring = string.Empty;
            string JSONResponce = string.Empty;
            bool exist = false;
            bool bloqued = false;
            bool accountExist = false;

            credencials.User = "ACHCoopsafa";
            credencials.pass = "C00psaF@1967$";

            try
            {
                var JsonRes = JsonConvert.DeserializeObject<ClsTransfer>(JSONRequest);
                if (string.IsNullOrEmpty(JSONRequest))
                {
                    status.StatusCode = "0100";
                    status.StatusMessage = "Internal Error";
                    status.SubStatusCode = "0101";
                    status.SubStatusMessage = "Request cannot be null or empty";

                    Responce(JsonRes, status, ref JSONResponce);
                    log(status.StatusCode + " " + status.StatusMessage + " " + status.SubStatusCode + " " + status.SubStatusMessage);
                    return JSONResponce;
                }

                if (JsonRes.typeTransaction.Equals("Transfer") || JsonRes.typeTransaction.Equals("account search"))
                {
                    if (JsonRes.credencials.User.Equals(credencials.User) && JsonRes.credencials.pass.Equals(credencials.pass))
                    {
                        accountExist = da.valida_esistencia_cuenta(JsonRes.accountNumber);
                        if (accountExist)
                        {
                            bloqued = da.valida_bloqueo(JsonRes.accountNumber);
                            if (bloqued)
                            {
                                status.StatusCode = "0000";
                                status.StatusMessage = "Rejected Request";
                                status.SubStatusCode = "0006";
                                status.SubStatusMessage = "Blocked Account";
                            }
                            else
                            {
                                exist = da.valida_cuenta(JsonRes.accountNumber);
                                if (exist)
                                {
                                    status.StatusCode = "0000";
                                    status.StatusMessage = "Rejected Request";
                                    status.SubStatusCode = "0004";
                                    status.SubStatusMessage = "Account Closed";
                                }
                                else
                                {
                                    status.StatusCode = "5000";
                                    status.StatusMessage = "Completed";
                                    status.SubStatusCode = "5001";
                                    status.SubStatusMessage = "Success";
                                    estado = true;
                                }
                            }
                        }
                        else
                        {
                            status.StatusCode = "0000";
                            status.StatusMessage = "Rejected Request";
                            status.SubStatusCode = "0005";
                            status.SubStatusMessage = "Nonexistent Account";
                            estado = false;
                        }
                    }
                    else
                    {
                        status.StatusCode = "1000";
                        status.StatusMessage = "Internal Error";
                        status.SubStatusCode = "1002";
                        status.SubStatusMessage = "Authentication Error";
                        estado = false;
                    }

                    Responce(JsonRes, status, ref JSONResponce);
                    return JSONResponce;
                }
                else
                {

                    if (JsonRes.typeTransaction.Equals("status operation"))
                    {
                        var JsonStatus = JsonConvert.DeserializeObject<ClsCoreStatusRequest>(JSONRequest);
                        if (JsonStatus.credencials.User.Equals(credencials.User) && JsonStatus.credencials.pass.Equals(credencials.pass))
                        {
                            bool transferExist = da.valida_esistencia_operation(Convert.ToInt32(JsonStatus.idTransaccion)); //falta metodo para verificar existencia 
                            if (transferExist)
                            {
                                status.StatusCode = "5000";
                                status.StatusMessage = "Completed";
                                status.SubStatusCode = "5001";
                                status.SubStatusMessage = "Success";
                                estado = true;
                            }
                            else
                            {
                                status.StatusCode = "1000";
                                status.StatusMessage = "Internal Error";
                                status.SubStatusCode = "1004";
                                status.SubStatusMessage = "non-existent transfer";
                            }
                        }
                        else
                        {
                            status.StatusCode = "1000";
                            status.StatusMessage = "Internal Error";
                            status.SubStatusCode = "1002";
                            status.SubStatusMessage = "Authentication Error";
                            estado = false;
                        }

                        ResponceStatus(JsonStatus, status, ref JSONResponce);
                        return JSONResponce;
                    }
                    else
                    {
                        status.StatusCode = "1000";
                        status.StatusMessage = "Internal Error";
                        status.SubStatusCode = "1003";
                        status.SubStatusMessage = "invalid operation";
                    }

                }
            }
            catch (Exception ex)
            {
                status.StatusCode = "1000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "1001";
                status.SubStatusMessage = "System Error";

                log(status.StatusCode + " " + status.StatusMessage + " " + status.SubStatusCode + " " + status.SubStatusMessage + " Error en JSON");
                error_interno = true;
                return JSONResponce;
            }
            return JSONResponce;
        }

        /// <summary>
        /// efectua los procesos necesarios para responder, en este apartado se hace el credito de la trasnferencia y el proceso contable 
        /// </summary>
        /// <param name="transfer"></param>
        /// <param name="status"></param>
        /// <param name="JSONResponse"></param>
        /// <param name="transctionType"></param>
        void Responce(ClsTransfer transfer, ClsStatus status, ref string JSONResponse, string transctionType = " ")
        {

            string oErr = string.Empty;
            ClsPersonalData personalData = new ClsPersonalData();
            transctionType = transfer.typeTransaction;
            bool cuenta_valida = da.valida_cuenta(transfer.accountNumber);
            int codigo_agencia;
            int codigo_empresa;
            int codigo_sub_aplicacion;

            try
            {
                DataTable personal = da.Personal_data(transfer.accountNumber);

                foreach (DataRow row in personal.Rows)
                {
                    personalData.nameRecived = row["NAME"].ToString();
                    personalData.idRecived = row["ID_NUMBER"].ToString();
                    personalData.idType = Convert.ToInt32(row["ID_TYPE"].ToString());
                    personalData.phone = row["PHONE_NUMBER"].ToString();
                    personalData.acountType = row["ACCOUNT_TYPE"].ToString();
                }

                DataTable dt = da.Data_Transaction(transfer.accountNumber);

                foreach (DataRow row in dt.Rows)
                {
                    codigo_agencia = Int32.Parse(dt.Rows[0]["CODIGO_AGENCIA"].ToString());
                    codigo_empresa = Int32.Parse(dt.Rows[0]["CODIGO_EMPRESA"].ToString());
                    codigo_sub_aplicacion = Int32.Parse(dt.Rows[0]["CODIGO_SUB_APLICACION"].ToString());
                }

                switch (transctionType)
                {
                    #region Transfer
                    case "Transfer":
                        if (transfer.idRecived.Equals(personalData.idRecived))
                        {
                            if (cuenta_valida || status.SubStatusMessage.Equals("Success"))
                            {
                                if (dt.Rows.Count > 0)
                                {
                                    codigo_agencia = Int32.Parse(dt.Rows[0]["CODIGO_AGENCIA"].ToString());
                                    codigo_empresa = Int32.Parse(dt.Rows[0]["CODIGO_EMPRESA"].ToString());
                                    codigo_sub_aplicacion = Int32.Parse(dt.Rows[0]["CODIGO_SUB_APLICACION"].ToString());

                                    bool insert = da.insert_firts(transfer.nameRecived, transfer.idRecived, transfer.nameTransffer, transfer.idSend,
                                                                    transfer.phone, transfer.typeID, transfer.amount, transfer.typeTransaction
                                                                    , transfer.codeBankRecived, transfer.codeBankSend, transfer.accountNumber, codigo_sub_aplicacion,
                                                                    codigo_agencia, status.StatusMessage, status.SubStatusMessage, transfer.currency_code
                                                                    );
                                    if (insert)
                                    {
                                        decimal comision = 20.00M;
                                        int codeOperation = da.obtener_codigo_operation(transfer.accountNumber, transfer.idRecived, transfer.phone, transfer.amount);
                                        bool exito = da.MCA_K_AHORROS(Convert.ToInt32(transfer.accountNumber), codigo_sub_aplicacion, codigo_agencia, codigo_empresa, transfer.amount, comision, codeOperation);

                                        if (exito)
                                        {
                                            DataTable dtTrans = da.inf_Transfer(codeOperation);
                                            string statusRespinse = string.Empty;
                                            string resultTrans = string.Empty;
                                            string descrpTrans = string.Empty;
                                            int idTrans = 0;
                                            int idOpe = 0;

                                            foreach (DataRow row in dtTrans.Rows)
                                            {
                                                idOpe = Convert.ToInt32(dtTrans.Rows[0]["OPERATION_CODE"].ToString());
                                                statusRespinse = dtTrans.Rows[0]["STATUS_REQUEST"].ToString();
                                                resultTrans = dtTrans.Rows[0]["RESULT_TRANSCTION"].ToString();
                                                descrpTrans = dtTrans.Rows[0]["DESCRIPTION_STATUS"].ToString();
                                                idTrans = Convert.ToInt32(dtTrans.Rows[0]["TRANSACCTION_ID"].ToString());
                                            }

                                            status.StatusCode = "5000";
                                            status.StatusMessage = "Completed";
                                            status.SubStatusCode = "5001";
                                            status.SubStatusMessage = "Success";

                                            var responce = new ClsTransferResponse
                                            {
                                                codTransaction = idOpe,
                                                statusRequest = statusRespinse,
                                                result = resultTrans,
                                                descriptionStatus = descrpTrans,
                                                transactionId = idTrans,
                                                statusResponse = new ClsStatus()
                                                {
                                                    StatusCode = status.StatusCode,
                                                    StatusMessage = status.StatusMessage,
                                                    SubStatusCode = status.SubStatusCode,
                                                    SubStatusMessage = status.SubStatusMessage
                                                }
                                            };
                                            log(responce.transactionId + " --> " + responce.result + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
                                            da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                                            JSONResponse = System.Text.Json.JsonSerializer.Serialize(responce);
                                            p_send_mail(transfer.mail, transfer.amount, transfer.nameRecived, transfer.nameTransffer);
                                        }
                                        else
                                        {
                                            DataTable dtTrans = da.inf_Transfer(codeOperation);
                                            int idOpe = 0;

                                            foreach (DataRow row in dtTrans.Rows)
                                            {
                                                idOpe = Convert.ToInt32(dtTrans.Rows[0]["OPERATION_CODE"].ToString());
                                            }

                                            var stErr = Error(status, idOpe);
                                            JSONResponse = System.Text.Json.JsonSerializer.Serialize(stErr);
                                        }
                                    }
                                    else
                                    {
                                        var stErr = Error(status, 0);
                                        JSONResponse = System.Text.Json.JsonSerializer.Serialize(stErr);
                                    }
                                }
                                else
                                {
                                    var stErr = Error(status, 0);
                                    JSONResponse = System.Text.Json.JsonSerializer.Serialize(stErr);
                                }
                            }
                            else
                            {
                                var stErr = Error(status, 0);
                                JSONResponse = System.Text.Json.JsonSerializer.Serialize(stErr);
                            }
                        }
                        else
                        {

                            if (string.IsNullOrEmpty(status.StatusCode))
                            {
                                status.StatusCode = "0000";
                                status.StatusMessage = "Internal Error";
                                status.SubStatusCode = "1010";
                                status.SubStatusMessage = "information does not match";
                            }

                            log(status.StatusCode + " " + status.StatusMessage + " " + status.SubStatusCode + " " + status.SubStatusMessage + " Error en Respuesta");
                            da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                            var stErr = Error(status, 0);
                            JSONResponse = System.Text.Json.JsonSerializer.Serialize(stErr);
                        }
                        break;
                    #endregion

                    #region account search
                    case "account search":
                        string tipoCuenta = string.Empty;
                        if (dt.Rows.Count > 0)
                        {
                            codigo_sub_aplicacion = Int32.Parse(dt.Rows[0]["CODIGO_SUB_APLICACION"].ToString());

                            if (codigo_sub_aplicacion == 111)
                            {
                                tipoCuenta = "Ahorro Dolares";
                            }
                            else
                            {
                                if (codigo_sub_aplicacion == 102)
                                {
                                    tipoCuenta = "Ahorro Lempiras";
                                }
                                else
                                {
                                    tipoCuenta = "Tipo de cuenta no valida";
                                    log( tipoCuenta + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
                                    da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                                    var ErroSearch = ErrorBusqueda(status, tipoCuenta);
                                    JSONResponse = System.Text.Json.JsonSerializer.Serialize(ErroSearch);
                                }
                            }

                            var accounData = new ClsAcountData
                            {
                                tipeAcount = tipoCuenta,
                                personalInfo = new ClsPersonalData
                                {
                                    nameRecived = personalData.nameRecived,
                                    idRecived = personalData.idRecived,
                                    idType = personalData.idType,
                                    acountType = personalData.acountType,
                                    phone = personalData.phone
                                },
                                statusResponse = new ClsStatus
                                {
                                    StatusCode = status.StatusCode,
                                    StatusMessage = status.StatusMessage,
                                    SubStatusCode = status.SubStatusCode,
                                    SubStatusMessage = status.SubStatusMessage
                                }
                            };
                            log(accounData.personalInfo.nameRecived + " --> " + accounData.tipeAcount + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
                            da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                            JSONResponse = System.Text.Json.JsonSerializer.Serialize(accounData);
                        }
                        else
                        {
                            var ErroSearch = ErrorBusqueda(status, "No valida");
                            JSONResponse = System.Text.Json.JsonSerializer.Serialize(ErroSearch);
                        }
                        break;
                    #endregion

                    default:

                        status.StatusCode = "1000";
                        status.StatusMessage = "Internal Error";
                        status.SubStatusCode = "1012";
                        status.SubStatusMessage = "Invalid Operation";

                        var defaultR = new ClsTransferResponse
                        {
                            codTransaction = 0,
                            statusRequest = "NONE",
                            result = "NONE",
                            descriptionStatus = "NONE",
                            transactionId = 0,
                            statusResponse = new ClsStatus()
                            {
                                StatusCode = status.StatusCode,
                                StatusMessage = status.StatusMessage,
                                SubStatusCode = status.SubStatusCode,
                                SubStatusMessage = status.SubStatusMessage
                            }
                        };
                        log(status.StatusMessage + " " + status.SubStatusMessage + " Error desde la transaccion");
                        da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                        JSONResponse = System.Text.Json.JsonSerializer.Serialize(defaultR);
                        break;
                }
            }
            catch (Exception ex)
            {
                status.StatusCode = "1000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "1001";
                status.SubStatusMessage = "System Error";

                var responce = new ClsTransferResponse
                {
                    statusRequest = "Error",
                    result = "Bad",
                    descriptionStatus = "Hubo un error al efectuar la transaccion",
                    transactionId = 0,
                    statusResponse = new ClsStatus()
                    {
                        StatusCode = status.StatusCode,
                        StatusMessage = status.StatusMessage,
                        SubStatusCode = status.SubStatusCode,
                        SubStatusMessage = status.SubStatusMessage
                    }
                };
                log(status.StatusMessage + " " + status.SubStatusMessage + responce.descriptionStatus);
                da.insert_err(status.StatusMessage, status.SubStatusMessage, transfer.accountNumber, transfer.amount);
                JSONResponse = System.Text.Json.JsonSerializer.Serialize(responce);
            }
        }

        /// <summary>
        /// trae la informacion necesaria para toda operacion procesada, erronea o correcta
        /// </summary>
        /// <param name="statusRequest"></param>
        /// <param name="status"></param>
        /// <param name="JSONResponse"></param>
        void ResponceStatus(ClsCoreStatusRequest statusRequest, ClsStatus status, ref string JSONResponse)
        {
            try
            {
                DataTable dtStatus = da.inf_Status(Convert.ToInt32(statusRequest.idTransaccion));

                if (dtStatus.Rows.Count > 0)
                {
                    var statusResult = new ClsStatusTransfer
                    {
                        idTransaccion = dtStatus.Rows[0]["TRANSACCTION_ID"].ToString(),
                        dateTransaction = Convert.ToDateTime(dtStatus.Rows[0]["DATE_TRANSACTION"].ToString()),
                        currency = dtStatus.Rows[0]["CURRENCY_CODE"].ToString(),
                        bankCodeSend = dtStatus.Rows[0]["CODE_BANK_SEND"].ToString(),
                        accountNumber = dtStatus.Rows[0]["ACOUNT_NUMBER"].ToString(),
                        amount = Convert.ToDecimal(dtStatus.Rows[0]["AMOUNT"].ToString()),
                        status = dtStatus.Rows[0]["RESULT_TRANSCTION"].ToString(),
                        statusDescription = dtStatus.Rows[0]["DESCRIPTION_STATUS"].ToString(),
                        statusResponse = new ClsStatus()
                        {
                            StatusCode = status.StatusCode,
                            StatusMessage = status.StatusMessage,
                            SubStatusCode = status.SubStatusCode,
                            SubStatusMessage = status.SubStatusMessage
                        }
                    };
                    log(statusResult.accountNumber + " --> "+ statusResult.amount + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
                    da.insert_err(status.StatusMessage, status.SubStatusMessage, "NA", 0.0M);
                    JSONResponse = System.Text.Json.JsonSerializer.Serialize(statusResult);
                }
                else
                {
                    status.StatusCode = "1000";
                    status.StatusMessage = "Internal Error";
                    status.SubStatusCode = "1013";
                    status.SubStatusMessage = "Problems validating the operation";

                    var responce = new ClsStatusTransfer
                    {
                        idTransaccion = "NONE",
                        dateTransaction = DateTime.Now,
                        currency = "NONE",
                        bankCodeSend = "NONE",
                        accountNumber = "NONE",
                        amount = 0.0M,
                        status = "NONE",
                        statusDescription = "NONE",
                        statusResponse = new ClsStatus()
                        {
                            StatusCode = status.StatusCode,
                            StatusMessage = status.StatusMessage,
                            SubStatusCode = status.SubStatusCode,
                            SubStatusMessage = status.SubStatusMessage
                        }
                    };
                    log(status.StatusMessage + " " + status.SubStatusMessage);
                    da.insert_err(status.StatusMessage, status.SubStatusMessage, "N/A", 0.0M);
                    JSONResponse = System.Text.Json.JsonSerializer.Serialize(responce);
                }
            }
            catch (Exception ex)
            {
                status.StatusCode = "1000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "1001";
                status.SubStatusMessage = "System Error";

                var responce = new ClsStatusTransfer
                {
                    idTransaccion = "NONE",
                    dateTransaction = DateTime.Now,
                    currency = "NONE",
                    bankCodeSend = "NONE",
                    accountNumber = "NONE",
                    amount = 0.0M,
                    status = "NONE",
                    statusDescription = "NONE",
                    statusResponse = new ClsStatus()
                    {
                        StatusCode = status.StatusCode,
                        StatusMessage = status.StatusMessage,
                        SubStatusCode = status.SubStatusCode,
                        SubStatusMessage = status.SubStatusMessage
                    }
                };
                log(status.StatusMessage + " " + status.SubStatusMessage);
                da.insert_err(status.StatusMessage, status.SubStatusMessage, "N/A", 0.0M);
                JSONResponse = System.Text.Json.JsonSerializer.Serialize(responce);
            }
        }

        /// <summary>
        /// trae la lista de errores en las transacciones
        /// </summary>
        /// <param name="status"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public ClsTransferResponse Error(ClsStatus status, int operation)
        {

            if (string.IsNullOrEmpty(status.StatusCode) || status.StatusCode.Equals("5000"))
            {
                status.StatusCode = "0000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "0008";
                status.SubStatusMessage = "Rejected by internal validation";
            }

            var responce = new ClsTransferResponse
            {
                codTransaction = operation,
                statusRequest = "Error",
                result = "Bad",
                descriptionStatus = "Hubo un error al efectuar la transaccion",
                transactionId = 0,
                statusResponse = new ClsStatus()
                {
                    StatusCode = status.StatusCode,
                    StatusMessage = status.StatusMessage,
                    SubStatusCode = status.SubStatusCode,
                    SubStatusMessage = status.SubStatusMessage
                }
            };
            log(operation + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
            da.insert_err(status.StatusMessage, status.SubStatusMessage, "N/A", 0.0M);
            return responce;
        }

        /// <summary>
        /// trae la lista de errores de busqueda
        /// </summary>
        /// <param name="status"></param>
        /// <param name="_tipoCuenta"></param>
        /// <returns></returns>
        public ClsAcountData ErrorBusqueda(ClsStatus status, string _tipoCuenta)
        {
            if (string.IsNullOrEmpty(status.StatusCode) || status.StatusCode.Equals("5000"))
            {
                status.StatusCode = "0000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "0011";
                status.SubStatusMessage = "problems validating account";
            }

            var accounData = new ClsAcountData
            {
                tipeAcount = _tipoCuenta,
                personalInfo = new ClsPersonalData
                {
                    nameRecived = "None",
                    idRecived = "None",
                    idType = 0,
                    acountType = "None",
                    phone = "None",
                },
                statusResponse = new ClsStatus
                {
                    StatusCode = status.StatusCode,
                    StatusMessage = status.StatusMessage,
                    SubStatusCode = status.SubStatusCode,
                    SubStatusMessage = status.SubStatusMessage
                }
            };

            return accounData;
            log (status.StatusMessage + " " + status.SubStatusMessage);
            da.insert_err(status.StatusMessage, status.SubStatusMessage, "N/A", 0.0M);
            log(accounData.personalInfo.nameRecived + " --> " + status.StatusMessage + " " + status.SubStatusMessage);
        }

        /// <summary>
        /// envio de correos de notificacion 
        /// </summary>
        /// <param name="destino"></param>
        /// <param name="monto"></param>
        /// <param name="recive"></param>
        /// <param name="envia"></param>
        public void p_send_mail(string destino, decimal monto, string recive, string envia) {
            try {
                MailMessage mailMessage = new MailMessage();

                string correo = "tucooperativa@sagradafamilia.hn";
                string clave = "%XkGJa=5S+=F6@+6";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                mailMessage.From = new MailAddress(correo);
                mailMessage.To.Add(destino);
                mailMessage.Subject = "Transferencia ACH";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = "<HTML><BODY> Estimado " + recive + ".\n\n " +
                    "\n\n\n\n" +
                    "Se le informa que ha recivido una transferencia ACH de " + monto + " LPS por parte de " +envia
                                    +"</BODY></HTML>";

                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(mailMessage.Body, null, "text/html");
                mailMessage.AlternateViews.Add(alternateView);

                SmtpClient client = new SmtpClient("correo.sagradafamilia.hn", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(correo, clave);

                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };

                client.Send(mailMessage);
                mailMessage.Dispose();
                mailMessage = null;
            }
            catch (Exception e)
            {
                log("error al enviar el correo a "+ destino +" --> "+e.Message);
                throw new Exception();
            }
        }

    }
}
