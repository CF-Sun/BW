using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class LackDocController : Controller
    {
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetConLackDoc()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" SELECT A.Con_ID,(B.Con_ChiNAME_Last+B.Con_ChiNAME_First)as Name,
                                                IsIDCardPos,IsIDCardBack,IsResidenceProof,IsConsent
                                                FROM ConInfoCredentials A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where IsIDCardPos=0 or IsIDCardBack=0 or IsResidenceProof=0 or IsConsent=0";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliLackDoc()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" SELECT A.Cli_ID,(B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                            (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName, D.Cli_Role,
                                            Isfile1,Isfile2,Isfile3,Isfile4,Isfile5,Isfile6,Isfile7,Isfile8,Isfile9
                                            FROM CliInfoCredentials A
                                            left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                            left join ConInfoDetail C on A.Con_ID=C.Con_ID
                                            left join CliInfo D on A.Cli_ID=D.Cli_ID
                                            where Isfile1=0 or Isfile2=0 or Isfile3=0 or Isfile4=0 or Isfile5=0 or Isfile6=0 or Isfile7=0 or Isfile8=0 or Isfile9=0";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConNewSignUp()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"   select A.Con_ID, A.CREATE_DATE,
                                              (B.Con_ChiNAME_Last+B.Con_ChiNAME_First)as Name 
                                              from ConInfo A
                                              join ConInfoDetail B on A.Con_ID=B.Con_ID
                                              where DATEDIFF(day,A.CREATE_DATE,'" + convertTime.UStoTW(DateTime.Now).ToString("yyyy/MM/dd hh:mm:ss") + @"')<=31
                                              order by A.CREATE_DATE desc";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliNewOpen()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Cli_ID, A.CREATE_DATE,
                                              (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                              (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName  
                                              from CliInfo A
                                              join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                              join ConInfoDetail C on A.Con_ID=C.Con_ID
                                              where DATEDIFF(day,A.CREATE_DATE,'" + convertTime.UStoTW(DateTime.Now).ToString("yyyy/MM/dd hh:mm:ss") + @"')<=31
                                              order by A.CREATE_DATE desc";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}