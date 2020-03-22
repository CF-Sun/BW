using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class ConInfoController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();
        [HttpGet]
        public ActionResult GetConInfo( string ConID, string ConName)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select CI.Con_ID,(CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First)as ChiName,CASE CI.Parent_Con_ID WHEN '000' THEN N'股東' ELSE CI.Parent_Con_ID END as Parent_Con_ID,
                                                CASE CI.Parent_Con_ID WHEN '000' THEN N'股東' ELSE (CID2.Con_ChiNAME_Last+CID2.Con_ChiNAME_First) END as Parent_ChiName, CID.Con_Gender,
                                                CID.Con_Phone_site, CID.Con_Phone, CID.Con_Email, CI.Con_Hiera, CI.Con_SysHiera, Distri_Con_ID, Distri_Percentage,
												(CID3.Con_ChiNAME_Last+CID3.Con_ChiNAME_First)as DistriName
												from ConInfo CI
                                                left join ConInfoDetail CID on CI.Con_ID=CID.Con_ID
												left join ConInfoDetail CID2 on CI.Parent_Con_ID=CID2.Con_ID
												left join BonusDistribution BD on CI.Con_ID=BD.Con_ID
												left join ConInfoDetail CID3 on BD.Con_ID=CID3.Con_ID
                                                where CI.Con_ID!='000' and CI.Con_ID like '%" + ConID.Trim() + "%' and CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First like N'%" + ConName.Trim() + "%' ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConInfoByID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select *, convert(varchar, Con_BirthDay, 111) as Con_BirthDay_new from ConInfoDetail where Con_ID=@ConID ";

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
        public ActionResult GetParentCon(string ConID, string OriConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Con_ID, (B.Con_ChiNAME_Last+B.Con_ChiNAME_First) as Name
                                                from ConInfo A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where A.Con_ID!='000' and A.Con_ID=@ConID and SUBSTRING(A.Con_ID, 1, 3)=@head ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID),
                    new SqlParameter("@head", OriConID.Substring(0,3))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetDistributionParentCon(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Con_ID, (B.Con_ChiNAME_Last+B.Con_ChiNAME_First) as Name
                                                from ConInfo A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where A.Con_ID!='000' and A.Con_ID=@ConID ";

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
        public ActionResult GetHieraRecordByID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select * from ConHieraRecord where Con_ID=@ConID order by Record_DATE desc";

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
        public ActionResult GetCredentialsByID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select '1' as ID,'IDCardPos' as name, IsIDCardPos as IsHave, IDCardPos as fileName from ConInfoCredentials where Con_ID=@ConID
                                                union all
                                                select '2' as ID,'IDCardBack' as name, IsIDCardBack, IDCardBack from ConInfoCredentials  where Con_ID=@ConID
                                                union all
                                                select '3' as ID,'ResidenceProof' as name, IsResidenceProof, ResidenceProof from ConInfoCredentials where Con_ID=@ConID
                                                union  all
                                                select '4' as ID,'Consent' as name, IsConsent, Consent from ConInfoCredentials where Con_ID=@ConID";

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
        public ActionResult GetConHieraSettingByID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Con_SysHiera, A.IsAuto, A.Con_Hiera, 
                                            convert(varchar, B.EffectiveStartDate, 111) as EffectiveStartDate , 
                                            convert(varchar, B.EffectiveEndDate, 111) as EffectiveEndDate 
                                            from ConInfo A
                                            left join ConHieraSetting B on A.Con_ID=B.Con_ID 
                                                where A.Con_ID=@ConID";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        DataTable dtFilter = new DataTable();
        [HttpGet]
        public ActionResult GetConOrgTree(string ConID, string ConName)
        {
            DataTable dtALL = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            string[] Con_PATH;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取出全部
                string sqlcommandstring = @" select CI.Con_ID, CI.Parent_Con_ID, (CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First)as ChiName, CI.Con_Hiera
                                                from ConInfo CI
                                                left join ConInfoDetail CID on CI.Con_ID=CID.Con_ID 
                                                where CI.Con_ID!='000' ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dtALL);
                dtFilter = dtALL.Clone();

                if (ConID.Trim() == ""&& ConName.Trim() == "")
                {
                    getChild(dtALL, "000");
                    //return Json(dtALL.ToJson(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //取得搜尋的顧問的階層路徑
                    sqlcommandstring = @" select CI.Con_PATH from ConInfo CI
                                                left join ConInfoDetail CID on CI.Con_ID=CID.Con_ID
                                                where CI.Con_ID!='000' ";

                    if (ConID.Trim() != "")
                        sqlcommandstring += " and CI.Con_ID='" + ConID.Trim() + "'";
                    if(ConName.Trim() != "")
                        sqlcommandstring += " and CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First=N'" + ConName.Trim() + "'";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    Con_PATH = Convert.ToString(sqlcommand.ExecuteScalar()).Split('/');

                    //無資料則把空dt回傳
                    if(Con_PATH.Length<2)
                        return Json(dtFilter.ToJson(), JsonRequestBehavior.AllowGet);

                    //判斷父顧問或父父顧問是否為公司
                    if (Con_PATH.Length < 4) //取出所有人 包含公司
                    {
                        //return Json(dtALL.ToJson(), JsonRequestBehavior.AllowGet);
                        getChild(dtALL, "000");
                    }
                    else
                    {
                        string Con_Parent_ID = Con_PATH[Con_PATH.Length - 3];

                        //先把該顧問寫入結果dt 再找出所有子顧問
                        foreach (DataRow dr in dtALL.Select("Con_ID='"+ Con_Parent_ID + "'"))
                        {
                            dtFilter.ImportRow(dr);
                        }
                        getChild(dtALL, Con_Parent_ID);
                    }
                }
                return Json(dtFilter.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConOrgTreeByConID(string ConID)
        {
            DataTable dtALL = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            string[] Con_PATH;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取出全部
                string sqlcommandstring = @" select CI.Con_ID, CI.Parent_Con_ID, (CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First)as ChiName, CI.Con_Hiera
                                                from ConInfo CI
                                                left join ConInfoDetail CID on CI.Con_ID=CID.Con_ID 
                                                where CI.Con_ID!='000' ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dtALL);
                dtFilter = dtALL.Clone();


                //取得搜尋的顧問的階層路徑
                sqlcommandstring = @" select CI.Con_PATH from ConInfo CI
                                                left join ConInfoDetail CID on CI.Con_ID=CID.Con_ID
                                                where CI.Con_ID!='000' ";

                if (ConID.Trim() != "")
                    sqlcommandstring += " and CI.Con_ID='" + ConID.Trim() + "'";

                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                Con_PATH = Convert.ToString(sqlcommand.ExecuteScalar()).Split('/');

                //無資料則把空dt回傳
                if (Con_PATH.Length < 2)
                    return Json(dtFilter.ToJson(), JsonRequestBehavior.AllowGet);

                //先把該顧問寫入結果dt 再找出所有子顧問
                foreach (DataRow dr in dtALL.Select("Con_ID='" + ConID + "'"))
                {
                    dtFilter.ImportRow(dr);
                }
                getChild(dtALL, ConID);

                ////判斷父顧問或父父顧問是否為公司
                //if (Con_PATH.Length < 3) //取出所有人 包含公司
                //{
                //    //return Json(dtALL.ToJson(), JsonRequestBehavior.AllowGet);
                //    getChild(dtALL, "000");
                //}
                //else
                //{
                //    string Con_Parent_ID = Con_PATH[Con_PATH.Length - 2];

                //    //先把該顧問寫入結果dt 再找出所有子顧問
                //    foreach (DataRow dr in dtALL.Select("Con_ID='" + Con_Parent_ID + "'"))
                //    {
                //        dtFilter.ImportRow(dr);
                //    }
                //    getChild(dtALL, Con_Parent_ID);
                //}

                return Json(dtFilter.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        public void getChild(DataTable dtALL, string Parant_ID)
        {
            string condition = "Parent_Con_ID='" + Parant_ID + "'";
            foreach (DataRow dr in dtALL.Select(condition))
            {
                dtFilter.ImportRow(dr);
                getChild(dtALL, dr["Con_ID"].ToString());
            }
        }

        [HttpGet]
        public ActionResult GetHieraRecordData(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select CR.Con_ID, CR.Con_Hiera, (CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First)as ChiName,
                                                convert(varchar, Record_DATE, 111) as RecordDATE
                                                from ConHieraRecord  CR
                                                left join ConInfoDetail CID on CR.Con_ID=CID.Con_ID 
                                                where 1=1 ";
                if (startDate != "")
                {
                    sqlcommandstring += "and Record_DATE >= '" + startDate + "' ";
                }
                if (endDate != "")
                {
                    sqlcommandstring += "and Record_DATE <= '" + endDate + " 23:59:59.999' ";
                }
                sqlcommandstring += " order by Record_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int SaveConInfo(ConInfoDetail conInfo)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update ConInfoDetail set Con_ChiName_Last=@Con_ChiName_Last, Con_ChiName_First=@Con_ChiName_First, 
                                                Con_EngName_Last=@Con_EngName_Last, Con_EngNAME_First=@Con_EngNAME_First, Con_Gender=@Con_Gender,
                                                Con_ID_Num=@Con_ID_Num, Con_Passport=@Con_Passport, Con_BirthDay=@Con_BirthDay, Con_Census_Country=@Con_Census_Country,
                                                Con_Phone_site=@Con_Phone_site, Con_Phone=@Con_Phone, Con_Email=@Con_Email,
                                                Con_Live_Country=@Con_Live_Country, Con_Live_Province=@Con_Live_Province, Con_Live_City=@Con_Live_City,
                                                Con_Live_Address=@Con_Live_Address, Con_Eng_Address=@Con_Eng_Address, Con_PostalCode=@Con_PostalCode,
                                                UPDATE_DATE=@date where  Con_ID=@Con_ID;
                                                update BWAccount set Email=@Con_Email where ACCOUNT=@Con_ID; ";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", conInfo.Con_ID==null?"": conInfo.Con_ID),
                        new SqlParameter("@Con_ChiName_Last", conInfo.Con_ChiNAME_Last==null?"":conInfo.Con_ChiNAME_Last),
                        new SqlParameter("@Con_ChiName_First", conInfo.Con_ChiNAME_First==null?"":conInfo.Con_ChiNAME_First),
                        new SqlParameter("@Con_EngName_Last", conInfo.Con_EngNAME_Last==null?"":conInfo.Con_EngNAME_Last),
                        new SqlParameter("@Con_EngNAME_First", conInfo.Con_EngNAME_First==null?"":conInfo.Con_EngNAME_First),
                        new SqlParameter("@Con_Gender", conInfo.Con_Gender==null?"":conInfo.Con_Gender),
                        new SqlParameter("@Con_ID_Num", conInfo.Con_ID_Num==null?"":conInfo.Con_ID_Num),
                        new SqlParameter("@Con_Passport", conInfo.Con_Passport==null?"":conInfo.Con_Passport),
                        new SqlParameter("@Con_BirthDay", conInfo.Con_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)conInfo.Con_BirthDay),
                        new SqlParameter("@Con_Census_Country", conInfo.Con_Census_Country==null?"":conInfo.Con_Census_Country),
                        new SqlParameter("@Con_Phone_site", conInfo.Con_Phone_site==null?"":conInfo.Con_Phone_site),
                        new SqlParameter("@Con_Phone", conInfo.Con_Phone==null?"":conInfo.Con_Phone),
                        new SqlParameter("@Con_Email", conInfo.Con_Email==null?"":conInfo.Con_Email),
                        new SqlParameter("@Con_Live_Country", conInfo.Con_Live_Country==null?"":conInfo.Con_Live_Country),
                        new SqlParameter("@Con_Live_Province", conInfo.Con_Live_Province==null?"":conInfo.Con_Live_Province),
                        new SqlParameter("@Con_Live_City", conInfo.Con_Live_City==null?"":conInfo.Con_Live_City),
                        new SqlParameter("@Con_Live_Address", conInfo.Con_Live_Address==null?"":conInfo.Con_Live_Address),
                        new SqlParameter("@Con_Eng_Address", conInfo.Con_Eng_Address==null?"":conInfo.Con_Eng_Address),
                        new SqlParameter("@Con_PostalCode", conInfo.Con_PostalCode==null?"":conInfo.Con_PostalCode),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(conInfo.LoginACCOUNT, "ConInfo/SaveConInfo", "更新顧問編號:" + conInfo.Con_ID + "基本資料");

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(conInfo.LoginACCOUNT, "ConInfo/SaveConInfo", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int SaveHieraSetting(string Con_ID, string IsAuto, string EffectiveStartDate, string EffectiveEndDate, decimal Con_Hiera, decimal Con_SysHiera, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    if (IsAuto == "Manual")
                        sqlcommandstring = @" update ConInfo set Con_Hiera=@Con_Hiera, Con_SysHiera=@Con_SysHiera, IsAuto=@IsAuto, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    else if(IsAuto == "Auto")
                        sqlcommandstring = @" update ConInfo set Con_SysHiera=@Con_SysHiera, IsAuto=@IsAuto, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID==null?"": Con_ID),
                        new SqlParameter("@Con_Hiera", Con_Hiera),
                        new SqlParameter("@Con_SysHiera", Con_SysHiera),
                        new SqlParameter("@IsAuto", IsAuto=="Auto"?1: 0),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/SaveHieraSetting", "更新ConInfo,顧問編號:" + Con_ID + ",職級:" + Con_Hiera + ",體系:" + Con_SysHiera);


                    sqlcommandstring = @" Delete ConHieraSetting  where  Con_ID=@Con_ID
                                          INSERT ConHieraSetting values(@Con_ID, @Con_Hiera, @EffectiveStartDate, @EffectiveEndDate, @date, @date);";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID==null?"": Con_ID),
                        new SqlParameter("@Con_Hiera", Con_Hiera),
                        new SqlParameter("@EffectiveStartDate", EffectiveStartDate),
                        new SqlParameter("@EffectiveEndDate", EffectiveEndDate),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/SaveHieraSetting", "刪除ConHieraSetting,顧問編號:" + Con_ID + "並新增職級為" + Con_Hiera + "日期為" + EffectiveStartDate + "到" + EffectiveEndDate);

                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "ConInfo/SaveHieraSetting", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateCredentialStatus(string Con_ID, string ID, bool IsHave, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (ID == "1")
                        sqlcommandstring = @" update ConInfoCredentials set IsIDCardPos=@IsHave, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (ID == "2")
                        sqlcommandstring = @" update ConInfoCredentials set IsIDCardBack=@IsHave, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (ID == "3")
                        sqlcommandstring = @" update ConInfoCredentials set IsResidenceProof=@IsHave, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (ID == "4")
                        sqlcommandstring = @" update ConInfoCredentials set IsConsent=@IsHave, UPDATE_DATE=@date where  Con_ID=@Con_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID==null?"": Con_ID),
                        new SqlParameter("@IsHave", IsHave?false:true),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/UpdateCredentialStatus", "更新顧問編號:" + Con_ID + ",證件資料ID為" + ID);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "ConInfo/UpdateCredentialStatus", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int ChangeParentCon(string Con_ID, string Parent_Con_ID, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string OriginConID = "";
                string Path = "";
                string parenrtPath = "";
                string Parent_Parent_Con_ID;
                DataTable dt = new DataTable();
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection sqlconnection = new SqlConnection(conn))
                    {
                        sqlconnection.Open();

                        //檢查Parent_Con_ID的直推顧問是否是Con_ID
                        string sqlcommandstring = @" select Parent_Con_ID from ConInfo where Con_ID=@Con_ID";
                        SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Parent_Con_ID)
                        });
                        Parent_Parent_Con_ID = Convert.ToString(sqlcommand.ExecuteScalar());

                        //表示Parent_Con_ID的直推顧問為Con_ID
                        if (Parent_Parent_Con_ID.Trim() == Con_ID.Trim())
                            return 2;

                        //取得該顧問的path
                        sqlcommandstring = @" select Con_PATH from ConInfo where Con_ID=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        Path = Convert.ToString(sqlcommand.ExecuteScalar());

                        //取得原本parent的ConID
                        sqlcommandstring = @" select Parent_Con_ID from ConInfo where Con_ID=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        OriginConID = Convert.ToString(sqlcommand.ExecuteScalar());

                        //取得parent的path
                        sqlcommandstring = @" select Con_PATH from ConInfo where Con_ID=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Parent_Con_ID)
                        });
                        parenrtPath = Convert.ToString(sqlcommand.ExecuteScalar());


                        //更新此顧問的parentConID及path
                        sqlcommandstring = @" update ConInfo set Parent_Con_ID=@Parent_Con_ID, Con_PATH=@path, UPDATE_DATE=@date where Con_ID=@Con_ID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@Parent_Con_ID", Parent_Con_ID),
                                new SqlParameter("@path", parenrtPath.Trim()+"/"+Con_ID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                        //取得此顧問底下子顧問
                        sqlcommandstring = @" select Con_ID,Con_PATH from ConInfo where Con_PATH like '%"+ Path.Trim()+ "%' and Con_ID!=@Con_ID  ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID)
                            });
                        SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                        da.Fill(dt);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string newpath = dt.Rows[i]["Con_PATH"].ToString().Replace(Path.Trim(), parenrtPath.Trim() + "/" + Con_ID);
                            //更新path
                            sqlcommandstring = @" update ConInfo set Con_PATH=@path, UPDATE_DATE=@date where Con_ID=@Con_ID ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@path", newpath),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();
                        }

                        //寫入異動紀錄表
                        sqlcommandstring = @" insert into TransferRecord(Type, ConIDOrCliID, OriginParentConID, NewParentConID, CREATE_DATE)
                                                values('1', @Con_ID, @OriginParentConID, @NewParentConID, @date) ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@OriginParentConID", OriginConID.Trim()),
                                new SqlParameter("@NewParentConID", Parent_Con_ID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                    }
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/ChangeParentCon", Con_ID + "更新直推顧問編號為:" + Parent_Con_ID);

                    scope.Complete();
                }


                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "ConInfo/ChangeParentCon", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int ChangeDistributionParentCon(string Con_ID, string Parent_Con_ID, string Distri_Con_ID, int Distri_Percentage, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                DataTable dt = new DataTable();
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection sqlconnection = new SqlConnection(conn))
                    {
                        sqlconnection.Open();

                        string sqlcommandstring = "";

                        //判斷是否已有存在資料
                        sqlcommandstring = @" select count(*) from BonusDistribution where Con_ID=@Con_ID";
                        SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                        //如果Distri_Con_ID為空則刪除
                        if (Distri_Con_ID.Trim() == "")
                        {
                            sqlcommandstring = @" Delete BonusDistribution where Con_ID=@Con_ID";
                        }
                        else
                        {
                            //若已存在則更新 否則新增
                            if (count > 0)
                            {
                                sqlcommandstring = @" update BonusDistribution set Distri_Con_ID=@Distri_Con_ID, Distri_Percentage=@Distri_Percentage 
                                                        where Con_ID=@Con_ID";
                            }
                            else
                            {
                                sqlcommandstring = @" Insert into BonusDistribution(Con_ID,Parent_Con_ID,Distri_Con_ID,Distri_Percentage,CREATE_DATE,UPDATE_DATE)
                                                        values(@Con_ID,@Parent_Con_ID,@Distri_Con_ID,@Distri_Percentage,@date,@date)";
                            }
                        }
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID),
                            new SqlParameter("@Parent_Con_ID", Parent_Con_ID),
                            new SqlParameter("@Distri_Con_ID", Distri_Con_ID),
                            new SqlParameter("@Distri_Percentage", Distri_Percentage),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                        
                        sqlcommand.ExecuteNonQuery();

                    }
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/ChangeDistributionParentCon", Con_ID + "更新分享顧問編號為:" + Parent_Con_ID);

                    scope.Complete();
                }


                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "ConInfo/ChangeDistributionParentCon", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public string AddCon(ConInfoDetail conInfo)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string Con_ID = "";
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection sqlconnection = new SqlConnection(conn))
                    {
                        sqlconnection.Open();

                        //如果區域號碼是台灣或日本,判斷第一碼是否為0,若不是則補0
                        string newPhone = "";
                        if (conInfo.Con_Phone_site == "886" || conInfo.Con_Phone_site == "81")
                        {
                            if (conInfo.Con_Phone != null)
                            {
                                if (conInfo.Con_Phone.Substring(0, 1) != "0")
                                    newPhone = "0" + conInfo.Con_Phone;
                                else
                                    newPhone = conInfo.Con_Phone;
                            }
                        }
                        else
                        {
                            newPhone = conInfo.Con_Phone;
                        }

                        Con_ID = conInfo.UniNo + newPhone;

                        //檢查Con_ID是否重複
                        string sqlcommandstring = @" select count(*) from ConInfo where Con_ID=@Con_ID";
                        SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                        if (count > 0)
                            return "1";

                        //判斷身分證長度
                        if (conInfo.Con_ID_Num.Length < 6)
                            return "2";

                        //檢查Con_ID是否重複
                        sqlcommandstring = @" select count(*) from BWAccount where ACCOUNT=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                        if (count > 0)
                            return "1";

                        //寫入BWAccount
                        sqlcommandstring = @" Insert BWAccount (ACCOUNT,PW,IsCon,CREATE_DATE,UPDATE_DATE,Enable,IsLock,ErrTimes,Email) 
                                            values(@ACCOUNT,@PW,1,@date,@date,1,0,0,@Email)";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@ACCOUNT", Con_ID),
                                new SqlParameter("@PW", conInfo.Con_ID_Num==null?"":conInfo.Con_ID_Num.Substring(conInfo.Con_ID_Num.Length-6)),
                                new SqlParameter("@Email", conInfo.Con_Email==null?"":conInfo.Con_Email),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                        //取直推顧問的階層
                        string Con_PATH = "";
                        sqlcommandstring = @" select Con_PATH from ConInfo where Con_ID=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", conInfo.LoginACCOUNT)
                            });
                        Con_PATH = Convert.ToString(sqlcommand.ExecuteScalar());
                        if(Con_PATH.Trim()=="")
                            return "0";

                        //寫入ConInfo
                        sqlcommandstring = @" Insert ConInfo values(@Con_ID, @Parent_Con_ID, @Con_ROLE,null,@Con_PATH, 0, 0, 1, @date, @date)";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@Parent_Con_ID", conInfo.LoginACCOUNT),
                                new SqlParameter("@Con_ROLE", conInfo.LoginACCOUNT=="000"?"SHA":"CON"),
                                new SqlParameter("@Con_PATH", Con_PATH+"/"+Con_ID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();


                        //檢查Con_ID是否重複
                        sqlcommandstring = @" select count(*) from ConInfoDetail where Con_ID=@Con_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Con_ID", Con_ID)
                        });
                        count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                        if (count > 0)
                            return "1";

                        //寫入ConInfoDetail
                        sqlcommandstring = @" Insert ConInfoDetail values(@Con_ID, @Con_EngNAME_Last, @Con_EngNAME_First, @Con_ChiNAME_Last, @Con_ChiNAME_First,
                                                @Con_Gender, @Con_ID_Num, @Con_BirthDay, @Con_Census_Country, @Con_Phone_site, @Con_Phone, @Con_Email,
                                                @Con_Live_Country, @Con_Live_Province,@Con_Live_City,@Con_Live_Address,@Con_Eng_Address,@Con_PostalCode,
                                                @date, @date, @Con_Passport )";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@Con_EngNAME_Last", conInfo.Con_EngNAME_Last==null?"":conInfo.Con_EngNAME_Last),
                                new SqlParameter("@Con_EngNAME_First", conInfo.Con_EngNAME_First==null?"":conInfo.Con_EngNAME_First),
                                new SqlParameter("@Con_ChiNAME_Last", conInfo.Con_ChiNAME_Last==null?"":conInfo.Con_ChiNAME_Last),
                                new SqlParameter("@Con_ChiNAME_First", conInfo.Con_ChiNAME_First==null?"":conInfo.Con_ChiNAME_First),
                                new SqlParameter("@Con_Gender", conInfo.Con_Gender==null?"":conInfo.Con_Gender),
                                new SqlParameter("@Con_ID_Num", conInfo.Con_ID_Num==null?"":conInfo.Con_ID_Num),
                                new SqlParameter("@Con_PassPort", conInfo.Con_Passport==null?"":conInfo.Con_Passport),
                                new SqlParameter("@Con_BirthDay", conInfo.Con_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)conInfo.Con_BirthDay),
                                new SqlParameter("@Con_Census_Country", conInfo.Con_Census_Country==null?"":conInfo.Con_Census_Country),
                                new SqlParameter("@Con_Phone_site", conInfo.Con_Phone_site==null?"":conInfo.Con_Phone_site),
                                new SqlParameter("@Con_Phone", newPhone==null?"":newPhone),
                                new SqlParameter("@Con_Email", conInfo.Con_Email==null?"":conInfo.Con_Email),
                                new SqlParameter("@Con_Live_Country", conInfo.Con_Live_Country==null?"":conInfo.Con_Live_Country),
                                new SqlParameter("@Con_Live_Province", conInfo.Con_Live_Province==null?"":conInfo.Con_Live_Province),
                                new SqlParameter("@Con_Live_City", conInfo.Con_Live_City==null?"":conInfo.Con_Live_City),
                                new SqlParameter("@Con_Live_Address", conInfo.Con_Live_Address==null?"":conInfo.Con_Live_Address),
                                new SqlParameter("@Con_Eng_Address", conInfo.Con_Eng_Address==null?"":conInfo.Con_Eng_Address),
                                new SqlParameter("@Con_PostalCode", conInfo.Con_PostalCode==null?"":conInfo.Con_PostalCode),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                        //寫入ConInfoCredentials
                        sqlcommandstring = @" Insert ConInfoCredentials values(@Con_ID, @IsIDCardPos, @IsIDCardBack, @IsResidenceProof, @IsConsent,
                                               @IDCardPos, @IDCardBack, @ResidenceProof, @Consent, @date, @date )";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@IsIDCardPos", conInfo.FileName1==null?0:1),
                                new SqlParameter("@IsIDCardBack", conInfo.FileName2==null?0:1),
                                new SqlParameter("@IsResidenceProof", conInfo.FileName3==null?0:1),
                                new SqlParameter("@IsConsent", conInfo.FileName4==null?0:1),
                                new SqlParameter("@IDCardPos", conInfo.FileName1==null?"":"Credential_1"),
                                new SqlParameter("@IDCardBack", conInfo.FileName2==null?"":"Credential_2"),
                                new SqlParameter("@ResidenceProof", conInfo.FileName3==null?"":"Credential_3"),
                                new SqlParameter("@Consent", conInfo.FileName4==null?"":"Credential_4"),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                    }
                    log.writeLogToDB("", "ConInfo/AddCon", "新增報聘顧問ID為" + Con_ID + "的資料");

                    scope.Complete();
                }
                return Con_ID;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "ConInfo/AddCon", e.ToString());
                return "0";
            }
        }
        public virtual ActionResult UploadFile(string Con_ID, string CredentialID, string OriFileName, string LoginACCOUNT)
        {
            try
            {
                var fileName = "";

                var fileContent = Request.Files["UploadFile"];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    //判斷檔案大小,不能超過3MB
                    if (fileContent.ContentLength > 524288000)
                        return Json(new { isUploaded = false, result = "檔案大小不能超過500MB", filename = "" }, "text/html");


                    string extension = Path.GetExtension(fileContent.FileName).ToLower();
                    string filePath = Server.MapPath("~/Content/files/ConCredential/" + Con_ID.Trim() + "/");
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

                        if (!updateCredentials(Con_ID, CredentialID, fileName, LoginACCOUNT))//更新資料庫
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
                log.writeLogToDB("", "ConInfo/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool updateCredentials(string Con_ID, string CredentialID, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (CredentialID == "1")
                        sqlcommandstring = @" update ConInfoCredentials set IDCardPos=@fileName, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (CredentialID == "2")
                        sqlcommandstring = @" update ConInfoCredentials set IDCardBack=@fileName, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (CredentialID == "3")
                        sqlcommandstring = @" update ConInfoCredentials set ResidenceProof=@fileName, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";
                    if (CredentialID == "4")
                        sqlcommandstring = @" update ConInfoCredentials set Consent=@fileName, UPDATE_DATE=@date  where  Con_ID=@Con_ID ;";


                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "ConInfo/updateCredentials", "更新證件名稱,顧問編號:" + Con_ID.Trim() + ",證件名稱為" + fileName);

                }
                return true;
            }
            catch(Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "ConInfo/updateCredentials", ex.ToString());
                return false;
            }
        }
        //開啟檔案
        public ActionResult Open(string FileName, string Con_ID)
        {

            var path = Server.MapPath("/Content/files/ConCredential/" + Con_ID.Trim() + "/" + FileName);
            var mime = MimeMapping.GetMimeMapping(FileName);


            //var mime = "application/vnd.ms-powerpoint";

            return File(path, mime, FileName);
        }
        public class ConInfoDetail
        {
            public string Con_ID { get; set; }
            public string Con_ChiNAME_Last { get; set; }
            public string Con_ChiNAME_First { get; set; }
            public string Con_EngNAME_Last { get; set; }
            public string Con_EngNAME_First { get; set; }
            public string Con_Gender { get; set; }
            public string Con_ID_Num { get; set; }
            public string Con_Passport { get; set; }
            public string Con_BirthDay { get; set; }
            public string Con_Census_Country { get; set; }
            public string Con_Phone_site { get; set; }
            public string Con_Phone { get; set; }
            public string Con_Email { get; set; }
            public string Con_Live_Country { get; set; }
            public string Con_Live_Province { get; set; }
            public string Con_Live_City { get; set; }
            public string Con_Live_Address { get; set; }
            public string Con_Eng_Address { get; set; }
            public string Con_PostalCode { get; set; }
            public string FileName1 { get; set; }
            public string FileName2 { get; set; }
            public string FileName3 { get; set; }
            public string FileName4 { get; set; }
            public string UniNo { get; set; }
            public string LoginACCOUNT { get; set; }
        }
    }
}