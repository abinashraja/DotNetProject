using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using System.Web;
namespace SIMS.Utility
{
    public class logWriter
    {
        public string ExePath { get; set; } = HttpContext.Current.Request.ApplicationPath;

        public logWriter(string logMessage)
        {
            LogWrite(logMessage);
        }

        public logWriter()
        {
        }

        public void LogWrite(string logMessage)
        {
            //ExePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter w = File.AppendText(ExePath + "\\" + "log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception)
            {
            }
        }

        public void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }
    }

}


