using CSF_PayACHServive;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public class ClsCore {

        Logs logs = new Logs();
        DataAccess da = new DataAccess();

        bool estado = false;
        bool error_interno = false;

        public string log(string mensaje)
        {
            Logs.Guarda_Log("Logs", mensaje);
            return "OK";
        }

        public string porcessRequest(string JSONRequest) {

            ClsCredencials credencials = new ClsCredencials();
            ClsStatus status = new ClsStatus();
            ClsTransfer transfer = new ClsTransfer();
            string transtring = string.Empty;
            string JSONResponce = string.Empty;
            bool exist = false;
            bool bloqued = false;
            bool accountExist = false;

            credencials.User = "ACHCoopsafa";
            credencials.pass = "C00psaF@1967$";

            if (string.IsNullOrEmpty(JSONRequest)) {
                status.StatusCode = "0100";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "0101";
                status.SubStatusMessage = "Request cannot be null or empty";

                Responce(transfer, status, ref JSONResponce);
                return JSONResponce;
            }

            try {
                var JsonRes = JsonConvert.DeserializeObject<ClsTransfer>(JSONRequest);

                if (JsonRes.typeTransaction.Equals("Transfer") || JsonRes.typeTransaction.Equals("account search"))
                {
                    if (JsonRes.user.Equals(credencials.User) && JsonRes.pass.Equals(credencials.pass))
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
                }
                else {
                   
                    if (JsonRes.typeTransaction.Equals("status operation"))
                    {
                        bool transferExist = false; //falta metodo para verificar existencia 
                        if (JsonRes.user.Equals(credencials.User) && JsonRes.pass.Equals(credencials.pass))
                        {
                            if (transferExist)
                            {
                                status.StatusCode = "5000";
                                status.StatusMessage = "Completed";
                                status.SubStatusCode = "5001";
                                status.SubStatusMessage = "Success";
                                estado = true;
                            }
                            else {
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
                    }
                    else {
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

                error_interno = true;

                //bool save = da.insert_Transfer_fist(transaction.chanel, transaction.user, transaction.pass, transaction.reference_code, transaction.service, transaction.first_name, transaction.last_name, transaction.account_number, transaction.Bank_code);
                //da.inser_Error(transaction.account_number, status.SubStatusMessage);
                //da.insert_Error_bitacora(status.SubStatusMessage + " error= " + ex.Message, transaction.account_number, Convert.ToDecimal(transaction.destination_amount));
            }

            return JSONResponce;
        }

        void Responce(ClsTransfer transfer, ClsStatus status, ref string JSONResponse, string transctionType = " ") {
            string oErr = string.Empty;
            try { 
                
            }
            catch (Exception ex)
            {
                status.StatusCode = "1000";
                status.StatusMessage = "Internal Error";
                status.SubStatusCode = "1001";
                status.SubStatusMessage = "System Error";
                oErr = ex.Message;
            }
        }

    }
}
