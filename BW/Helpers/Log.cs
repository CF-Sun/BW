using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace BW.Helpers
{
    public class Log
    {
        ConvertTime convertTime = new ConvertTime();

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        public void writeLogToFile(string info)
        {

            string logpath = System.AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["Logpath"].ToString();

            if (!Directory.Exists(logpath))
            {
                //新增資料夾
                Directory.CreateDirectory(logpath);
            }
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = new StreamWriter(logpath + "Log_" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".txt", true))
                {
                    sw.WriteLine(convertTime.UStoTW(DateTime.Now).ToString("yyyy/MM/dd HH:mm:ss:fff") + " " + info);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }

        }
        public void writeLogToDB(string ACCOUNT, string controllerName, string info)
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" INSERT SystemLog values(@ACCOUNT, @ControllerName, @info, @GetDate) ;";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ACCOUNT", ACCOUNT==null?"":ACCOUNT),
                        new SqlParameter("@ControllerName", controllerName),
                        new SqlParameter("@info", info),
                        new SqlParameter("@GetDate", convertTime.UStoTW(DateTime.Now))
                    });
                sqlcommand.ExecuteNonQuery();
            }
        }
    }
}