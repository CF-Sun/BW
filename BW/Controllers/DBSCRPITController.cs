using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    public class DBSCRPITController : Controller
    {
        [Route("[controller]")]
        public ActionResult DBSCRPIT()
        {
            return View();
        }

        [HttpGet]
        public string sqlcommand(string sqlcmd)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                StringBuilder sb = new StringBuilder();
                DataTable dt = new DataTable();
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    SqlCommand sqlcommand = new SqlCommand(sqlcmd, sqlconnection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        sb.Append("<table><tr>");

                        //組欄位名稱
                        foreach (DataColumn dc in dt.Columns)
                        {
                            sb.Append("<td>" + dc.ColumnName + "</td>");
                        }
                        sb.Append("</tr><tr>");
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            sb.Append("<tr>");
                            foreach (DataColumn dc in dt.Columns)
                            {
                                sb.Append("<td>" + dt.Rows[i][dc.ColumnName] + "</td>");
                            }
                            sb.Append("</tr>");
                        }
                        sb.Append("</table>");
                    }
                    else
                    {
                        return "no data";
                    }

                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                return "<p>" + e + "</p>";
            }
        }
        [HttpGet]
        public string sqlexecute(string sqlcmd)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                StringBuilder sb = new StringBuilder();
                DataTable dt = new DataTable();
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    SqlCommand sqlcommand = new SqlCommand(sqlcmd, sqlconnection);
                    sqlcommand.ExecuteNonQuery();
                }
                return "<p>success</p>";

            }
            catch (Exception e)
            {
                return "<p>" + e + "</p>";
            }
        }
    }
}