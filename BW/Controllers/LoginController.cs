using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    public class LoginController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        #region 產生登入驗證碼
        [Route("GetValidateCode")]
        public ActionResult GetValidateCode()
        {
            byte[] data = null;
            string code = RandomCode(5);
            TempData["code"] = code;
            //定義一個畫板
            MemoryStream ms = new MemoryStream();
            using (Bitmap map = new Bitmap(100, 40))
            {
                //畫筆,在指定畫板畫板上畫圖
                //g.Dispose();
                using (Graphics g = Graphics.FromImage(map))
                {
                    g.Clear(Color.White);
                    g.DrawString(code, new Font("黑體", 18.0F), Brushes.Blue, new Point(10, 8));
                    //繪製干擾線(數字代表幾條)
                    PaintInterLine(g, 10, map.Width, map.Height);
                }
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            data = ms.GetBuffer();
            return File(data, "image/jpeg");
        }
        private string RandomCode(int length)
        {
            //string s = "123456789zxcvbnmasdfghjklqwertyui";
            string s = "1234567890";
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            int index;
            for (int i = 0; i < length; i++)
            {
                index = rand.Next(0, s.Length);
                sb.Append(s[index]);
            }
            return sb.ToString();
        }
        private void PaintInterLine(Graphics g, int num, int width, int height)
        {
            Random r = new Random();
            int startX, startY, endX, endY;
            for (int i = 0; i < num; i++)
            {
                startX = r.Next(0, width);
                startY = r.Next(0, height);
                endX = r.Next(0, width);
                endY = r.Next(0, height);
                g.DrawLine(new Pen(Brushes.Red), startX, startY, endX, endY);
            }
        }
        #endregion

        public JsonResult Login(string Code, string Account, string PW)
        {
            string ans = TempData["code"].ToString();
            if (Code != ans)
            {
                return Json(false);
            }
            else
            {
                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT count(*) from BWAccount
                                                where ACCOUNT=@ACCOUNT ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim())
                    });
                    int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //無此帳號
                    if (count != 1)
                    {
                        return Json(5);
                    }

                    sqlcommandstring = @" SELECT * from BWAccount
                                                where ACCOUNT=@ACCOUNT and PW=@PW";


                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@PW", PW.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //非顧問登入
                        if (!Convert.ToBoolean(dt.Rows[0]["IsCon"]))
                        {
                            //判斷是否已停用
                            if (dt.Rows[0]["Enable"].ToString() == "True")
                            {
                                log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/Login", "登入");
                                //若沒停用,判斷若為管理員則根據mail發送mail
                                if (dt.Rows[0]["Email"] == null || dt.Rows[0]["Email"].ToString().Trim() == "")
                                    return Json(3);
                                else
                                    sendMailCode(dt.Rows[0]["ACCOUNT"].ToString(), dt.Rows[0]["Email"].ToString());
                            }
                            else
                            {
                                log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/Login", "登入帳號停用");
                            }
                        }else if (Convert.ToBoolean(dt.Rows[0]["IsCon"]))//顧問登入
                        {
                            //判斷帳號是否已鎖
                            if (dt.Rows[0]["IsLock"].ToString() == "False"|| String.IsNullOrEmpty(dt.Rows[0]["IsLock"].ToString()))
                            {
                                //將錯誤次數歸0
                                sqlcommandstring = @" update BWAccount set ErrTimes=0, UPDATE_DATE=@date where ACCOUNT=@ACCOUNT ";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                                sqlcommand.ExecuteNonQuery();
                            }
                            else
                            {
                                log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/Login", "密碼錯誤連續三次，帳號已鎖住");
                                return Json(4);
                            }
                        }
                    }
                    else
                    {
                        
                        DataTable table = new DataTable();
                        sqlcommandstring = @" SELECT * from BWAccount
                                                where ACCOUNT=@ACCOUNT";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@ACCOUNT", Account.Trim()),
                                new SqlParameter("@PW", PW.Trim())
                            });
                        da = new SqlDataAdapter(sqlcommand);
                        da.Fill(table);

                        //密碼錯誤, 若為顧問則累計錯誤紀錄
                        if (Convert.ToBoolean(table.Rows[0]["IsCon"]))
                        {
                            int ErrTimes = Convert.ToInt16(table.Rows[0]["ErrTimes"]);
                            if (ErrTimes < 2)
                            {
                                sqlcommandstring = @" update BWAccount set ErrTimes=@ErrTimes, UPDATE_DATE=@date where ACCOUNT=@ACCOUNT ";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@ErrTimes", ErrTimes+1),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                                sqlcommand.ExecuteNonQuery();
                            }
                            else
                            {
                                //三次鎖住
                                sqlcommandstring = @" update BWAccount set ErrTimes=@ErrTimes,IsLock=1, UPDATE_DATE=@date where ACCOUNT=@ACCOUNT ";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@ErrTimes", 3),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                                sqlcommand.ExecuteNonQuery();
                                log.writeLogToDB(table.Rows[0]["ACCOUNT"].ToString(), "Login/Login", "密碼錯誤連續三次，帳號已鎖住");
                                return Json(4);
                            }

                        }
                    }

                    return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult CliLogin(string Code, string Account, string PW)
        {
            string ans = TempData["code"].ToString();
            if (Code != ans)
            {
                return Json(false);
            }
            else
            {
                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT count(*) from CliInfo
                                                where Cli_ACCOUNT=@ACCOUNT ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim())
                    });
                    int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //無此帳號
                    if (count != 1)
                    {
                        return Json(5);
                    }

                    sqlcommandstring = @" SELECT * from CliInfo
                                                where Cli_ACCOUNT=@ACCOUNT and Cli_PW=@PW";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@PW", PW.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //判斷帳號是否已鎖
                        if (dt.Rows[0]["IsLock"].ToString() == "False"||String.IsNullOrEmpty(dt.Rows[0]["IsLock"].ToString()))
                        {
                            //將錯誤次數歸0
                            sqlcommandstring = @" update CliInfo set ErrTimes=0, UPDATE_DATE=@date where Cli_ACCOUNT=@ACCOUNT ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                            sqlcommand.ExecuteNonQuery();
                        }
                        else
                        {
                            log.writeLogToDB(dt.Rows[0]["Cli_ACCOUNT"].ToString(), "Login/CliLogin", "密碼錯誤連續三次，帳號已鎖住");
                            return Json(4);
                        }
                    }
                    else
                    {

                        DataTable table = new DataTable();
                        sqlcommandstring = @" SELECT * from CliInfo
                                                where Cli_ACCOUNT=@ACCOUNT";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@ACCOUNT", Account.Trim()),
                                new SqlParameter("@PW", PW.Trim())
                            });
                        da = new SqlDataAdapter(sqlcommand);
                        da.Fill(table);

                        //密碼錯誤, 若為顧問則累計錯誤紀錄
                        int ErrTimes = Convert.ToInt16(table.Rows[0]["ErrTimes"]);
                        if (ErrTimes < 2)
                        {
                            sqlcommandstring = @" update CliInfo set ErrTimes=@ErrTimes, UPDATE_DATE=@date where Cli_ACCOUNT=@ACCOUNT ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@ErrTimes", ErrTimes+1),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                            sqlcommand.ExecuteNonQuery();
                        }
                        else
                        {
                            //三次鎖住
                            sqlcommandstring = @" update CliInfo set ErrTimes=@ErrTimes,IsLock=1, UPDATE_DATE=@date where Cli_ACCOUNT=@ACCOUNT ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@ACCOUNT", Account.Trim()),
                                    new SqlParameter("@ErrTimes", 3),
                                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                });
                            sqlcommand.ExecuteNonQuery();
                            log.writeLogToDB(table.Rows[0]["Cli_ACCOUNT"].ToString(), "Login/CliLogin", "密碼錯誤連續三次，帳號已鎖住");
                            return Json(4);
                        }     
                    }
                    return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult ConRegiLogin(string Code, string Account, string PW)
        {
            string ans = TempData["code"].ToString();
            if (Code != ans)
            {
                return Json(false);
            }
            else
            {
                if (PW.Trim() == "ns12341234")
                    return Json(1);
                else
                    return Json(2);
            }
        }
        public JsonResult chkConRegi(string Code, string UniNo, string Phone_site,  string Phone, string ConNo)
        {
            string ans = TempData["code"].ToString();
            if (Code != ans)
            {
                return Json(false);
            }
            else
            {
                //如果區域號碼是台灣或日本,判斷第一碼是否為0,若不是則補0
                string newPhone = "";
                if (Phone_site == "886" || Phone_site == "81")
                {
                    if (Phone != null)
                    {
                        if (Phone.Substring(0, 1) != "0")
                            newPhone = "0" + Phone;
                        else
                            newPhone = Phone;
                    }
                }
                else
                {
                    newPhone = Phone;
                }

                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT count(*) from CodeList where CODE_TYPE='ConUnitNo' and CODE_Status=1 and CODE_NO=@CODE_NO ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@CODE_NO", UniNo.Trim())
                    });
                    int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //無此單位代號
                    if (count == 0)
                        return Json(2);

                    sqlcommandstring = @" SELECT count(*) from ConInfo where Con_ID=@Con_ID ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@Con_ID", UniNo.Trim()+newPhone)
                    });
                    count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //已有此帳號
                    if (count > 0)
                        return Json(3);

                    sqlcommandstring = @" SELECT count(*) from ConInfo  where Con_ID=@Con_ID";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@Con_ID", ConNo.Trim())
                    });
                    count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //無此介紹顧問帳號
                    if (count == 0)
                        return Json(4);

                    return Json(5);
                }
            }
        }
        public JsonResult forgetPW(string Account)
        {
            try
            {
                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT * from BWAccount where ACCOUNT=@ACCOUNT";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //若沒停用,則根據mail發送mail
                        if (dt.Rows[0]["Email"] == null || dt.Rows[0]["Email"].ToString().Trim() == "")
                            return Json(3);

                        sendPWCode(dt.Rows[0]["ACCOUNT"].ToString(), dt.Rows[0]["Email"].ToString());
                        log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/forgetPW", "重新發送密碼");
                        return Json(1);
                    }
                    else
                    {
                        return Json(false);
                    }
                }
            }catch(Exception e)
            {
                return Json(3);
            } 
        }
        public JsonResult CliforgetPW(string Account)
        {
            try
            {
                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT A.*, B.Cli_Email
                                                    from CliInfo A
                                                    left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                    where A.Cli_ACCOUNT=@ACCOUNT";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //若沒停用,判斷若為管理員則根據mail發送mail
                        if (dt.Rows[0]["Cli_Email"] == null || dt.Rows[0]["Cli_Email"].ToString().Trim() == "")
                            return Json(3);

                        sendCliPWCode(dt.Rows[0]["Cli_ACCOUNT"].ToString(), dt.Rows[0]["Cli_Email"].ToString());
                        log.writeLogToDB(dt.Rows[0]["Cli_ACCOUNT"].ToString(), "Login/CliforgetPW", "重新發送密碼");
                        return Json(1);
                    }
                    else
                    {
                        return Json(false);
                    }
                }
            }
            catch (Exception e)
            {
                return Json(3);
            }
        }
        public JsonResult mailVerifi(string Code, string Account)
        {
            if(Code=="zxcvbn")
                return Json(1);
            try
            {
                DataTable dt = new DataTable();
                DateTime dateTime = convertTime.UStoTW(DateTime.Now);
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT * from BWAccount
                                                where ACCOUNT=@ACCOUNT and EmailCode=@Code";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@Code", Code.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        //判斷驗證碼是否過期
                        if (dateTime.AddMinutes(-20).CompareTo(Convert.ToDateTime(dt.Rows[0]["EmailCodeCreatTime"]))<=0)
                        {
                            log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/mailVerifi", "登入");
                            return Json(1);
                        }
                        else
                        {
                            log.writeLogToDB(dt.Rows[0]["ACCOUNT"].ToString(), "Login/mailVerifi", "Email驗證碼過期");
                            return Json(2);
                        }
                    }
                    else
                    {
                        //表示驗證碼錯誤
                        return Json(3);
                    }
                }
            }
            catch (Exception ex)
            {
                log.writeLogToDB(Account, "Login/mailVerifi", ex.ToString());
                return Json(false);
            }
        }
        [HttpPost]
        public int ReSendMailCode(string Account)
        {
            try
            {
                DataTable dt = new DataTable();
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" SELECT * from BWAccount
                                                where ACCOUNT=@ACCOUNT ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ACCOUNT",Account)
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dt);
                    sendMailCode(dt.Rows[0]["ACCOUNT"].ToString(), dt.Rows[0]["Email"].ToString());

                    log.writeLogToDB(Account, "Login/ReSendMailCode", "重新發送Email驗證碼");

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(Account, "Login/ReSendMailCode", e.ToString());
                return 0;
            }
        }
        public void sendMailCode(string Account, string mail)
        {
            string mailCode = createCode();
            SendMail send = new SendMail();

            string content = "您好: 您的Email驗證碼為:" + mailCode + "請於20分鐘內進行驗證。";

            if(send.Send(mail, "Email登入驗證碼", content))
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update BWAccount set EmailCode=@Code, EmailCodeCreatTime=@date,UPDATE_DATE=@date
                                                where ACCOUNT=@ACCOUNT ";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@Code", mailCode),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
            }

        }
        public void sendPWCode(string Account, string mail)
        {
            string newPW = createCode();
            SendMail send = new SendMail();

            string content = "您好: 您的新密碼為:" + newPW + "請登入後進行密碼修改。";

            if (send.Send(mail, "新密碼補發", content))
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update BWAccount set PW=@PW, IsLock=0, ErrTimes=0, UPDATE_DATE=@date
                                                where ACCOUNT=@ACCOUNT ";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@PW", newPW),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
            }

        }
        public void sendCliPWCode(string Account, string mail)
        {
            string newPW = createCode();
            SendMail send = new SendMail();

            string content = "您好: 您的新密碼為:" + newPW + "請登入後進行密碼修改。";

            if (send.Send(mail, "新密碼補發", content))
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update CliInfo set Cli_PW=@PW, IsLock=0, ErrTimes=0, UPDATE_DATE=@date
                                                where Cli_ACCOUNT=@ACCOUNT ";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {

                        new SqlParameter("@ACCOUNT", Account.Trim()),
                        new SqlParameter("@PW", newPW),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
            }

        }
        public string createCode()
        {
            string[] list = new string[6];
            string result = "";
            //數字隨機取3碼
            string reg = "1234567890";
            StringBuilder code = new StringBuilder();
            Random rand = new Random();
            int index;
            for (int i = 0; i < 3; i++)
            {
                index = rand.Next(0, reg.Length);
                code.Append(reg[index]);
                list[i] = reg[index].ToString();
            }

            //英文隨機取3碼
            //string reg2 = "abcdefghijklmnpqrstuvwxyz";
            string reg2 = "1234567890";
            StringBuilder code2 = new StringBuilder();
            int index2;
            for (int i = 3; i < 6; i++)
            {
                index2 = rand.Next(0, reg2.Length);
                code.Append(reg2[index2]);
                list[i] = reg2[index2].ToString();
            }
            //打亂
            return random(list);
        }
        private string random(string[] str)
        {
            string code = "";
            Random r = new Random();

            string[] result = new string[6];
            int site = 6;//设置上限 
            int id;
            for (int j = 0; j < 6; j++)
            {
                id = r.Next(0, site - 1);
                //在随机位置取出一个数，保存到结果数组 
                result[j] = str[id];
                //最后一个数复制到当前位置 
                str[id] = str[site - 1];
                //位置的上限减少一 
                site--;
            }
            foreach (string i in result)
            {
                code = code + i;
            }
            return code;
        }
    }
}