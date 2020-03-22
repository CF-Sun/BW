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
    public class TransferRecordController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetTransferRecord(string ID, string Name, string Type)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = "";
                if (Type == "1")
                {
                    sqlcommandstring = @" select TR.ConIDOrCliID, (CID1.Con_ChiNAME_Last+CID1.Con_ChiNAME_First)as ChiName1,
CASE TR.OriginParentConID WHEN '000' THEN N'股東' ELSE TR.OriginParentConID END as OriginParentConID,
CASE TR.OriginParentConID WHEN '000' THEN N'股東' ELSE (CID2.Con_ChiNAME_Last+CID2.Con_ChiNAME_First) END as ChiName2,
CASE TR.NewParentConID WHEN '000' THEN N'股東' ELSE TR.NewParentConID END as NewParentConID,
CASE TR.NewParentConID WHEN '000' THEN N'股東' ELSE (CID3.Con_ChiNAME_Last+CID3.Con_ChiNAME_First) END as ChiName3,
TR.CREATE_DATE 
from TransferRecord TR
left join ConInfoDetail CID1 on TR.ConIDOrCliID=CID1.Con_ID
left join ConInfoDetail CID2 on TR.OriginParentConID=CID2.Con_ID
left join ConInfoDetail CID3 on TR.NewParentConID=CID3.Con_ID
where TR.Type='1' and TR.ConIDOrCliID like '%" + ID.Trim() + "%' and CID1.Con_ChiNAME_Last+CID1.Con_ChiNAME_First like N'%" + Name.Trim() + "%' order by ID desc ";
                }
                else if (Type == "2")
                {
                    sqlcommandstring = @" select TR.ConIDOrCliID, (CID1.Cli_ChiNAME_Last+CID1.Cli_ChiNAME_First)as ChiName1,
CASE TR.OriginParentConID WHEN '000' THEN N'股東' ELSE TR.OriginParentConID END as OriginParentConID,
CASE TR.OriginParentConID WHEN '000' THEN N'股東' ELSE (CID2.Con_ChiNAME_Last+CID2.Con_ChiNAME_First) END as ChiName2,
CASE TR.NewParentConID WHEN '000' THEN N'股東' ELSE TR.NewParentConID END as NewParentConID,
CASE TR.NewParentConID WHEN '000' THEN N'股東' ELSE (CID3.Con_ChiNAME_Last+CID3.Con_ChiNAME_First) END as ChiName3,
TR.CREATE_DATE 
from TransferRecord TR
left join CliInfoDetail CID1 on TR.ConIDOrCliID=CID1.Cli_ID
left join ConInfoDetail CID2 on TR.OriginParentConID=CID2.Con_ID
left join ConInfoDetail CID3 on TR.NewParentConID=CID3.Con_ID
where TR.Type='2' and TR.ConIDOrCliID like '%" + ID.Trim() + "%' and CID1.Cli_ChiNAME_Last+CID1.Cli_ChiNAME_First like N'%" + Name.Trim() + "%' order by ID desc";
                }

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}