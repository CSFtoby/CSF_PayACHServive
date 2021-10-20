using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        #region validaciones de cuenta
        public bool valida_cuenta(string accaount)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT CANCELADA_B
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader["CANCELADA_B"].ToString().Equals("N"))
                            returno = false;
                        else
                            returno = true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool valida_bloqueo(string accaount)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT BLOQUEADA_B
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader["BLOQUEADA_B"].ToString().Equals("N"))
                            returno = false;
                        else
                            returno = true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        public bool valida_esistencia_cuenta(string accaount)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT NUMERO_CUENTA
                                    FROM MCA_CUENTAS
                                    WHERE NUMERO_CUENTA = :pa_accaount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", OracleDbType.Varchar2).Value = accaount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (String.IsNullOrEmpty(reader["NUMERO_CUENTA"].ToString()))
                            returno = false;
                        else
                            returno = true;
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        public bool valida_esistencia_operation(int operation)
        {
            bool returno = false;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT
                                        OPERATION_CODE
                                    FROM MCA.MCA_ACH_MOVIMIENTOS
                                    WHERE OPERATION_CODE = :pa_operationt";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_operationt", OracleDbType.Int32).Value = operation;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (String.IsNullOrEmpty(reader["OPERATION_CODE"].ToString()))
                            returno = false;
                        else
                            returno = true;
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        #endregion

        #region Informacion general
        public DataTable Personal_data(string accaount)
        {
            DataTable returno = new DataTable();
            OracleCommand command = new OracleCommand();
            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT 
                                   (REPLACE(CL.NOMBRES,'  ',' ') ||' '||(CL.PRIMER_APELLIDO ||' '|| CL.SEGUNDO_APELLIDO)) NAME,
                                   CTS.CODIGO_SUB_APLICACION ACCOUNT_TYPE, 
                                   REPLACE(CL.NUMERO_IDENTIFICACION,'-','') ID_NUMBER,
                                   CL.CODIGO_TIPO_IDENTIFICACION ID_TYPE,
                                   DIR.TELEFONO PHONE_NUMBER
                                FROM MCA.MCA_CUENTAS CTS,
                                     MGI.MGI_CLIENTES CL,
                                     (
                                       SELECT CODIGO_CLIENTE,CODIGO_MUNICIPIO,CODIGO_DEPARTAMENTO,CODIGO_PAIS,NOMENCLATURA_2 DIRECCION,
                                         CASE
                                            WHEN TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(TELEFONOS, TRANSLATE(TELEFONOS, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))IS  NULL  THEN  FAX
                                            WHEN SUBSTR(TELEFONOS,0,1) = '2' THEN FAX
                                            WHEN SUBSTR(TELEFONOS,0,1) != '2' THEN TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(TELEFONOS, TRANSLATE(TELEFONOS, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))
                                            ELSE
                                                   TO_NUMBER(REPLACE(TRANSLATE(SUBSTR((REPLACE(LTRIM(TRANSLATE(FAX, TRANSLATE(FAX, '1234567890', ' ') , ' ')),' ' ,'')),1,30), '-',' '),' ',''))
                                        END TELEFONO 
                                       FROM MGI.MGI_DIRECCIONES
                                        WHERE CODIGO_DIRECCION=1 
                                     )DIR
                                WHERE CL.CODIGO_CLIENTE = CTS.CODIGO_CLIENTE
                                  AND CTS.CODIGO_CLIENTE = DIR.CODIGO_CLIENTE(+)
                                  AND CTS.CANCELADA_B = 'N'
                                  AND (substr(DIR.TELEFONO,0,1) = '9' or substr(DIR.TELEFONO,0,1) = '3' or substr(DIR.TELEFONO,0,1) = '8')
                                  AND CTS.NUMERO_CUENTA = :pa_accaount
                                  AND ROWNUM = 1";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", accaount);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(returno);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }

            return returno;
        }

        //apartado para encontrar los datos que tienen que enviarse al procedimiento de movimientos diarios y el credito
        public DataTable Data_Transaction(string accaount)
        {
            DataTable da = new DataTable();
            OracleCommand command = new OracleCommand();
            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"select CODIGO_AGENCIA, CODIGO_EMPRESA, CODIGO_SUB_APLICACION
                                    from MCA_CUENTAS
                                    where NUMERO_CUENTA = :pa_accaount";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_accaount", accaount);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(da);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return da;
        }

        //obtener el codigo de operacion una vez se haya logrado el credito 
        public int obtener_codigo_operation(string accaount, string id_recived, string phone, decimal amount)
        {
            int returno = 0;
            OracleCommand command = new OracleCommand();
            OracleDataReader reader;

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT MAX(OPERATION_CODE) OPERATION_CODE
                                    FROM MCA.MCA_ACH_MOVIMIENTOS
                                   WHERE ACOUNT_NUMBER=:pa_acount
                                    AND ID_RECIVED = :pa_id
                                    AND PHONE = :pa_phone
                                    AND AMOUNT = :pa_amount";
                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_acaount", OracleDbType.Varchar2).Value = accaount;
                    command.Parameters.Add("pa_id", OracleDbType.Varchar2).Value = id_recived;
                    command.Parameters.Add("pa_phone", OracleDbType.Varchar2).Value = phone;
                    command.Parameters.Add("pa_amount", OracleDbType.Decimal).Value = amount;
                    connection.Open();

                    reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        if (reader["OPERATION_CODE"].ToString() != null)
                            returno = Convert.ToInt32(reader["OPERATION_CODE"]);
                        else
                            returno = 0;
                    }

                }
            }
            catch (Exception ex)
            {
                returno = 0;
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return returno;
        }

        public DataTable inf_Transfer(int operacion)
        {
            DataTable dt = new DataTable();
            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT OPERATION_CODE
                                        ,TRANSACCTION_ID, 
                                        STATUS_REQUEST, 
                                        RESULT_TRANSCTION, 
                                        DESCRIPTION_STATUS
                                    FROM MCA.MCA_ACH_MOVIMIENTOS
                                        WHERE OPERATION_CODE = :operation";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("operation", operacion);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(dt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return dt;
        }

        public DataTable inf_Status(int operacion)
        {
            DataTable dt = new DataTable();
            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"SELECT
                                    TRANSACCTION_ID,
                                    DATE_TRANSACTION,
                                    CURRENCY_CODE,
                                    CODE_BANK_SEND,
                                    ACOUNT_NUMBER,
                                    AMOUNT,
                                    RESULT_TRANSCTION,
                                    DESCRIPTION_STATUS
                                FROM MCA.MCA_ACH_MOVIMIENTOS
                                WHERE OPERATION_CODE = :pa_operation";

                    command.CommandText = sql;
                    command.Connection = connection;
                    command.Parameters.Add("pa_operation", operacion);

                    OracleDataAdapter dataAdapter = new OracleDataAdapter(command);

                    try
                    {
                        dataAdapter.Fill(dt);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error en " + e.TargetSite.ToString() + " " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en " + ex.TargetSite + "  " + ex.Message);
            }
            return dt;
        }

        #endregion

        #region Logs
        Logs logs = new Logs();

        public string log(string mensaje)
        {
            Logs.Guarda_Log("Logs", mensaje);
            return "OK";
        }
        #endregion

        //es en este apartado donde se llama el procedimiento que hace el credito a la cuenta del afiliado
        #region paquete
        public bool MCA_K_AHORROS(int cuenta, int subApp, int agencia, int empresa, decimal monto, decimal comision, int operacion)
        {
            bool returno = false;

            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    //MCA.MCA_P_ACH(P_CUENTA NUMBER, P_SUB_APP NUMBER, P_CODIGO_AGENCIA NUMBER, P_EMPRESA NUMBER, P_AMOUNT NUMBER, P_CODIGO_OPERACION NUMBER)
                    string query = @"MCA.MCA_P_ACH";

                    command.CommandText = query;
                    command.Connection = connection;
                    command.Parameters.Add("P_CUENTA", OracleDbType.Int32).Value = cuenta;
                    command.Parameters.Add("P_SUB_APP", OracleDbType.Int32).Value = subApp;
                    command.Parameters.Add("P_CODIGO_AGENCIA", OracleDbType.Int32).Value = agencia;
                    command.Parameters.Add("P_EMPRESA", OracleDbType.Int32).Value = empresa;
                    command.Parameters.Add("P_AMOUNT", OracleDbType.Decimal).Value = monto;
                    command.Parameters.Add("P_AMOUNT2", OracleDbType.Decimal).Value = comision;
                    command.Parameters.Add("P_CODIGO_OPERACION", OracleDbType.Int32).Value = operacion;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();

                    command.Dispose();
                    connection.Close();
                    returno = true;

                }
            }

            catch (Exception ex)
            {
                returno = false;
                throw new Exception($"{ex.TargetSite}: {ex.Message}", ex.InnerException);
                log($"{ex.TargetSite}:  {ex.Message}" + ex.InnerException);
            }

            return returno;
        }

        public bool insert_firts(string name_recived, string id_recived, string name_send, string id_send, string phone,
                                int id_type, decimal amount, string trans_type, string bank_recived,
                                string banK_send, string acount, int cod_sub_app, int cod_agencia, 
                                string status_messagge, string sub_status_messagge, char currency) {
            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle)) {
                    string sql = @"Insert into MCA.MCA_ACH_MOVIMIENTOS (
                                        NAME_RECIVED,
                                        ID_RECIVED,
                                        NAME_SEND,
                                        ID_SEND,
                                        PHONE,
                                        ID_TYPE,
                                        ACOUNT_NUMBER,
                                        ACOUNTTYPE,
                                        DATE_TRANSACTION,
                                        TYPE_TRANSACTION,
                                        CODE_BANK_RECIVED,
                                        CODE_BANK_SEND,
                                        AMOUNT,
                                        CONCEP,
                                        STATUS_REQUEST,
                                        RESULT_TRANSCTION,
                                        DESCRIPTION_STATUS,
                                        COD_SUB_AP,
                                        COD_AGENCIA,
                                        STATUS_MESSAGGE,
                                        SUB_STATUS_MESSAGGE,
                                        TRANSACCTION_ID,
                                        CURRENCY_CODE) 
                                values (
                                        :PA_NAME_RECIVED,               --VARCHAR2(80 BYTE)
                                        :PA_ID_RECIVED,                 --VARCHAR2(15 BYTE)
                                        :PA_NAME_SEND,                  --VARCHAR2(80 BYTE)
                                        :PA_ID_SEND,                    --VARCHAR2(15 BYTE)
                                        :PA_PHONE,                      --VARCHAR2(13 BYTE)
                                        :PA_ID_TYPE,                    --NUMBER
                                        :PA_ACOUNT_NUMBER,              --VARCHAR2(15 BYTE)
                                        :PA_ACOUNTTYPE,                 --NUMBER
                                        to_date(SYSDATE,'DD/MM/RRRR'),  --DATE_TRANSACTION    
                                        :PA_TYPE_TRANSACTION,           --VARCHAR2(20 BYTE)
                                        :PA_CODE_BANK_RECIVED,          --VARCHAR2(10 BYTE)
                                        :PA_CODE_BANK_SEND,             --VARCHAR2(10 BYTE)
                                        :PA_AMOUNT,                     --NUMBER(15,2)
                                        'TRANSFERENCIA ACH',            
                                        null,                           --:PA_STATUS_REQUEST
                                        null,                           --:PA_RESULT_TRANSCTION,
                                        null,                           --:PA_DESCRIPTION_STATUS,
                                        :PA_COD_SUB_AP,                 --NUMBER
                                        :PA_COD_AGENCIA,                --NUMBER
                                        :PA_STATUS_MESSAGGE,            --VARCHAR2(5 BYTE)
                                        :PA_SUB_STATUS_MESSAGGE,        --VARCHAR2(5 BYTE)
                                        null,                           -- PA_TRANSACCTION_IDNUMBER
                                        :PA_CURRENCY_CODE               --CHAR(1 BYTE)
                                        )";
                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("PA_NAME_RECIVED", OracleDbType.Varchar2, 80).Value = name_recived;
                    command.Parameters.Add("PA_ID_RECIVED", OracleDbType.Varchar2, 15).Value = id_recived;
                    command.Parameters.Add("PA_NAME_SEND", OracleDbType.Varchar2, 80).Value = name_send;
                    command.Parameters.Add("PA_ID_SEND", OracleDbType.Varchar2, 15).Value = id_send;
                    command.Parameters.Add("PA_PHONE", OracleDbType.Varchar2, 13).Value = phone;
                    command.Parameters.Add("PA_ID_TYPE", OracleDbType.Int32).Value = id_type;
                    command.Parameters.Add("PA_ACOUNT_NUMBER", OracleDbType.Varchar2, 15).Value = acount;
                    command.Parameters.Add("PA_ACOUNTTYPE", OracleDbType.Int32).Value = cod_sub_app;
                    command.Parameters.Add("PA_TYPE_TRANSACTION", OracleDbType.Varchar2, 20).Value = trans_type;
                    command.Parameters.Add("PA_CODE_BANK_RECIVED", OracleDbType.Varchar2, 10).Value = bank_recived;
                    command.Parameters.Add("PA_CODE_BANK_SEND", OracleDbType.Varchar2, 10).Value = banK_send;
                    command.Parameters.Add("PA_AMOUNT", OracleDbType.Decimal).Value = amount;
                    command.Parameters.Add("PA_COD_SUB_AP", OracleDbType.Int32).Value = cod_sub_app;
                    command.Parameters.Add("PA_COD_AGENCIA", OracleDbType.Int32).Value = cod_agencia;
                    command.Parameters.Add("PA_STATUS_MESSAGGE", OracleDbType.Varchar2, 15).Value = status_messagge;
                    command.Parameters.Add("PA_SUB_STATUS_MESSAGGE", OracleDbType.Varchar2, 25).Value = sub_status_messagge;
                    command.Parameters.Add("PA_CURRENCY_CODE", OracleDbType.Char).Value = currency;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    return true;
                    connection.Close();
                }
            }
            catch (Exception ex) {
                return false;
                log("Error en " + ex.TargetSite + "  " + ex.Message);
            }
        }

        public void insert_err(string status, string sub_status, string acount, decimal amount)
        {
            OracleCommand command = new OracleCommand();

            try
            {
                using (OracleConnection connection = new OracleConnection(cadenaConexionOracle))
                {
                    string sql = @"Insert into MCA_BITACORA_ACH (STATUS_MESSAGGE,SUB_STATUS_MESSAGGE,FECHA,CUENTA,MONTO) 
                                            values (:pa_status,:pa_sub_status,to_date(sysdate,'DD/MM/RRRR'),:pa_acount,:pa_amount)";
                    command.CommandText = sql;
                    command.Connection = connection;

                    command.Parameters.Add("pa_status", OracleDbType.Varchar2, 50).Value = status;
                    command.Parameters.Add("pa_sub_status", OracleDbType.Varchar2, 25).Value = sub_status;
                    command.Parameters.Add("pa_acount", OracleDbType.Varchar2, 50).Value = acount;
                    command.Parameters.Add("pa_amount", OracleDbType.Decimal).Value = amount;

                    if (connection.State.ToString().ToUpper().Equals("CLOSED"))
                        connection.Open();

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                log("Error en " + ex.TargetSite + "  " + ex.Message);
            }
        }

        #endregion
    }
}
