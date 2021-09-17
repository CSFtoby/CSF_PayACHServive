using Oracle.ManagedDataAccess.Client;
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

    }
}
