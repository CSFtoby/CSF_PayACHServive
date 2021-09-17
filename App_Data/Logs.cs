using System;
using System.IO;

namespace CSF_PayACHServive
{
    public class Logs
    {
        public static void Guarda_Log(string nombre, string messagge)
        {
            string directorio = AppDomain.CurrentDomain.BaseDirectory + "Logs/" + DateTime.Now.Year.ToString() +
                                                                            "/" + DateTime.Now.Month.ToString() +
                                                                            "/" + DateTime.Now.Day.ToString();

            if (!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            StreamWriter archivo = new StreamWriter(directorio + "/" + nombre + ".txt", true);
            string cadena = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ---> " + messagge;
            archivo.WriteLine(cadena);
            archivo.Close();
        }
    }
}
