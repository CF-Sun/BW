using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]

    public class AnnouncementController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetAnnouncement(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Announcement_NO,Announcement_Subject,Announcement_Content,FileName1,FileName2,FileName3,
                                            convert(varchar, CREATE_DATE, 111) as CREATE_DATE,
											CODE_DESC as AnnounceObject, Announcement_Object
                                            from Announcement A
											left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Announcement_Object')CL on A.Announcement_Object=CL.CODE_NO
											where 1=1 ";

                if (startDate != "")
                {
                    sqlcommandstring += "and CREATE_DATE >= '" + startDate + "' ";
                }
                if (endDate != "")
                {
                    sqlcommandstring += "and CREATE_DATE <= '" + endDate + " 23:59:59.999' ";
                }
                sqlcommandstring += " order by CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@startDate", startDate),
                        new SqlParameter("@endDate", endDate)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetAnnouncementByObject(string announceObject)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Announcement_NO,Announcement_Subject,Announcement_Content,FileName1,FileName2,FileName3,
                                            convert(varchar, CREATE_DATE, 111) as CREATE_DATE
                                            from Announcement 
											where 1=1 ";
                if (announceObject == "0")
                    sqlcommandstring += " and Announcement_Object=0 or Announcement_Object=2 ";
                else if (announceObject == "1")
                    sqlcommandstring += " and Announcement_Object=1 or Announcement_Object=2 ";
                sqlcommandstring += " order by CREATE_DATE desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetAnnouncementByID(string Announcement_NO)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Announcement_NO,Announcement_Subject,Announcement_Content,FileName1,FileName2,FileName3,
                                            convert(varchar, CREATE_DATE, 111) as CREATE_DATE 
                                            from Announcement where Announcement_NO=@Announcement_NO ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Announcement_NO", Announcement_NO)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public string AddNewAnnouncement(string Subject, string ReplyContent, string EmailTo, string FileName1, string FileName2, string FileName3, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string newAnnouncement_NO = "";
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" select ISNULL(max(Announcement_NO),0) from Announcement";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    newAnnouncement_NO = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                    while (newAnnouncement_NO.Length < 10)  //ID長度為10 不足長度補0
                    {
                        newAnnouncement_NO = "0" + newAnnouncement_NO;
                    }
                    sqlcommandstring = @" Insert Announcement values(@Announcement_NO,@Subject,@ReplyContent,@FileName1,@FileName2,@FileName3,@date,@date,@EmailTo) ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Announcement_NO", newAnnouncement_NO),
                        new SqlParameter("@Subject", Subject),
                        new SqlParameter("@ReplyContent", ReplyContent),
                        new SqlParameter("@FileName1", FileName1.Trim()),
                        new SqlParameter("@FileName2", FileName2.Trim()),
                        new SqlParameter("@FileName3", FileName3.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@EmailTo", EmailTo)
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Announcement/AddNewAnnouncement", "新增公告編號為" + newAnnouncement_NO);
                }

                return newAnnouncement_NO;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Announcement/AddNewAnnouncement", e.ToString());
                return "0";
            }
        }
        [HttpPost]
        public string EditAnnouncement(string Announcement_NO, string Subject, string ReplyContent, string EmailTo, string FileName1, string FileName2, string FileName3, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update Announcement set Announcement_Subject=@Subject, Announcement_Content=@ReplyContent,Announcement_Object=@EmailTo, UPDATE_DATE=@date  ";
                    if (FileName1.Trim() != "")
                        sqlcommandstring += @", FileName1=@FileName1 ";
                    if (FileName2.Trim() != "")
                        sqlcommandstring += @", FileName2=@FileName2 ";
                    if (FileName3.Trim() != "")
                        sqlcommandstring += @", FileName3=@FileName3 ";
                    sqlcommandstring += @" where Announcement_NO=@Announcement_NO ";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Announcement_NO", Announcement_NO.Trim()),
                        new SqlParameter("@Subject", Subject),
                        new SqlParameter("@ReplyContent", ReplyContent),
                        new SqlParameter("@FileName1", FileName1.Trim()),
                        new SqlParameter("@FileName2", FileName2.Trim()),
                        new SqlParameter("@FileName3", FileName3.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@EmailTo", EmailTo)
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Announcement/AddNewAnnouncement", "更新公告編號為" + Announcement_NO);
                }

                return Announcement_NO;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Announcement/AddNewAnnouncement", e.ToString());
                return "0";
            }
        }
        [HttpPost]
        public int SendAnnouncement(string Announcement_NO, string Subject, string ReplyContent, string EmailTo)
        {
            try
            {
                string fileName1 = "";
                string fileName2 = "";
                string fileName3 = "";
                DataTable dt = new DataTable();
                SendMail send = new SendMail();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";

                    if (EmailTo == "0")
                        sqlcommandstring = @" select Con_Email as Email from ConInfoDetail where Con_Email is not null and Con_Email !='' ";
                    else if (EmailTo == "1")
                        sqlcommandstring = @" select Cli_Email as Email from CliInfoDetail where Cli_Email is not null and Cli_Email !='' ";
                    else if(EmailTo == "2")
                        sqlcommandstring = @" select Con_Email as Email from ConInfoDetail where Con_Email is not null and Con_Email !=''
                                            union
                                            select Cli_Email as Email from CliInfoDetail where Cli_Email is not null and Cli_Email !='' ";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    sqlcommandstring = @" select * from Announcement where Announcement_NO=@Announcement_NO ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Announcement_NO", Announcement_NO.Trim())
                    });
                    SqlDataReader reader = sqlcommand.ExecuteReader();
                    while (reader.Read())
                    {
                        fileName1 = reader["FileName1"].ToString();
                        fileName2 = reader["FileName2"].ToString();
                        fileName3 = reader["FileName3"].ToString();
                    }
                    reader.Close();

                    if (fileName1.Trim() != "")
                        fileName1 = Server.MapPath("/Content/files/Announcement/" + Announcement_NO + "/" + fileName1);
                    if (fileName2.Trim() != "")
                        fileName2 = Server.MapPath("/Content/files/Announcement/" + Announcement_NO + "/" + fileName2);
                    if (fileName3.Trim() != "")
                        fileName3 = Server.MapPath("/Content/files/Announcement/" + Announcement_NO + "/" + fileName3);

                    if (dt.Rows.Count > 0)
                    {
                        for(int i = 0; i < dt.Rows.Count; i++)
                        {
                            string address = dt.Rows[i]["Email"].ToString();
                            if (IsValidEmail(address))
                                send.SendForAnnounce(address, "公告:" + Subject, ReplyContent, fileName1, fileName2, fileName3);                      
                        }
                    }
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "Announcement/SendAnnouncement", e.ToString());
                return 0;
            }
        }
        public virtual ActionResult chkFile()
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
                    fileName = fileContent.FileName;

                    //判斷檔名是否含有特殊字元
                    Validate validate = new Validate();
                    if (!validate.ValidateString(fileName))
                        return Json(new { isUploaded = false, result = "檔名不能包含特殊字元'&'", filename = "" }, "text/html");

                    if (extension == ".docx" || extension == ".pdf" || extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
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
                log.writeLogToDB("", "Announcement/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult UploadFile(string ID)
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
                    string filePath = Server.MapPath("~/Content/files/Announcement/" + ID + "/");
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


                        Request.Files["UploadFile"].SaveAs(fileLocation); // 存放檔案到伺服器上

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
                log.writeLogToDB("", "Announcement/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }

        //開啟檔案
        public ActionResult Open(string FileName, string Announcement_NO)
        {

            var path = Server.MapPath("/Content/files/Announcement/" + Announcement_NO + "/" + FileName);

            if (System.IO.File.Exists(path))
            {
                var mime = MimeMapping.GetMimeMapping(FileName);
                return File(path, mime, FileName);
            }
            else
            {
                return Json("找不到檔案", JsonRequestBehavior.AllowGet);
            }
        }
        bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}