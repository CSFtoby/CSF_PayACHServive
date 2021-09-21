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
                        if (reader["CANCELADA_B"].ToString() == "N")
                            returno = true;
                        else
                            returno = false;
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
                        if (reader["BLOQUEADA_B"].ToString() == "N")
                            returno = true;
                        else
                            returno = false;
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
                                   DIR.TELEFONO PHONE_NUMBER,
                                   CL.SEXO SEX
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


        #endregion
    }
}
