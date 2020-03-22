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
    public class AccountController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetAccountRole(string Role_Name)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from SystemRole
                                                where ROLE_Name like N'%" + Role_Name.Trim() + "%' order by ROLE_Name asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetRoleAuth(int ROLE_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from SystemRole
                                                where ROLE_ID=@ROLE_ID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLE_ID", ROLE_ID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetAccount(string Account)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.* , B.ROLE_Name
                                                from BWAccount A
                                                left join SystemRole B on A.ROLE=B.ROLE_ID 
                                                where A.ACCOUNT like N'%" + Account.Trim() + "%' and A.IsCon=0 order by ACCOUNT asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliAccount()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from CliInfo  order by Cli_ID asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConAccount()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from BWAccount where IsCon=1  order by ACCOUNT asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public int AddNewRole(string RoleName)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"select count(*) from SystemRole where ROLE_Name=@ROLE_Name ";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLE_Name", RoleName)
                    });
                    int num = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    if (num > 0)
                        return 2;//表示已經有存在的名稱

                    sqlcommandstring = @"Insert INTO  SystemRole(ROLE_Name,Enable) values(@ROLE_Name,1) ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLE_Name", RoleName)
                    });
                    sqlcommand.ExecuteNonQuery();
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB("","Account/AddNewRole", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int DeleteRole(int ROLEID)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"select count(*) from BWAccount where ROLE=@ROLEID ";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLEID", ROLEID)
                    });
                    int num = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    if (num > 0)
                        return 2;//表示有人使用此角色

                    sqlcommandstring = @"Delete SystemRole where ROLE_ID=@ROLEID ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLEID", ROLEID)
                    });
                    sqlcommand.ExecuteNonQuery();
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "Account/DeleteRole", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int enableRole(int ROLEID, bool Enable, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    SqlCommand sqlcommand;
                    if (Enable == true)
                    {
                        sqlcommandstring = @"select count(*) from BWAccount where ROLE=@ROLEID ";

                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@ROLEID", ROLEID)
                            });
                        int num = Convert.ToInt16(sqlcommand.ExecuteScalar());

                        if (num > 0)
                            return 2;//表示有人使用此角色

                        sqlcommandstring = @"update SystemRole set Enable=0 where ROLE_ID=@ROLEID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ROLEID", ROLEID)
                        });
                        sqlcommand.ExecuteNonQuery();
                        log.writeLogToDB(LoginACCOUNT, "Account/enableRole", "停用角色:"+ ROLEID);
                    }
                    else
                    {
                        sqlcommandstring = @"update SystemRole set Enable=1 where ROLE_ID=@ROLEID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ROLEID", ROLEID)
                        });
                        sqlcommand.ExecuteNonQuery();
                        log.writeLogToDB(LoginACCOUNT, "Account/enableRole", "啟用角色:" + ROLEID);
                    }
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/enableRole", e.ToString());
                return 0;
            }
        }

        [HttpPost]
        public int EditRole(string LoginACCOUNT, int ROLEID, bool Auth_1, bool Auth_2, bool Auth_3, bool Auth_4, bool Auth_5, bool Auth_6, bool Auth_7, bool Auth_8, bool Auth_9
            , bool Auth_10, bool Auth_11, bool Auth_12, bool Auth_13, bool Auth_14, bool Auth_15, bool Auth_16, bool Auth_17, bool Auth_18, bool Auth_19, bool Auth_20)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update SystemRole set Auth_1=@Auth_1, Auth_2=@Auth_2, Auth_3=@Auth_3 , Auth_4=@Auth_4, Auth_5=@Auth_5, Auth_6=@Auth_6,
                                                Auth_7=@Auth_7, Auth_8=@Auth_8 , Auth_9=@Auth_9 , Auth_10=@Auth_10,Auth_11=@Auth_11,Auth_12=@Auth_12,Auth_13=@Auth_13,
                                                Auth_14=@Auth_14,Auth_15=@Auth_15,Auth_16=@Auth_16,Auth_17=@Auth_17,Auth_18=@Auth_18,Auth_19=@Auth_19,Auth_20=@Auth_20
                                                where ROLE_ID=@ROLEID ";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ROLEID", ROLEID),
                        new SqlParameter("@Auth_1", Auth_1),new SqlParameter("@Auth_2", Auth_2),new SqlParameter("@Auth_3", Auth_3),new SqlParameter("@Auth_4", Auth_4),
                        new SqlParameter("@Auth_5", Auth_5),new SqlParameter("@Auth_6", Auth_6),new SqlParameter("@Auth_7", Auth_7),new SqlParameter("@Auth_8", Auth_8),
                        new SqlParameter("@Auth_9", Auth_9),new SqlParameter("@Auth_10", Auth_10),new SqlParameter("@Auth_11", Auth_11),new SqlParameter("@Auth_12", Auth_12),
                        new SqlParameter("@Auth_13", Auth_13),new SqlParameter("@Auth_14", Auth_14),new SqlParameter("@Auth_15", Auth_15),new SqlParameter("@Auth_16", Auth_16),
                        new SqlParameter("@Auth_17", Auth_17),new SqlParameter("@Auth_18", Auth_18),new SqlParameter("@Auth_19", Auth_19),new SqlParameter("@Auth_20", Auth_20),
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Account/EditRole", "更新角色ID:"+ ROLEID + "之權限");

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/EditRole", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int enableAccount(string ID, bool Enable, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    SqlCommand sqlcommand;
                    if (Enable == true)
                    {
                        sqlcommandstring = @"update BWAccount set Enable=0 , UPDATE_DATE=@date where ID=@ID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ID", ID),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                        sqlcommand.ExecuteNonQuery();
                        log.writeLogToDB(LoginACCOUNT, "Account/enableAccount", "停用帳號ID為:"+ ID);

                    }
                    else
                    {
                        sqlcommandstring = @"update BWAccount set Enable=1, UPDATE_DATE=@date where ID=@ID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ID", ID),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                        sqlcommand.ExecuteNonQuery();
                        log.writeLogToDB(LoginACCOUNT, "Account/enableAccount", "啟用帳號ID為:" + ID);

                    }
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/enableAccount", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int EditAccountRole(string ID, string Role, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = @"update BWAccount set ROLE=@ROLE , UPDATE_DATE=@date where ID=@ID ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ID", ID),
                            new SqlParameter("@ROLE", Role),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Account/EditAccountRole", "設定帳號ID:" + ID + "之權限為" + Role);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/EditAccountRole", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int unLockCliAccount(string CliID, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //客戶密碼預設是身分證號碼後四碼
                    string sqlcommandstring = @"select Cli_ID_Num from  CliInfoDetail where Cli_ID=@Cli_ID  ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Cli_ID", CliID)
                        });
                    string IDNum = Convert.ToString(sqlcommand.ExecuteScalar()).Trim();

                    sqlcommandstring = @"update CliInfo set Cli_PW=@Cli_PW, IsLock=0, ErrTimes=0, UPDATE_DATE=@date where Cli_ID=@Cli_ID ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Cli_ID", CliID),
                            new SqlParameter("@Cli_PW", IDNum.Substring(IDNum.Length-4)),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Account/unLockCliAccount", "解鎖帳號客戶編號:"+ CliID);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/unLockCliAccount", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int unLockConAccount(string ID,string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //顧問密碼預設是身分證號碼後六碼
                    string sqlcommandstring = @"select Con_ID_Num from  ConInfoDetail
                                        where Con_ID=(select ACCOUNT from BWAccount where ID=@ID)  ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ID", ID)
                        });
                    string IDNum = Convert.ToString(sqlcommand.ExecuteScalar()).Trim();

                    sqlcommandstring = @"update BWAccount set PW=@PW, IsLock=0, ErrTimes=0, UPDATE_DATE=@date where ID=@ID ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@ID", ID),
                            new SqlParameter("@PW", IDNum.Substring(IDNum.Length-6)),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Account/unLockConAccount", "解鎖帳號顧問編號:" + ID);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Account/unLockConAccount", e.ToString());
                return 0;
            }
        }

        [HttpPost]
        public int AccountEditEmail(string Account, string Email)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update BWAccount set Email=@Email  where Account=@Account ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Account", Account),
                        new SqlParameter("@Email", Email)
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(Account, "Account/AccountEditEmail", "修改Email為:"+ Email);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(Account, "Account/AccountEditEmail", e.ToString());
                return 0;
            }
        }

        [HttpPost]
        public int AccountEditPW(string Account, string OldPW, string NewPW)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"select count(*) from BWAccount where Account=@Account and PW=@OldPW";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Account", Account),
                        new SqlParameter("@OldPW", OldPW)
                    });
                    int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //表示密碼錯誤
                    if(count==0)
                        return 2;

                    sqlcommandstring = @"update BWAccount set PW=@NewPW  where Account=@Account ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Account", Account),
                        new SqlParameter("@NewPW", NewPW)
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(Account, "Account/AccountEditPW", "修改PW為:" + NewPW);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(Account, "Account/AccountEditPW", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int cliAccountEditPW(string Account, string OldPW, string NewPW)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"select count(*) from CliInfo where Cli_ACCOUNT=@Account and Cli_PW=@OldPW";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Account", Account),
                        new SqlParameter("@OldPW", OldPW)
                    });
                    int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    //表示密碼錯誤
                    if (count == 0)
                        return 2;

                    sqlcommandstring = @"update CliInfo set Cli_PW=@NewPW  where Cli_ACCOUNT=@Account ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Account", Account),
                        new SqlParameter("@NewPW", NewPW)
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(Account, "Account/cliAccountEditPW", "修改PW為:" + NewPW);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(Account, "Account/cliAccountEditPW", e.ToString());
                return 0;
            }
        }
    }
}