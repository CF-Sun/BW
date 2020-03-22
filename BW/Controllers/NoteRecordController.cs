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
    public class NoteRecordController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetNoteRecord()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" SELECT NR.*, 
                                            (CASE NR.Source_Role when '0' THEN Cli.Cli_ChiNAME_Last+Cli.Cli_ChiNAME_First
                                            ELSE  Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First END) as Name 
                                            from NoteRecord NR
                                            left join ConInfoDetail Con on NR.Source_ID=Con.Con_ID 
                                            left join CliInfoDetail Cli on NR.Source_ID=Cli.Cli_ID
                                            where IsReply =0 and IsClosed=0";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetNoteRecordByConID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //string sqlcommandstring = @" SELECT NR.*, 
                //                            (CASE NR.Source_Role when '0' THEN Cli.Cli_ChiNAME_Last+Cli.Cli_ChiNAME_First
                //                            ELSE  Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First END) as Name 
                //                            from NoteRecord NR
                //                            left join ConInfoDetail Con on NR.Source_ID=Con.Con_ID
                //                            left join CliInfoDetail Cli on NR.Source_ID=Cli.Cli_ID
                //                            where NR.Source_ID=@ConID or NR.Source_ID in (select Cli_ID from CliInfo where Con_ID=@ConID) 
                //                            order by NR.CREATE_DATE desc ";
                string sqlcommandstring = @" SELECT NR.*, 
                                            (CASE NR.Source_Role when '0' THEN Cli.Cli_ChiNAME_Last+Cli.Cli_ChiNAME_First
                                            ELSE  Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First END) as Name 
                                            from NoteRecord NR
                                            left join ConInfoDetail Con on NR.Source_ID=Con.Con_ID
                                            left join CliInfoDetail Cli on NR.Source_ID=Cli.Cli_ID
                                            where NR.CREATE_ID=@ConID 
                                            order by NR.CREATE_DATE desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetNoteRecordbyRole(string NO, string Name, string Source_Role)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" SELECT NR.*, 
                                            (CASE NR.Source_Role when '0' THEN Cli.Cli_ChiNAME_Last+Cli.Cli_ChiNAME_First
                                            ELSE  Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First END) as Name 
                                            from NoteRecord NR
                                            left join ConInfoDetail Con on NR.Source_ID=Con.Con_ID 
                                            left join CliInfoDetail Cli on NR.Source_ID=Cli.Cli_ID";

                if (Source_Role == "0")
                {
                    sqlcommandstring+= @" where Source_Role=@Source_Role and Source_ID in (select Cli_ID from CliInfoDetail
		                                            where Cli_ID like'%" + NO.Trim() + "%' and Cli_ChiNAME_Last+Cli_ChiNAME_First like N'%" + Name.Trim() + "%' )"; 
                }else if (Source_Role == "1")
                {
                    sqlcommandstring += @" where Source_Role=@Source_Role and Source_ID in (select Con_ID from ConInfoDetail
		                                            where Con_ID like'%" + NO.Trim() + "%' and Con_ChiNAME_Last+Con_ChiNAME_First like N'%" + Name.Trim() + "%' )";

                }


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@Source_Role", Source_Role)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetNoteRecordDetail(string Note_NO)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from NoteRecordDetail 
                                            where Note_No=@Note_NO order by SeqNo asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public int SaveNoteReply(string Note_NO, string ReplyContent, string FileName1, string FileName2, string FileName3, int newNote_Seq, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = @" update NoteRecord set IsReply=1, REPLY_DATE=@date
                                                where  Note_NO=@Note_NO";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();


                    sqlcommandstring = @" Insert NoteRecordDetail values(@Note_NO,@newNote_Seq,@ReplyContent,@FileName1,@FileName2,@FileName3,1,@date)";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim()),
                        new SqlParameter("@newNote_Seq", newNote_Seq),
                        new SqlParameter("@ReplyContent", ReplyContent),
                        new SqlParameter("@FileName1", FileName1.Trim()),
                        new SqlParameter("@FileName2", FileName2.Trim()),
                        new SqlParameter("@FileName3", FileName3.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "NoteRecord/SaveNoteReply", "儲存照會回覆內容,照會編號:"+ Note_NO.Trim()+",第"+ newNote_Seq + "則");

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "NoteRecord/SaveNoteReply", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int SaveNoteReplyByCon(string Note_NO, string ReplyContent, string FileName1, string FileName2, string FileName3, int newNote_Seq, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //更新照會狀態
                    string sqlcommandstring = @" update NoteRecord set IsReply=0, CREATE_DATE=@date
                                                where  Note_NO=@Note_NO";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();


                    //寫入新回覆內容
                    sqlcommandstring = @" Insert NoteRecordDetail values(@Note_NO,@newNote_Seq,@ReplyContent,@FileName1,@FileName2,@FileName3,0,@date)";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim()),
                        new SqlParameter("@newNote_Seq", newNote_Seq),
                        new SqlParameter("@ReplyContent", ReplyContent),
                        new SqlParameter("@FileName1", FileName1.Trim()),
                        new SqlParameter("@FileName2", FileName2.Trim()),
                        new SqlParameter("@FileName3", FileName3.Trim()),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "NoteRecord/SaveNoteReplyByCon", "儲存照會回覆內容,照會編號:" + Note_NO.Trim() + ",第" + newNote_Seq + "則");

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "NoteRecord/SaveNoteReplyByCon", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int CloseNoteRecord(string Note_NO, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update NoteRecord set IsClosed=1, CLOSED_DATE=@date
                                                where  Note_NO=@Note_NO";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "NoteRecord/CloseNoteRecord", "照會結案,編號為"+ Note_NO);

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "NoteRecord/CloseNoteRecord", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public JsonResult SendNoteReply(string Note_NO, string ReplyContent, string LoginACCOUNT)
        {
            try
            {
                string receEmail;

                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //取得客戶或顧問的email
                    string sqlcommandstring = @" select (CASE NR.Source_Role when '0' THEN Cli.Cli_Email ELSE  Con.Con_Email END)as email
                                                from NoteRecord NR 
                                                left join CliInfoDetail Cli on NR.Source_ID=Cli.Cli_ID
                                                left join ConInfoDetail Con on NR.Source_ID=Con.Con_ID
                                                where  Note_NO=@Note_NO";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", Note_NO.Trim())
                    });
                    receEmail = Convert.ToString(sqlcommand.ExecuteScalar()); //收件信箱

                    if (receEmail.Trim() == "")
                        return Json(new { result = false, message = "請確認收件者信箱" }, "text/html");

                    SendMail send = new SendMail();
                    if(!send.Send(receEmail, "照會回覆", ReplyContent))
                        return Json(new { result = false, message = "發送失敗" }, "text/html");
                }
                return Json(new { result = true, message = "發送成功" }, "text/html");
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "NoteRecord/SendNoteReply", e.ToString());
                return Json(new { result = false, message = "系統發生錯誤" }, "text/html");
            }
        }

        [HttpPost]
        public string AddNewNote(string Subject, string Content, string FileName1, string FileName2, string FileName3, string IsCon, string ConID, string CliID, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string newNote_NO = "";
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" select ISNULL(max(Note_NO),0) from NoteRecord";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    newNote_NO = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                    while (newNote_NO.Length < 10)  //ID長度為10 不足長度補0
                    {
                        newNote_NO = "0" + newNote_NO;
                    }

                    sqlcommandstring = @" Insert NoteRecord (Note_NO,Source_ID,Source_Role,Note_Subject,IsReply,IsClosed,CREATE_DATE,CREATE_ID)
                                        values(@Note_NO,@Source_ID,@Source_Role,@Subject,0,0,@date,@CREATE_ID); 
                                         Insert NoteRecordDetail values(@Note_NO,1,@Content,@FileName1,@FileName2,@FileName3,0,@date) ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_NO", newNote_NO),
                        new SqlParameter("@Source_ID", IsCon=="True"?ConID.Trim():CliID.Trim()),
                        new SqlParameter("@Source_Role", IsCon=="True"?"1":"0"),
                        new SqlParameter("@Subject", Subject),
                        new SqlParameter("@Content", Content),
                        new SqlParameter("@FileName1", FileName1),
                        new SqlParameter("@FileName2", FileName2),
                        new SqlParameter("@FileName3", FileName3),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@CREATE_ID", LoginACCOUNT)
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "AddNewNote/AddNewNote", "新增照會編號為" + newNote_NO);
                }

                return newNote_NO;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "AddNewNote/AddNewNote", e.ToString());
                return "0";
            }
        }

        public virtual ActionResult UploadFile(string Note_NO, string Note_Seq)
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
                    string filePath = Server.MapPath("~/Content/files/Note/" + Note_NO + "/" + Note_Seq + "/");
                    fileName = fileContent.FileName;
                    string fileLocation = filePath + fileName;

                    //判斷檔名是否含有特殊字元
                    Validate validate = new Validate();
                    if(!validate.ValidateString(fileName))
                        return Json(new { isUploaded = false, result = "檔名不能包含特殊字元'&'", filename = "" }, "text/html");

                    if (extension == ".docx" || extension == ".pdf" || extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {                       

                        //路徑不存在則新增資料夾
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (System.IO.File.Exists(fileLocation)) // 驗證檔案是否存在
                            System.IO.File.Delete(fileLocation);


                        Request.Files["UploadFile"].SaveAs(fileLocation); // 存放檔案到伺服器上

                        //if(!updateNoteRecord(Note_ID, Location, fileName, LoginACCOUNT))//更新資料庫
                        //{
                        //    System.IO.File.Delete(fileLocation); //刪除以上傳的檔案
                        //    return Json(new { isUploaded = false, result = "資料庫發生錯誤", filename = "" }, "text/html");
                        //}
                    }
                    else
                    {
                        return Json(new { isUploaded = false, result = "檔案格式不符", filename = "" }, "text/html");
                    }

                }
                return Json(new { isUploaded = true, result = "上傳成功", filename = fileName }, "text/html");
            }
            catch(Exception ex)
            {
                log.writeLogToDB("", "NoteRecord/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool updateNoteRecord(string Note_ID, string Location, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update NoteRecord set REPLY_FileName"+ Location+ @"=@fileName, REPLY_DATE=@date
                                                where  ID=@Note_ID";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Note_ID", Note_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "NoteRecord/updateNoteRecord", "更新回覆照會檔案,照會編號為" + Note_ID.Trim() + ",檔名為" + fileName);

                }
                return true;
            }
            catch(Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "NoteRecord/updateNoteRecord", ex.ToString());
                return false;
            }
        }
        //開啟檔案
        public ActionResult Open(string FileName, string Note_NO, int SeqNo)
        {

                var path = Server.MapPath("/Content/files/Note/" + Note_NO + "/" + SeqNo + "/" + FileName);

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
    }
}