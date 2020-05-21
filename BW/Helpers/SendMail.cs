using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace BW.Helpers
{
    public class SendMail
    {
        Log log = new Log();
        public bool Send(string receEmail, string SUBJECT, string mailbody)
        {
            try
            {
                string fromAccount = "";
                string fromAccountPW = "";
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //取得發送帳密
                    string sqlcommandstring = @" select * from MailAccount ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    SqlDataReader dr = sqlcommand.ExecuteReader();

                    while (dr.Read())
                    {
                        fromAccount = dr[1].ToString(); //帳號
                        fromAccountPW = dr[2].ToString();  //密碼
                    }
                    if (!toSend(fromAccount, fromAccountPW, receEmail, SUBJECT, mailbody))
                        return false;
                }
                return true;

            }
            catch (Exception e)
            {
                log.writeLogToFile("SendMail/Send=>"+ e.ToString());
                return false;
            }
        }
        public bool toSend(string fromAccount, string pw, string receEmail, string SUBJECT, string mailbody)
        {
            try
            {
                //建立 SmtpClient 物件 並設定 Gmail的smtp主機及Port  
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("relay-hosting.secureserver.net");
                //SmtpClient MySmtp = new SmtpClient();
                //設定你的帳號密碼
                MySmtp.Credentials = new System.Net.NetworkCredential(fromAccount, pw);
                //Gmial 的 smtp 必需要使用 SSL
                MySmtp.EnableSsl = false;
                //發送Email
                log.writeLogToDB("", "SendMail/toSend", "start send");
                MySmtp.Send(fromAccount, receEmail, SUBJECT, mailbody); MySmtp.Dispose();
                log.writeLogToDB("", "SendMail/toSend", "end send");
                return true;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "SendMail/toSend", e.ToString());
                return false;
            }
        }

        public bool SendForAnnounce(string receEmail, string SUBJECT, string mailbody, string fileName1, string fileName2, string fileName3)
        {
            try
            {
                string fromAccount = "";
                string fromAccountPW = "";
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //取得發送帳密
                    string sqlcommandstring = @" select * from MailAccount ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    SqlDataReader dr = sqlcommand.ExecuteReader();

                    while (dr.Read())
                    {
                        fromAccount = dr[1].ToString(); //帳號
                        fromAccountPW = dr[2].ToString();  //密碼
                    }
                    if (!toSendForAnnounce(fromAccount, fromAccountPW, receEmail, SUBJECT, mailbody, fileName1, fileName2, fileName3))
                        return false;
                }
                return true;

            }
            catch (Exception e)
            {
                log.writeLogToFile("SendMail/Send=>" + e.ToString());
                return false;
            }
        }
        public bool toSendForAnnounce(string fromAccount, string pw, string receEmail, string SUBJECT, string mailbody, string fileName1, string fileName2, string fileName3)
        {
            try
            {
                //建立 SmtpClient 物件 並設定 Gmail的smtp主機及Port  
                System.Net.Mail.SmtpClient MySmtp = new System.Net.Mail.SmtpClient("relay-hosting.secureserver.net");
                //設定你的帳號密碼
                MySmtp.Credentials = new System.Net.NetworkCredential(fromAccount, pw);
                //Gmial 的 smtp 必需要使用 SSL
                MySmtp.EnableSsl = false;

                MailMessage mms = new MailMessage();
                mms.From = new MailAddress(fromAccount);
                mms.Bcc.Add(new MailAddress(receEmail));
                mms.Subject = SUBJECT;
                mms.Body = mailbody;

                //附加檔案
                if (fileName1.Trim() != "")
                {
                    Attachment attachment = new Attachment(fileName1);
                    mms.Attachments.Add(attachment);
                }
                if (fileName2.Trim() != "")
                {
                    Attachment attachment = new Attachment(fileName2);
                    mms.Attachments.Add(attachment);
                }
                if (fileName3.Trim() != "")
                {
                    Attachment attachment = new Attachment(fileName3);
                    mms.Attachments.Add(attachment);
                }

                //發送Email
                MySmtp.Send(mms); MySmtp.Dispose();
                return true;
            }
            catch (Exception e)
            {
                log.writeLogToFile("SendMail/toSend=>" + e.ToString());
                return false;
            }
        }
    }
}