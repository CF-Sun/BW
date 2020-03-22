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
    public class PurchaseController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetPurchaseData()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.ID, A.Name, A.Phone, A.Email, B.CODE_DESC as Deposit_Typ, A.Deposit_Amount,
                                                A.Con_ID, A.Status as StatusCode, C.CODE_DESC as Status, 
                                                convert(varchar, CREATE_DATE, 111) as Apply_DATE
                                                from PurchaseApply A
                                                left join (select CODE_NO, CODE_DESC from CodeList 
		                                                where CODE_TYPE='Deposit_Type' and CODE_Status=1) B on A.Deposit_Type=B.CODE_NO
                                                left join (select CODE_NO, CODE_DESC from CodeList 
		                                                where CODE_TYPE='PurchaseApply' and CODE_Status=1) C on A.Status=C.CODE_NO
                                                order by Apply_DATE ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int PurchaseApply(string Name, string Phone, string Email, string DepositType, string Amount, string ConID)
        {
            try
            {
                Amount = Amount.Replace(",", "");

                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" Insert PurchaseApply values(@Name, @Phone, @Email, @DepositType, @Amount, @ConID, 1, @date, @date)";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Name", Name),
                        new SqlParameter("@Phone", Phone),
                        new SqlParameter("@Email", Email),
                        new SqlParameter("@DepositType", DepositType),
                        new SqlParameter("@Amount", Amount),
                        new SqlParameter("@ConID", ConID),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "Purchase/PurchaseApply", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateStatus(string ID, string Status, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update PurchaseApply set Status=@Status,UPDATE_DATE=@date  where ID=@ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ID", ID==null?"": ID),
                        new SqlParameter("@Status", Status),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Purchase/UpdateStatus", "更新申購狀態為"+ Status + ",編號為" + ID);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Purchase/UpdateStatus", e.ToString());
                return 0;
            }
        }
    }
}