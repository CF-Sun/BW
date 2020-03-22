using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class WithdrawalController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetCliNewWithdrawal()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Withdrawal_ID, A.Cli_ID, A.Withdrawal_Amount,A.Status as StatusCode, CL.CODE_DESC as Status, A.CREATE_DATE,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE, WithdrawalListFileName, Isfile,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                                (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName,
                                                (A.ExpectYear+'/'+A.ExpectMonth)as ExpectMonth, Remark
                                                from WithdrawalList A
                                                join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                join ConInfoDetail C on A.Con_ID=C.Con_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Withdrawal_Status')CL on A.Status=CL.CODE_NO
                                                where A.Status=0 and DATEDIFF(day,A.CREATE_DATE,'" + convertTime.UStoTW(DateTime.Now).ToString("yyyy/MM/dd hh:mm:ss") + @"')<=31
                                                order by A.CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetWithdrawalDataTotal(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select  sum(A.Withdrawal_Amount) Withdrawal_Amount, CL.CODE_DESC,D.Deposit_Type
                                                from WithdrawalList A
												left join DepositList D on A.Deposit_ID=D.Deposit_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on D.Deposit_Type=CL.CODE_NO
												where A.Status=2 ";
                if (startDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE >= '" + startDate + "' ";
                }
                if (endDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE <= '" + endDate + "' ";
                }
                sqlcommandstring += " group by CL.CODE_DESC,D.Deposit_Type order by D.Deposit_Type asc  ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetWithdrawalData(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Deposit_ID,A.Cli_ID, A.Con_ID, A.Withdrawal_Amount, A.Status,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE, CL.CODE_DESC,D.Deposit_Type,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                                (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName 
                                                from WithdrawalList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join ConInfoDetail C on A.Con_ID=C.Con_ID
												left join DepositList D on A.Deposit_ID=D.Deposit_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on D.Deposit_Type=CL.CODE_NO
												where A.Status=2 ";
                if (startDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE >= '" + startDate + "' ";
                }
                if (endDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE <= '" + endDate + "' ";
                }
                sqlcommandstring += " order by A.Arrival_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetWithdrawalDataByConId(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Withdrawal_ID,A.Cli_ID, A.Withdrawal_Amount, CL1.CODE_DESC as Status,A.Deposit_ID,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE,
                                                convert(varchar, A.CREATE_DATE, 111) as CREATE_DATE,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
												(A.ExpectYear+'/'+A.ExpectMonth)as ExpectMonth
                                                from WithdrawalList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
												left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Withdrawal_Status' and CODE_Status=1) CL1 on A.Status=CL1.CODE_NO
												where A.Con_ID=@ConID
												order by A.CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetWithdrawalDataByCliId(string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Withdrawal_ID,A.Cli_ID, A.Withdrawal_Amount, CL1.CODE_DESC as Status,A.Deposit_ID,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE,
                                                convert(varchar, A.CREATE_DATE, 111) as CREATE_DATE,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
												(A.ExpectYear+'/'+A.ExpectMonth)as ExpectMonth
                                                from WithdrawalList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
												left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Withdrawal_Status' and CODE_Status=1) CL1 on A.Status=CL1.CODE_NO
												where A.Cli_ID=@CliID
												order by A.CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public int WithdrawalRegi(string CliNo, string ConNo,  string WithdrawalAmount, string sele_From, string ApplyDate, 
            string ExpectYear, string ExpectMonth, string Remark, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string Withdrawal_ID = "";

                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = @" select ISNULL(max(Withdrawal_ID),0) from WithdrawalList";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    Withdrawal_ID = Convert.ToString(Convert.ToInt32(Convert.ToString(sqlcommand.ExecuteScalar()).Substring(1)) + 1);

                    while (Withdrawal_ID.Length < 9)  //ID長度為10 不足長度補0
                    {
                        Withdrawal_ID = "0" + Withdrawal_ID;
                    }
                    Withdrawal_ID = "W" + Withdrawal_ID;

                    if (String.IsNullOrEmpty(ConNo))
                    {
                        sqlcommandstring = @" select Con_ID from CliInfo where Cli_ID=@Cli_ID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Cli_ID", CliNo)
                        });
                        ConNo = Convert.ToString(sqlcommand.ExecuteScalar());
                    }

                    WithdrawalAmount = WithdrawalAmount.Replace(",", "");

                    sqlcommandstring = @" Insert WithdrawalList (Withdrawal_ID,Cli_ID,Con_ID,Withdrawal_Amount,Deposit_ID
                                                               ,Status,CREATE_DATE,UPDATE_DATE,ExpectYear,ExpectMonth,Remark) 
                                            values(@Withdrawal_ID,@Cli_ID,@ConNo,@WithdrawalAmount,@DepositID,
                                                                0,@ApplyDate,@date,@ExpectYear,@ExpectMonth,@Remark) ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Withdrawal_ID", Withdrawal_ID),
                        new SqlParameter("@Cli_ID", CliNo),
                        new SqlParameter("@ConNo", ConNo),
                        new SqlParameter("@WithdrawalAmount", WithdrawalAmount),
                        new SqlParameter("@DepositID", sele_From),
                        new SqlParameter("@ApplyDate", ApplyDate),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@ExpectYear", ExpectYear),
                        new SqlParameter("@ExpectMonth", ExpectMonth),
                        new SqlParameter("@Remark", Remark)
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Deposite/DepositeRegi", "新增出金登記編號為" + Withdrawal_ID);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/DepositeRegi", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateStatus(string Withdrawal_ID, string Status, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update WithdrawalList set Status=@Status,UPDATE_DATE=@date  where Withdrawal_ID=@Withdrawal_ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Withdrawal_ID", Withdrawal_ID==null?"": Withdrawal_ID),
                        new SqlParameter("@Status", Status),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Withdrawal/UpdateStatus","設定出金單號為"+ Withdrawal_ID+",狀態:"+ Status);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Withdrawal/UpdateStatus", e.ToString());
                return 0;
            }
        }
        //檔案上傳
        public virtual ActionResult UploadFile(string Withdrawal_ID, string OriFileName,string LoginACCOUNT)
        {
            try
            {
                var fileName = "";

                var fileContent = Request.Files["UploadFile"];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    //判斷檔案大小,不能超過3MB
                    if (fileContent.ContentLength > 3145728)
                        return Json(new { isUploaded = false, result = "檔案大小不能超過3MB", filename = "" }, "text/html");


                    string extension = Path.GetExtension(fileContent.FileName).ToLower();
                    string filePath = Server.MapPath("~/Content/files/WithdrawalList/" + Withdrawal_ID.Trim() + "/");
                    fileName = fileContent.FileName;
                    string fileLocation = filePath + fileName;

                    //判斷檔名是否含有特殊字元
                    Validate validate = new Validate();
                    if (!validate.ValidateString(fileName))
                        return Json(new { isUploaded = false, result = "檔名不能包含特殊字元'&'", filename = "" }, "text/html");

                    if (extension == ".docx" || extension == ".pdf" || extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {

                        //路徑不存在則新增資料夾
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (System.IO.File.Exists(fileLocation)) // 驗證檔案是否存在
                            System.IO.File.Delete(fileLocation);

                        if (System.IO.File.Exists(filePath + OriFileName)) //驗證舊檔案是否存在
                            System.IO.File.Delete(filePath + OriFileName);

                        Request.Files["UploadFile"].SaveAs(fileLocation); // 存放檔案到伺服器上

                        if (!updateWithdrawal(Withdrawal_ID, fileName, LoginACCOUNT))//更新資料庫
                        {
                            System.IO.File.Delete(fileLocation); //刪除以上傳的檔案
                            return Json(new { isUploaded = false, result = "資料庫發生錯誤", filename = "" }, "text/html");
                        }
                    }
                    else
                    {
                        return Json(new { isUploaded = false, result = "檔案格式不符", filename = "" }, "text/html");
                    }

                }
                return Json(new { isUploaded = true, result = "上傳成功", filename = fileName }, "text/html");
            }
            catch (Exception ex)
            {
                log.writeLogToDB("", "Withdrawal/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool updateWithdrawal(string Withdrawal_ID, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update WithdrawalList set WithdrawalListFileName=@fileName, Isfile=1, UPDATE_DATE=@date  where  Withdrawal_ID=@Withdrawal_ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Withdrawal_ID", Withdrawal_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Withdrawal/updateWithdrawal", "更新出金單號為" + Withdrawal_ID.Trim() + ",檔案名稱:" + fileName);

                }
                return true;
            }
            catch (Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "Withdrawal/updateWithdrawal", ex.ToString());
                return false;
            }
        }

        //開啟檔案
        public ActionResult Open(string Withdrawal_ID, string FileName)
        {
            var path = Server.MapPath("/Content/files/WithdrawalList/" + Withdrawal_ID.Trim() + "/" + FileName);
            var mime = MimeMapping.GetMimeMapping(FileName);
            return File(path, mime, FileName);
        }
    }
}