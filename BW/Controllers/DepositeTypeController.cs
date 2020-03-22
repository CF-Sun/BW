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
    public class DepositeTypeController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetDepositeType()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from DepositType order by Type_NO asc";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetEnableDepositeType()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from DepositType where Type_Status=1 order by Type_NO asc";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public int EditDepositeType(string Type_NO, string Type_NAME, decimal Type_RATE, string Type_MinAmount, string LoginACCOUNT)
        {
            try
            {
                Type_MinAmount = Type_MinAmount.Replace(",", "");

                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    decimal Actural_RATE = Type_RATE * 0.5m;

                    sqlconnection.Open();

                    string sqlcommandstring = @"update CodeList set CODE_DESC=@Type_NAME  where CODE_TYPE='Deposit_Type' and CODE_NO=@Type_NO ;
                                                update DepositType set Type_NAME=@Type_NAME, Type_RATE=@Type_RATE, Actural_RATE=@Actural_RATE, Type_MinAmount=@Type_MinAmount, UPDATE_DATE=@date where Type_NO=@Type_NO ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Type_NAME", Type_NAME),
                        new SqlParameter("@Type_NO", Type_NO),
                        new SqlParameter("@Type_RATE", Type_RATE),
                        new SqlParameter("@Actural_RATE", Actural_RATE),
                        new SqlParameter("@Type_MinAmount", Type_MinAmount),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/EditDepositeType", "更新案別,案別編號為" + Type_NO);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/EditDepositeType", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int EnableDepositeType(string Type_NO, bool Enable, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update DepositType set Type_Status=@Type_Status, UPDATE_DATE=@date where Type_NO=@Type_NO ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Type_NO", Type_NO),
                        new SqlParameter("@Type_Status", Enable),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    if (Enable)
                        log.writeLogToDB(LoginACCOUNT, "Deposite/EnableDepositeType", "販售案別,案別編號為" + Type_NO);
                    else
                        log.writeLogToDB(LoginACCOUNT, "Deposite/EnableDepositeType", "停售案別,案別編號為" + Type_NO);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/EnableDepositeType", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int AddDepositeType(string Type_NAME, decimal Type_RATE, string Type_MinAmount, string LoginACCOUNT)
        {
            try
            {
                Type_MinAmount = Type_MinAmount.Replace(",", "");
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    int Type_NO = 0;
                    decimal Actural_RATE = Type_RATE * 0.5m;

                    string sqlcommandstring = @"select ISNULL(max(Type_NO),0) as Type_NO from DepositType";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.ExecuteNonQuery();
                    Type_NO = Convert.ToInt16(sqlcommand.ExecuteScalar()) + 1;

                    //寫入資料庫
                    sqlcommandstring = @" insert into CodeList values('Deposit_Type', @Type_NO, @Type_NAME, 1); 
                                          insert into DepositType(Type_NO,Type_NAME,Type_RATE,Actural_RATE,Type_MinAmount,Type_Status,CREATE_DATE,UPDATE_DATE) 
                                            values(@Type_NO, @Type_NAME, @Type_RATE, @Actural_RATE, @Type_MinAmount, 1, @date, @date); ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Type_NAME", Type_NAME),
                        new SqlParameter("@Type_NO", Type_NO),
                        new SqlParameter("@Type_RATE", Type_RATE),
                        new SqlParameter("@Actural_RATE", Actural_RATE),
                        new SqlParameter("@Type_MinAmount", Type_MinAmount),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Deposite/AddDepositeType", "新增案別,案別編號為" + Type_NO);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/AddDepositeType", e.ToString());
                return 0;
            }
        }
    }
}