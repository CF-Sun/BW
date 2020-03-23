using BW.Helpers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class CliInfoController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetCliInfo(string CliID, string CliName)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select CI.Cli_ID, CI.Cli_ROLE, (CID.Cli_ChiNAME_Last+CID.Cli_ChiNAME_First)as ChiName, 
                                                (CID.Cli_EngNAME_Last+CID.Cli_EngNAME_First)as EngNAME, CI.Con_ID, 
                                                (Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First)as ConChiName, CI.IsBuyCerti,
                                                CID.Cli_Gender, CID.Cli_Phone_site, CID.Cli_Phone, CID.Cli_Email, CID.Cli_Live_Address, CID.Cli_Eng_Address 
                                                from CliInfo CI
                                                left join CliInfoDetail CID on CI.Cli_ID=CID.Cli_ID
                                                left join ConInfoDetail Con on Con.Con_ID=CI.Con_ID
                                                where CI.Cli_ID like '%" + CliID .Trim()+ "%' and CID.Cli_ChiNAME_Last+CID.Cli_ChiNAME_First like N'%"+ CliName.Trim() + "%'";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliInfoByID(string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select *, convert(varchar, Cli_BirthDay, 111) as Cli_BirthDay_new from CliInfoDetail where Cli_ID=@CliID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliInfoByIDandConID(string CliID, string LoginACCOUNT)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select *, convert(varchar, Cli_BirthDay, 111) as Cli_BirthDay_new from CliInfoDetail where Cli_ID=@CliID and Con_ID=@Con_ID";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID),
                    new SqlParameter("@Con_ID", LoginACCOUNT)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliInfoByTempID(string TempID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.*, convert(varchar, Cli_BirthDay, 111) as Cli_BirthDay_new, B.Cli_ROLE
                                                from CliInfoDetail_Temp A
                                                left join CliInfo_Temp B on A.Temp_ID=B.Temp_ID
                                                where A.Temp_ID=@Temp_ID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@Temp_ID", TempID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliListByConID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Cli_ID, (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as ChiName
                                                from CliInfo A
                                                left join CliInfoDetail B on B.Cli_ID=A.Cli_ID            
                                                where A.Con_ID=@ConID ";

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
        public ActionResult GetTempCliInfoByConID(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Temp_ID, (A.Cli_ChiNAME_Last+A.Cli_ChiNAME_First)as ChiName, A.Cli_Phone_site, A.Cli_Phone, B.IsTemp,
                                            (A.Cli_EngNAME_Last+A.Cli_EngNAME_First)as EngNAME from CliInfoDetail_Temp A
                                            left join CliInfo_Temp B on A.Temp_ID=B.Temp_ID
                                            where A.Con_ID=@ConID order by A.UPDATE_DATE desc";

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
        public ActionResult GetCredentialsByID(string CliID, string CliROLE)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = "";

                if(CliROLE== "Individual")//個人戶
                {
                    sqlcommandstring = @"select '1' as ID,N'護照正面' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials where Cli_ID=@CliID
                                            union all
                                            select '2' as ID,N'護照反面', Isfile2, file2 from CliInfoCredentials  where Cli_ID=@CliID
                                            union all
                                            select '3' as ID,N'護照公證副本', Isfile3, file3 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '4' as ID,N'三個月內居住證明', Isfile4, file4 from CliInfoCredentials where Cli_ID=@CliID 
                                            union  all
                                            select '5' as ID,N'收息/出金銀行存簿', Isfile5, file5 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '6' as ID,N'申購書', Isfile6, file6 from CliInfoCredentials where Cli_ID=@CliID";

                }
                else if (CliROLE == "Corporate")//法人戶
                {
                    sqlcommandstring = @"select '1' as ID,N'公司註冊證書or商業登記證的公證副本' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials where Cli_ID=@CliID
                                            union all
                                            select '2' as ID,N'所有董事的公證名單', Isfile2, file2 from CliInfoCredentials  where Cli_ID=@CliID
                                            union all
                                            select '3' as ID,N'股東登記冊公證副本', Isfile3, file3 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '4' as ID,N'完好存續證明', Isfile4, file4 from CliInfoCredentials where Cli_ID=@CliID 
                                            union all
                                            select '5' as ID,N'公司備忘錄和章程的公證副本 (如有)', Isfile5, file5 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '6' as ID,N'董事與10％以上股東每人的居住證明公證副本', Isfile6, file6 from CliInfoCredentials where Cli_ID=@CliID 
                                            union all
                                            select '7' as ID,N'董事與10％以上股東每人的護照(包含簽名)公證副本', Isfile7, file7 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '8' as ID,N'收息/出金銀行存簿', Isfile8, file8 from CliInfoCredentials where Cli_ID=@CliID 
                                            union  all
                                            select '9' as ID,N'申購書', Isfile9, file9 from CliInfoCredentials where Cli_ID=@CliID ";
                }
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCredentialsByIDFromFront(string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = "";
                string CliROLE = "";

                //取得CliROLE
                sqlcommandstring = @"select Cli_ROLE from CliInfo where Cli_ID=@CliID";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID)
                    });
                CliROLE = Convert.ToString(sqlcommand.ExecuteScalar());

                if (CliROLE == "Individual")//個人戶
                {
                    sqlcommandstring = @"select '1' as ID,N'護照正面' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials where Cli_ID=@CliID
                                            union all
                                            select '2' as ID,N'護照反面', Isfile2, file2 from CliInfoCredentials  where Cli_ID=@CliID
                                            union all
                                            select '3' as ID,N'護照公證副本', Isfile3, file3 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '4' as ID,N'三個月內居住證明', Isfile4, file4 from CliInfoCredentials where Cli_ID=@CliID 
                                            union  all
                                            select '5' as ID,N'收息/出金銀行存簿', Isfile5, file5 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '6' as ID,N'申購書', Isfile6, file6 from CliInfoCredentials where Cli_ID=@CliID";

                }
                else if (CliROLE == "Corporate")//法人戶
                {
                    sqlcommandstring = @"select '1' as ID,N'公司註冊證書or商業登記證的公證副本' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials where Cli_ID=@CliID
                                            union all
                                            select '2' as ID,N'所有董事的公證名單', Isfile2, file2 from CliInfoCredentials  where Cli_ID=@CliID
                                            union all
                                            select '3' as ID,N'股東登記冊公證副本', Isfile3, file3 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '4' as ID,N'完好存續證明', Isfile4, file4 from CliInfoCredentials where Cli_ID=@CliID 
                                            union all
                                            select '5' as ID,N'公司備忘錄和章程的公證副本 (如有)', Isfile5, file5 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '6' as ID,N'董事與10％以上股東每人的居住證明公證副本', Isfile6, file6 from CliInfoCredentials where Cli_ID=@CliID 
                                            union all
                                            select '7' as ID,N'董事與10％以上股東每人的護照(包含簽名)公證副本', Isfile7, file7 from CliInfoCredentials where Cli_ID=@CliID
                                            union  all
                                            select '8' as ID,N'收息/出金銀行存簿', Isfile8, file8 from CliInfoCredentials where Cli_ID=@CliID 
                                            union  all
                                            select '9' as ID,N'申購書', Isfile9, file9 from CliInfoCredentials where Cli_ID=@CliID ";
                }
                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCredentialsByTempID(string TempID, string CliROLE, bool ischeck)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = "";

                if (CliROLE == "Individual")//個人戶
                {
                    if(ischeck)
                        sqlcommandstring = @"select '1' as ID,N'護照正面' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union all
                                            select '2' as ID,N'護照反面', Isfile2, file2 from CliInfoCredentials_Temp  where Temp_ID=@TempID
                                            union all
                                            select '3' as ID,N'護照公證副本', Isfile3, file3 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '4' as ID,N'三個月內居住證明', Isfile4, file4 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '5' as ID,N'收息/出金銀行存簿', Isfile5, file5 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '6' as ID,N'申購書', Isfile6, file6 from CliInfoCredentials_Temp where Temp_ID=@TempID";
                    else
                        sqlcommandstring = @"select '1' as ID,N'護照正面' as Name, 0 as IsHave
                                            union all
                                            select '2' as ID,N'護照反面', 0
                                            union all
                                            select '3' as ID,N'護照公證副本', 0 
                                            union  all
                                            select '4' as ID,N'三個月內居住證明', 0
                                            union  all
                                            select '5' as ID,N'收息/出金銀行存簿', 0
                                            union  all
                                            select '6' as ID,N'申購書', 0 ";

                }
                else if (CliROLE == "Corporate")//法人戶
                {
                    if (ischeck)
                        sqlcommandstring = @"select '1' as ID,N'公司註冊證書or商業登記證的公證副本' as Name, Isfile1 as IsHave, file1 as fileName from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union all
                                            select '2' as ID,N'所有董事的公證名單', Isfile2, file2 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union all
                                            select '3' as ID,N'股東登記冊公證副本', Isfile3, file3 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '4' as ID,N'完好存續證明', Isfile4, file4 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union all
                                            select '5' as ID,N'公司備忘錄和章程的公證副本 (如有)', Isfile5, file5 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '6' as ID,N'董事與10％以上股東每人的居住證明公證副本', Isfile6, file6 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union all
                                            select '7' as ID,N'董事與10％以上股東每人的護照(包含簽名)公證副本', Isfile7, file7 from CliInfoCredentials_Temp where Temp_ID=@TempID
                                            union  all
                                            select '8' as ID,N'收息/出金銀行存簿', Isfile8, file8 from CliInfoCredentials_Temp where Temp_ID=@TempID 
                                            union all
                                            select '9' as ID,N'申購書', Isfile9, file9 from CliInfoCredentials_Temp where Temp_ID = @TempID ";
                    else
                        sqlcommandstring = @"select '1' as ID,N'公司註冊證書or商業登記證的公證副本' as Name, 0 as IsHave
                                            union all
                                            select '2' as ID,N'所有董事的公證名單', 0
                                            union all
                                            select '3' as ID,N'股東登記冊公證副本', 0
                                            union  all
                                            select '4' as ID,N'完好存續證明', 0
                                            union all
                                            select '5' as ID,N'公司備忘錄和章程的公證副本 (如有)', 0
                                            union  all
                                            select '6' as ID,N'董事與10％以上股東每人的居住證明公證副本', 0
                                            union all
                                            select '7' as ID,N'董事與10％以上股東每人的護照(包含簽名)公證副本', 0
                                            union  all
                                            select '8' as ID,N'收息/出金銀行存簿', 0
                                            union  all
                                            select '9' as ID,N'申購書', 0";
                }
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@TempID", TempID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliID(string CliID, string CliName)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select Cli_ID from  CliInfoDetail where 1=1 ";
                if (CliID.Trim() != "")
                    sqlcommandstring += " and Cli_ID='" + CliID.Trim() + "'";
                if (CliName.Trim() != "")
                    sqlcommandstring += " and Cli_ChiNAME_Last+Cli_ChiNAME_First=N'" + CliName.Trim() + "'";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliList(string ConID, string ConName)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = @" select CL.Cli_ID, CL.Con_ID,(Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First)as ConChiName,
                                            (CLD.Cli_ChiNAME_Last+CLD.Cli_ChiNAME_First)as ChiName,CLD.Cli_EngNAME_Last, CLD.Cli_EngNAME_First,
                                            CLD.Cli_Gender,CLD.Cli_Phone_site, CLD.Cli_Phone, CLD.Cli_Email, CL.IsBuyCerti, 
                                            ISNULL(NetDeposit.NetDeposit,0)NetDeposit
                                            from CliInfo CL
                                            left join CliInfoDetail CLD on CL.Cli_ID=CLD.Cli_ID
                                            left join ConInfoDetail Con on CL.Con_ID=Con.Con_ID
                                            left join (select Cli_ID, SUM(Deposit_Amount)-SUM(ISNULL(Withdrawal_Amount,0)) as NetDeposit
                                                                                    from DepositList A 
                                                                                    left join
                                                                                    (select Deposit_ID, SUM(Withdrawal_Amount)as Withdrawal_Amount from WithdrawalList
		                                                                                    where  Status=2
		                                                                                    group by Deposit_ID) B on A.Deposit_ID=B.Deposit_ID
                                                                                    where A.Status=2 
                                                                                    group by Cli_ID) NetDeposit on NetDeposit.Cli_ID=CL.Cli_ID
                                            
                                            where 1=1 ";
                //string sqlcommandstring = @" select CL.Cli_ID, CL.Con_ID,(Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First)as ConChiName,
                //                            (CLD.Cli_ChiNAME_Last+CLD.Cli_ChiNAME_First)as ChiName,CLD.Cli_EngNAME_Last, CLD.Cli_EngNAME_First,
                //                            CLD.Cli_Gender,CLD.Cli_Phone_site, CLD.Cli_Phone, CLD.Cli_Email, CL.IsBuyCerti, 
                //                            ISNULL(Type1.NetDeposit,0)Type1,ISNULL(Type2.NetDeposit,0)Type2,ISNULL(Type3.NetDeposit,0)Type3
                //                            from CliInfo CL
                //                            left join CliInfoDetail CLD on CL.Cli_ID=CLD.Cli_ID
                //                            left join ConInfoDetail Con on CL.Con_ID=Con.Con_ID
                //                            left join (select Cli_ID, SUM(Deposit_Amount)as Deposit, Deposit_Type, SUM(ISNULL(Withdrawal_Amount,0)) as Withdrawal,
                //                                                                    SUM(Deposit_Amount)-SUM(ISNULL(Withdrawal_Amount,0)) as NetDeposit
                //                                                                    from DepositList A 
                //                                                                    left join
                //                                                                    (select Deposit_ID, SUM(Withdrawal_Amount)as Withdrawal_Amount from WithdrawalList
                //                                                                      where  Status=2
                //                                                                      group by Deposit_ID) B on A.Deposit_ID=B.Deposit_ID
                //                                                                    where A.Status=2 and A.Deposit_Type=1
                //                                                                    group by Cli_ID,Deposit_Type) Type1 on Type1.Cli_ID=CL.Cli_ID
                //                            left join (select Cli_ID, SUM(Deposit_Amount)as Deposit, Deposit_Type, SUM(ISNULL(Withdrawal_Amount,0)) as Withdrawal,
                //                                                                    SUM(Deposit_Amount)-SUM(ISNULL(Withdrawal_Amount,0)) as NetDeposit
                //                                                                    from DepositList A 
                //                                                                    left join
                //                                                                    (select Deposit_ID, SUM(Withdrawal_Amount)as Withdrawal_Amount from WithdrawalList
                //                                                                      where  Status=2
                //                                                                      group by Deposit_ID) B on A.Deposit_ID=B.Deposit_ID
                //                                                                    where A.Status=2 and A.Deposit_Type=2
                //                                                                    group by Cli_ID,Deposit_Type) Type2 on Type2.Cli_ID=CL.Cli_ID
                //                            left join (select Cli_ID, SUM(Deposit_Amount)as Deposit, Deposit_Type, SUM(ISNULL(Withdrawal_Amount,0)) as Withdrawal,
                //                                                                    SUM(Deposit_Amount)-SUM(ISNULL(Withdrawal_Amount,0)) as NetDeposit
                //                                                                    from DepositList A 
                //                                                                    left join
                //                                                                    (select Deposit_ID, SUM(Withdrawal_Amount)as Withdrawal_Amount from WithdrawalList
                //                                                                      where  Status=2
                //                                                                      group by Deposit_ID) B on A.Deposit_ID=B.Deposit_ID
                //                                                                    where A.Status=2 and A.Deposit_Type=3
                //                                                                    group by Cli_ID,Deposit_Type) Type3 on Type3.Cli_ID=CL.Cli_ID
                //                            where 1=1 ";

                if (ConID.Trim() != "")
                    sqlcommandstring += " and CL.Con_ID='" + ConID.Trim() + "'";
                if (ConName.Trim() != "")
                    sqlcommandstring += " and Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First=N'" + ConName.Trim() + "'";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCertiZipName()
        {
            DataRow row;
            DataTable dt = new DataTable();
            dt.Columns.Add("zipName", typeof(string));

            string FilePath = Server.MapPath("/Content/files/CliCerti/"); 

            if (Directory.Exists(FilePath))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(FilePath);
                FileInfo[] sortList = dirinfo.GetFiles();
                Array.Sort(sortList, new MyDateSorter());
                foreach (FileInfo item in sortList)
                {
                    //如果附檔名是壓縮檔才加入table
                    string extension = item.Extension.ToLower();
                    if (extension == ".zip" || extension == ".7z" || extension == ".rar")
                    {
                        row = dt.NewRow();
                        row["zipName"] = item.Name;
                        dt.Rows.Add(row);

                    }
                }
            }
            return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public string AddTempCli(CliInfoTempDetail cliInfo)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                //新建暫存檔
                if (cliInfo.TempID == null)
                {
                    string Temp_ID = "";
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection sqlconnection = new SqlConnection(conn))
                        {
                            sqlconnection.Open();

                            string sqlcommandstring = @" select ISNULL(max(Temp_ID),0) from CliInfo_Temp";
                            SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            Temp_ID = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                            while (Temp_ID.Length < 10)  //ID長度為10 不足長度補0
                            {
                                Temp_ID = "0" + Temp_ID;
                            }

                            //寫入CliInfo_Temp
                            sqlcommandstring = @" Insert CliInfo_Temp values(@Temp_ID, @Con_ID, @Cli_ROLE, 1, @date, @date)";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Temp_ID", Temp_ID),
                            new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                            new SqlParameter("@Cli_ROLE", cliInfo.Type==null?"":cliInfo.Type),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoDetail_Temp
                            sqlcommandstring = @" Insert CliInfoDetail_Temp values(@Temp_ID, @Con_ID, @Cli_EngNAME_Last, @Cli_EngNAME_First, @Cli_ChiNAME_Last, @Cli_ChiNAME_First,
                                               @Cli_Gender, @Cli_ID_Num, @Cli_PassPort, @Cli_BirthDay, @Cli_Census_Country, @Cli_Phone_site, @Cli_Phone, @Cli_Email,
                                                @Cli_Live_Address, @Cli_Eng_Address, @date, @date, @Cli_PostalCode )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Temp_ID", Temp_ID),
                            new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                            new SqlParameter("@Cli_EngNAME_Last", cliInfo.Cli_EngNAME_Last==null?"":cliInfo.Cli_EngNAME_Last),
                            new SqlParameter("@Cli_EngNAME_First", cliInfo.Cli_EngNAME_First==null?"":cliInfo.Cli_EngNAME_First),
                            new SqlParameter("@Cli_ChiNAME_Last", cliInfo.Cli_ChiNAME_Last==null?"":cliInfo.Cli_ChiNAME_Last),
                            new SqlParameter("@Cli_ChiNAME_First", cliInfo.Cli_ChiNAME_First==null?"":cliInfo.Cli_ChiNAME_First),
                            new SqlParameter("@Cli_Gender", cliInfo.Cli_Gender==null?"":cliInfo.Cli_Gender),
                            new SqlParameter("@Cli_ID_Num", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num),
                            new SqlParameter("@Cli_PassPort", cliInfo.Cli_PassPort==null?"":cliInfo.Cli_PassPort),
                            new SqlParameter("@Cli_BirthDay", cliInfo.Cli_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)cliInfo.Cli_BirthDay),
                            new SqlParameter("@Cli_Census_Country", cliInfo.Cli_Census_Country==null?"":cliInfo.Cli_Census_Country),
                            new SqlParameter("@Cli_Phone_site", cliInfo.Cli_Phone_site==null?"":cliInfo.Cli_Phone_site),
                            new SqlParameter("@Cli_Phone", cliInfo.Cli_Phone==null?"":cliInfo.Cli_Phone),
                            new SqlParameter("@Cli_Email", cliInfo.Cli_Email==null?"":cliInfo.Cli_Email),
                            new SqlParameter("@Cli_Live_Address", cliInfo.Cli_Live_Address==null?"":cliInfo.Cli_Live_Address),
                            new SqlParameter("@Cli_Eng_Address", cliInfo.Cli_Eng_Address==null?"":cliInfo.Cli_Eng_Address),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                            new SqlParameter("@Cli_PostalCode", cliInfo.Cli_PostalCode==null?"":cliInfo.Cli_PostalCode)

                        });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoCredentials_Temp
                            sqlcommandstring = @" Insert CliInfoCredentials_Temp values(@Temp_ID, @Con_ID, @Isfile1, @Isfile2, @Isfile3, @Isfile4,
                                               @Isfile5, @Isfile6, @Isfile7, @Isfile8, @Isfile9, @file1, @file2, @file3, @file4,
                                                @file5, @file6, @file7, @file8, @file9, @date, @date )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Temp_ID", Temp_ID),
                            new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                            new SqlParameter("@Isfile1", cliInfo.FileName1==null?0:1),
                            new SqlParameter("@Isfile2", cliInfo.FileName2==null?0:1),
                            new SqlParameter("@Isfile3", cliInfo.FileName3==null?0:1),
                            new SqlParameter("@Isfile4", cliInfo.FileName4==null?0:1),
                            new SqlParameter("@Isfile5", cliInfo.FileName5==null?0:1),
                            new SqlParameter("@Isfile6", cliInfo.FileName6==null?0:1),
                            new SqlParameter("@Isfile7", cliInfo.FileName7==null?0:1),
                            new SqlParameter("@Isfile8", cliInfo.FileName8==null?0:1),
                            new SqlParameter("@Isfile9", cliInfo.FileName9==null?0:1),
                            new SqlParameter("@file1", cliInfo.FileName1==null?"": "CliCredential_1"),
                            new SqlParameter("@file2", cliInfo.FileName2==null?"": "CliCredential_2"),
                            new SqlParameter("@file3", cliInfo.FileName3==null?"": "CliCredential_3"),
                            new SqlParameter("@file4", cliInfo.FileName4==null?"": "CliCredential_4"),
                            new SqlParameter("@file5", cliInfo.FileName5==null?"": "CliCredential_5"),
                            new SqlParameter("@file6", cliInfo.FileName6==null?"": "CliCredential_6"),
                            new SqlParameter("@file7", cliInfo.FileName7==null?"": "CliCredential_7"),
                            new SqlParameter("@file8", cliInfo.FileName8==null?"": "CliCredential_8"),
                            new SqlParameter("@file9", cliInfo.FileName9==null?"": "CliCredential_9"),
                            new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                        });
                            sqlcommand.ExecuteNonQuery();
                        }
                        log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddTempCli", "新增報客戶資料Temp_ID為" + Temp_ID + "的資料");

                        scope.Complete();
                    }
                    return Temp_ID;
                }
                else
                {
                    //更新暫存檔
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection sqlconnection = new SqlConnection(conn))
                        {
                            sqlconnection.Open();

                            //更新CliInfo_Temp
                            string sqlcommandstring = @" update CliInfo_Temp set Cli_ROLE=@Cli_ROLE, UPDATE_DATE=@date where Temp_ID=@Temp_ID";
                            SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Temp_ID", cliInfo.TempID==null?"":cliInfo.TempID),
                                new SqlParameter("@Cli_ROLE", cliInfo.Type==null?"":cliInfo.Type),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();

                            //更新CliInfoDetail_Temp
                            sqlcommandstring = @" update CliInfoDetail_Temp set Cli_ChiName_Last=@Cli_ChiName_Last, Cli_ChiName_First=@Cli_ChiName_First, 
                                                Cli_EngName_Last=@Cli_EngName_Last, Cli_EngNAME_First=@Cli_EngNAME_First, Cli_Gender=@Cli_Gender,
                                                Cli_ID_Num=@Cli_ID_Num, Cli_PassPort=@Cli_PassPort, Cli_BirthDay=@Cli_BirthDay, Cli_Census_Country=@Cli_Census_Country,
                                                Cli_Phone_site=@Cli_Phone_site, Cli_Phone=@Cli_Phone, Cli_Email=@Cli_Email,
                                                Cli_Live_Address=@Cli_Live_Address, Cli_Eng_Address=@Cli_Eng_Address, Cli_PostalCode=@Cli_PostalCode, UPDATE_DATE=@date 
                                                where Temp_ID=@Temp_ID ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Temp_ID", cliInfo.TempID==null?"":cliInfo.TempID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Cli_EngNAME_Last", cliInfo.Cli_EngNAME_Last==null?"":cliInfo.Cli_EngNAME_Last),
                                new SqlParameter("@Cli_EngNAME_First", cliInfo.Cli_EngNAME_First==null?"":cliInfo.Cli_EngNAME_First),
                                new SqlParameter("@Cli_ChiNAME_Last", cliInfo.Cli_ChiNAME_Last==null?"":cliInfo.Cli_ChiNAME_Last),
                                new SqlParameter("@Cli_ChiNAME_First", cliInfo.Cli_ChiNAME_First==null?"":cliInfo.Cli_ChiNAME_First),
                                new SqlParameter("@Cli_Gender", cliInfo.Cli_Gender==null?"":cliInfo.Cli_Gender),
                                new SqlParameter("@Cli_ID_Num", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num),
                                new SqlParameter("@Cli_PassPort", cliInfo.Cli_PassPort==null?"":cliInfo.Cli_PassPort),
                                new SqlParameter("@Cli_BirthDay", cliInfo.Cli_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)cliInfo.Cli_BirthDay),
                                new SqlParameter("@Cli_Census_Country", cliInfo.Cli_Census_Country==null?"":cliInfo.Cli_Census_Country),
                                new SqlParameter("@Cli_Phone_site", cliInfo.Cli_Phone_site==null?"":cliInfo.Cli_Phone_site),
                                new SqlParameter("@Cli_Phone", cliInfo.Cli_Phone==null?"":cliInfo.Cli_Phone),
                                new SqlParameter("@Cli_Email", cliInfo.Cli_Email==null?"":cliInfo.Cli_Email),
                                new SqlParameter("@Cli_Live_Address", cliInfo.Cli_Live_Address==null?"":cliInfo.Cli_Live_Address),
                                new SqlParameter("@Cli_Eng_Address", cliInfo.Cli_Eng_Address==null?"":cliInfo.Cli_Eng_Address),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                                new SqlParameter("@Cli_PostalCode", cliInfo.Cli_PostalCode==null?"":cliInfo.Cli_PostalCode)
                            });
                            sqlcommand.ExecuteNonQuery();

                            //更新CliInfoCredentials_Temp
                            sqlcommandstring = @" update CliInfoCredentials_Temp set UPDATE_DATE=@date ";

                            if (cliInfo.FileName1 != null && cliInfo.FileName1 != "V")
                                sqlcommandstring += @", Isfile1=@Isfile1, file1=@file1 ";
                            if (cliInfo.FileName2 != null && cliInfo.FileName2 != "V")
                                sqlcommandstring += @", Isfile2=@Isfile2, file2=@file2 ";
                            if (cliInfo.FileName3 != null && cliInfo.FileName3 != "V")
                                sqlcommandstring += @", Isfile3=@Isfile3, file3=@file3 ";
                            if (cliInfo.FileName4 != null && cliInfo.FileName4 != "V")
                                sqlcommandstring += @", Isfile4=@Isfile4, file4=@file4 ";
                            if (cliInfo.FileName5 != null && cliInfo.FileName5 != "V")
                                sqlcommandstring += @", Isfile5=@Isfile5, file5=@file5 ";
                            if (cliInfo.FileName6 != null && cliInfo.FileName6 != "V")
                                sqlcommandstring += @", Isfile6=@Isfile6, file6=@file6 ";
                            if (cliInfo.FileName7 != null && cliInfo.FileName7 != "V")
                                sqlcommandstring += @", Isfile7=@Isfile7, file7=@file7 ";
                            if (cliInfo.FileName8 != null && cliInfo.FileName8 != "V")
                                sqlcommandstring += @", Isfile8=@Isfile8, file8=@file8 ";
                            if (cliInfo.FileName9 != null && cliInfo.FileName9 != "V")
                                sqlcommandstring += @", Isfile9=@Isfile9, file9=@file9 ";

                            sqlcommandstring += @" where Temp_ID=@Temp_ID ";

                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Temp_ID", cliInfo.TempID==null?"":cliInfo.TempID),
                                new SqlParameter("@Isfile1", cliInfo.FileName1==null?0:1),
                                new SqlParameter("@Isfile2", cliInfo.FileName2==null?0:1),
                                new SqlParameter("@Isfile3", cliInfo.FileName3==null?0:1),
                                new SqlParameter("@Isfile4", cliInfo.FileName4==null?0:1),
                                new SqlParameter("@Isfile5", cliInfo.FileName5==null?0:1),
                                new SqlParameter("@Isfile6", cliInfo.FileName6==null?0:1),
                                new SqlParameter("@Isfile7", cliInfo.FileName7==null?0:1),
                                new SqlParameter("@Isfile8", cliInfo.FileName8==null?0:1),
                                new SqlParameter("@Isfile9", cliInfo.FileName9==null?0:1),
                                new SqlParameter("@file1", cliInfo.FileName1==null?"": "CliCredential_1"),
                                new SqlParameter("@file2", cliInfo.FileName2==null?"": "CliCredential_2"),
                                new SqlParameter("@file3", cliInfo.FileName3==null?"": "CliCredential_3"),
                                new SqlParameter("@file4", cliInfo.FileName4==null?"": "CliCredential_4"),
                                new SqlParameter("@file5", cliInfo.FileName5==null?"": "CliCredential_5"),
                                new SqlParameter("@file6", cliInfo.FileName6==null?"": "CliCredential_6"),
                                new SqlParameter("@file7", cliInfo.FileName7==null?"": "CliCredential_7"),
                                new SqlParameter("@file8", cliInfo.FileName8==null?"": "CliCredential_8"),
                                new SqlParameter("@file9", cliInfo.FileName9==null?"": "CliCredential_9"),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();
                        }
                        log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddTempCli", "更新報客戶資料Temp_ID為" + cliInfo.TempID + "的資料");

                        scope.Complete();
                    }
                    return cliInfo.TempID;
                }

            }
            catch (Exception e)
            {
                log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddTempCli", e.ToString());
                return "0";
            }
        }
        [HttpGet]
        public ActionResult GetParentCon(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select A.Con_ID, (B.Con_ChiNAME_Last+B.Con_ChiNAME_First) as Name
                                                from ConInfo A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where A.Con_ID!='000' and SUBSTRING(A.Con_ID, 1, 3)=@head ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID),
                    new SqlParameter("@head", ConID.Substring(0,3))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public string chkRepeatPhone(string Phone_site, string Phone, string LoginACCOUNT)
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            string Cli_ID = "";
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

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

                Cli_ID = LoginACCOUNT.Substring(0, 2) + newPhone;
                //檢查Cli_ID是否重複
                string sqlcommandstring = @" select count(*) from CliInfo where Cli_ID=@Cli_ID";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID)
                            });
                int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                if (count > 0)
                {
                    return "1";
                }
                else
                {
                    //檢查Cli_ID是否重複
                    sqlcommandstring = @" select count(*) from CliInfoDetail where Cli_ID=@Cli_ID";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID)
                            });
                    count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                    if (count > 0)
                        return "1";
                    else
                        return "0";
                }
            }
        }
        [HttpPost]
        public string AddCli(CliInfoTempDetail cliInfo)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                if (cliInfo.TempID == null)
                {
                    string Cli_ID = "";
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection sqlconnection = new SqlConnection(conn))
                        {
                            sqlconnection.Open();

                            //如果區域號碼是台灣或日本,判斷第一碼是否為0,若不是則補0
                            string newPhone = "";
                            if (cliInfo.Cli_Phone_site == "886" || cliInfo.Cli_Phone_site == "81")
                            {
                                if (cliInfo.Cli_Phone != null)
                                {
                                    if (cliInfo.Cli_Phone.Substring(0, 1) != "0")
                                        newPhone = "0" + cliInfo.Cli_Phone;
                                    else
                                        newPhone = cliInfo.Cli_Phone;
                                }
                            }
                            else
                            {
                                newPhone = cliInfo.Cli_Phone;
                            }


                            Cli_ID = cliInfo.LoginACCOUNT.Substring(0, 2) + newPhone;

                            ////檢查Cli_ID是否重複
                            //string sqlcommandstring = @" select count(*) from CliInfo where Cli_ID=@Cli_ID";
                            //SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            //sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            //    new SqlParameter("@Cli_ID", Cli_ID)
                            //});
                            //int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                            //if (count > 0)
                            //    return "1";

                            if (cliInfo.Cli_ID_Num.Length < 4)
                                return "2";

                            //寫入CliInfo
                            string sqlcommandstring = @" Insert CliInfo values(@Cli_ID, @Con_ID, @Cli_ACCOUNT,@Cli_PW, @Cli_ROLE, 0, '','', @date, @date,0,0)";
                            SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Cli_ACCOUNT", Cli_ID==null?"":Cli_ID),
                                new SqlParameter("@Cli_PW", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num.Substring(cliInfo.Cli_ID_Num.Length-4)),
                                new SqlParameter("@Cli_ROLE", cliInfo.Type==null?"":cliInfo.Type),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoDetail
                            sqlcommandstring = @" Insert CliInfoDetail values(@Cli_ID, @Con_ID, @Cli_EngNAME_Last, @Cli_EngNAME_First, @Cli_ChiNAME_Last, @Cli_ChiNAME_First,
                                               @Cli_Gender, @Cli_ID_Num, @Cli_PassPort, @Cli_BirthDay, @Cli_Census_Country, @Cli_Phone_site, @Cli_Phone, @Cli_Email,
                                                @Cli_Live_Address, @Cli_Eng_Address, @date, @date, @Cli_PostalCode )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Cli_EngNAME_Last", cliInfo.Cli_EngNAME_Last==null?"":cliInfo.Cli_EngNAME_Last),
                                new SqlParameter("@Cli_EngNAME_First", cliInfo.Cli_EngNAME_First==null?"":cliInfo.Cli_EngNAME_First),
                                new SqlParameter("@Cli_ChiNAME_Last", cliInfo.Cli_ChiNAME_Last==null?"":cliInfo.Cli_ChiNAME_Last),
                                new SqlParameter("@Cli_ChiNAME_First", cliInfo.Cli_ChiNAME_First==null?"":cliInfo.Cli_ChiNAME_First),
                                new SqlParameter("@Cli_Gender", cliInfo.Cli_Gender==null?"":cliInfo.Cli_Gender),
                                new SqlParameter("@Cli_ID_Num", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num),
                                new SqlParameter("@Cli_PassPort", cliInfo.Cli_PassPort==null?"":cliInfo.Cli_PassPort),
                                new SqlParameter("@Cli_BirthDay", cliInfo.Cli_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)cliInfo.Cli_BirthDay),
                                new SqlParameter("@Cli_Census_Country", cliInfo.Cli_Census_Country==null?"":cliInfo.Cli_Census_Country),
                                new SqlParameter("@Cli_Phone_site", cliInfo.Cli_Phone_site==null?"":cliInfo.Cli_Phone_site),
                                new SqlParameter("@Cli_Phone", newPhone==null?"":newPhone),
                                new SqlParameter("@Cli_Email", cliInfo.Cli_Email==null?"":cliInfo.Cli_Email),
                                new SqlParameter("@Cli_Live_Address", cliInfo.Cli_Live_Address==null?"":cliInfo.Cli_Live_Address),
                                new SqlParameter("@Cli_Eng_Address", cliInfo.Cli_Eng_Address==null?"":cliInfo.Cli_Eng_Address),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                                new SqlParameter("@Cli_PostalCode", cliInfo.Cli_PostalCode==null?"":cliInfo.Cli_PostalCode)
                            });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoCredentials
                            sqlcommandstring = @" Insert CliInfoCredentials values(@Cli_ID, @Con_ID, @Isfile1, @Isfile2, @Isfile3, @Isfile4,
                                               @Isfile5, @Isfile6, @Isfile7, @Isfile8, @Isfile9, @file1, @file2, @file3, @file4,
                                                @file5, @file6, @file7, @file8, @file9, @date, @date )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Isfile1", cliInfo.FileName1==null?0:1),
                                new SqlParameter("@Isfile2", cliInfo.FileName2==null?0:1),
                                new SqlParameter("@Isfile3", cliInfo.FileName3==null?0:1),
                                new SqlParameter("@Isfile4", cliInfo.FileName4==null?0:1),
                                new SqlParameter("@Isfile5", cliInfo.FileName5==null?0:1),
                                new SqlParameter("@Isfile6", cliInfo.FileName6==null?0:1),
                                new SqlParameter("@Isfile7", cliInfo.FileName7==null?0:1),
                                new SqlParameter("@Isfile8", cliInfo.FileName8==null?0:1),
                                new SqlParameter("@Isfile9", cliInfo.FileName9==null?0:1),
                                new SqlParameter("@file1", cliInfo.FileName1==null?"": "CliCredential_1"),
                                new SqlParameter("@file2", cliInfo.FileName2==null?"": "CliCredential_2"),
                                new SqlParameter("@file3", cliInfo.FileName3==null?"": "CliCredential_3"),
                                new SqlParameter("@file4", cliInfo.FileName4==null?"": "CliCredential_4"),
                                new SqlParameter("@file5", cliInfo.FileName5==null?"": "CliCredential_5"),
                                new SqlParameter("@file6", cliInfo.FileName6==null?"": "CliCredential_6"),
                                new SqlParameter("@file7", cliInfo.FileName7==null?"": "CliCredential_7"),
                                new SqlParameter("@file8", cliInfo.FileName8==null?"": "CliCredential_8"),
                                new SqlParameter("@file9", cliInfo.FileName9==null?"": "CliCredential_9"),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();
                        }
                        log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddCli", "新增報客戶資料ID為" + Cli_ID + "的資料");

                        scope.Complete();
                    }
                    return Cli_ID;
                }
                else
                {
                    string Cli_ID = "";
                    using (TransactionScope scope = new TransactionScope())
                    {
                        using (SqlConnection sqlconnection = new SqlConnection(conn))
                        {
                            sqlconnection.Open();

                            //如果區域號碼是台灣或日本,判斷第一碼是否為0,若不是則補0
                            string newPhone = "";
                            if (cliInfo.Cli_Phone_site == "886" || cliInfo.Cli_Phone_site == "81")
                            {
                                if (cliInfo.Cli_Phone != null)
                                {
                                    if (cliInfo.Cli_Phone.Substring(0, 1) != "0")
                                        newPhone = "0" + cliInfo.Cli_Phone;
                                    else
                                        newPhone = cliInfo.Cli_Phone;
                                }
                            }
                            else
                            {
                                newPhone = cliInfo.Cli_Phone;
                            }

                            Cli_ID = cliInfo.LoginACCOUNT.Substring(0, 2) + newPhone;

                            ////檢查Cli_ID是否重複
                            //string sqlcommandstring = @" select count(*) from CliInfo where Cli_ID=@Cli_ID";
                            //SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            //sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            //    new SqlParameter("@Cli_ID", Cli_ID)
                            //});
                            //int count = Convert.ToInt16(sqlcommand.ExecuteScalar());

                            //if (count > 0)
                            //    return "1";

                            if (cliInfo.Cli_ID_Num.Length < 4)
                                return "2";

                            //寫入CliInfo
                            string sqlcommandstring = @" Insert CliInfo values(@Cli_ID, @Con_ID, @Cli_ACCOUNT,@Cli_PW, @Cli_ROLE, 0, '','', @date, @date,0,0)";
                            SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Cli_ACCOUNT", Cli_ID==null?"":Cli_ID),
                                new SqlParameter("@Cli_PW", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num.Substring(cliInfo.Cli_ID_Num.Length-4)),
                                new SqlParameter("@Cli_ROLE", cliInfo.Type==null?"":cliInfo.Type),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoDetail
                            sqlcommandstring = @" Insert CliInfoDetail values(@Cli_ID, @Con_ID, @Cli_EngNAME_Last, @Cli_EngNAME_First, @Cli_ChiNAME_Last, @Cli_ChiNAME_First,
                                               @Cli_Gender, @Cli_ID_Num, @Cli_PassPort, @Cli_BirthDay, @Cli_Census_Country, @Cli_Phone_site, @Cli_Phone, @Cli_Email,
                                                @Cli_Live_Address, @Cli_Eng_Address, @date, @date, @Cli_PostalCode )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Cli_EngNAME_Last", cliInfo.Cli_EngNAME_Last==null?"":cliInfo.Cli_EngNAME_Last),
                                new SqlParameter("@Cli_EngNAME_First", cliInfo.Cli_EngNAME_First==null?"":cliInfo.Cli_EngNAME_First),
                                new SqlParameter("@Cli_ChiNAME_Last", cliInfo.Cli_ChiNAME_Last==null?"":cliInfo.Cli_ChiNAME_Last),
                                new SqlParameter("@Cli_ChiNAME_First", cliInfo.Cli_ChiNAME_First==null?"":cliInfo.Cli_ChiNAME_First),
                                new SqlParameter("@Cli_Gender", cliInfo.Cli_Gender==null?"":cliInfo.Cli_Gender),
                                new SqlParameter("@Cli_ID_Num", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num),
                                new SqlParameter("@Cli_PassPort", cliInfo.Cli_PassPort==null?"":cliInfo.Cli_PassPort),
                                new SqlParameter("@Cli_BirthDay", cliInfo.Cli_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)cliInfo.Cli_BirthDay),
                                new SqlParameter("@Cli_Census_Country", cliInfo.Cli_Census_Country==null?"":cliInfo.Cli_Census_Country),
                                new SqlParameter("@Cli_Phone_site", cliInfo.Cli_Phone_site==null?"":cliInfo.Cli_Phone_site),
                                new SqlParameter("@Cli_Phone", newPhone==null?"":newPhone),
                                new SqlParameter("@Cli_Email", cliInfo.Cli_Email==null?"":cliInfo.Cli_Email),
                                new SqlParameter("@Cli_Live_Address", cliInfo.Cli_Live_Address==null?"":cliInfo.Cli_Live_Address),
                                new SqlParameter("@Cli_Eng_Address", cliInfo.Cli_Eng_Address==null?"":cliInfo.Cli_Eng_Address),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                                new SqlParameter("@Cli_PostalCode", cliInfo.Cli_PostalCode==null?"":cliInfo.Cli_PostalCode)
                            });
                            sqlcommand.ExecuteNonQuery();

                            //寫入CliInfoCredentials
                            sqlcommandstring = @" Insert CliInfoCredentials values(@Cli_ID, @Con_ID, @Isfile1, @Isfile2, @Isfile3, @Isfile4,
                                               @Isfile5, @Isfile6, @Isfile7, @Isfile8, @Isfile9, @file1, @file2, @file3, @file4,
                                                @file5, @file6, @file7, @file8, @file9, @date, @date )";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Con_ID", cliInfo.LoginACCOUNT==null?"":cliInfo.LoginACCOUNT),
                                new SqlParameter("@Isfile1", cliInfo.FileName1==null?0:cliInfo.FileName1=="V"?0:1),
                                new SqlParameter("@Isfile2", cliInfo.FileName2==null?0:cliInfo.FileName2=="V"?0:1),
                                new SqlParameter("@Isfile3", cliInfo.FileName3==null?0:cliInfo.FileName3=="V"?0:1),
                                new SqlParameter("@Isfile4", cliInfo.FileName4==null?0:cliInfo.FileName4=="V"?0:1),
                                new SqlParameter("@Isfile5", cliInfo.FileName5==null?0:cliInfo.FileName5=="V"?0:1),
                                new SqlParameter("@Isfile6", cliInfo.FileName6==null?0:cliInfo.FileName6=="V"?0:1),
                                new SqlParameter("@Isfile7", cliInfo.FileName7==null?0:cliInfo.FileName7=="V"?0:1),
                                new SqlParameter("@Isfile8", cliInfo.FileName8==null?0:cliInfo.FileName8=="V"?0:1),
                                new SqlParameter("@Isfile9", cliInfo.FileName9==null?0:cliInfo.FileName9=="V"?0:1),
                                new SqlParameter("@file1", cliInfo.FileName1==null?"":cliInfo.FileName1=="V"?"": "CliCredential_1"),
                                new SqlParameter("@file2", cliInfo.FileName2==null?"":cliInfo.FileName2=="V"?"": "CliCredential_2"),
                                new SqlParameter("@file3", cliInfo.FileName3==null?"":cliInfo.FileName3=="V"?"": "CliCredential_3"),
                                new SqlParameter("@file4", cliInfo.FileName4==null?"":cliInfo.FileName4=="V"?"": "CliCredential_4"),
                                new SqlParameter("@file5", cliInfo.FileName5==null?"":cliInfo.FileName5=="V"?"": "CliCredential_5"),
                                new SqlParameter("@file6", cliInfo.FileName6==null?"":cliInfo.FileName6=="V"?"": "CliCredential_6"),
                                new SqlParameter("@file7", cliInfo.FileName7==null?"":cliInfo.FileName7=="V"?"": "CliCredential_7"),
                                new SqlParameter("@file8", cliInfo.FileName8==null?"":cliInfo.FileName8=="V"?"": "CliCredential_8"),
                                new SqlParameter("@file9", cliInfo.FileName9==null?"":cliInfo.FileName9=="V"?"": "CliCredential_9"),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();

                            //將原本temp更新回CliInfoCredentials
                            sqlcommandstring = @" update CliInfoCredentials  set  Con_ID=CliInfoCredentials_Temp.Con_ID ";

                            if (cliInfo.FileName1 == null|| cliInfo.FileName1 == "V")
                                sqlcommandstring += @", Isfile1=CliInfoCredentials_Temp.Isfile1 , file1=CliInfoCredentials_Temp.file1 ";
                            if (cliInfo.FileName2 == null || cliInfo.FileName2 == "V")
                                sqlcommandstring += @", Isfile2=CliInfoCredentials_Temp.Isfile2 , file2=CliInfoCredentials_Temp.file2 ";
                            if (cliInfo.FileName3 == null || cliInfo.FileName3 == "V")
                                sqlcommandstring += @", Isfile3=CliInfoCredentials_Temp.Isfile3 , file3=CliInfoCredentials_Temp.file3 ";
                            if (cliInfo.FileName4 == null || cliInfo.FileName4 == "V")
                                sqlcommandstring += @", Isfile4=CliInfoCredentials_Temp.Isfile4 , file4=CliInfoCredentials_Temp.file4 ";
                            if (cliInfo.FileName5 == null || cliInfo.FileName5 == "V")
                                sqlcommandstring += @", Isfile5=CliInfoCredentials_Temp.Isfile5 , file5=CliInfoCredentials_Temp.file5 ";
                            if (cliInfo.FileName6 == null || cliInfo.FileName6 == "V")
                                sqlcommandstring += @", Isfile6=CliInfoCredentials_Temp.Isfile6 , file6=CliInfoCredentials_Temp.file6 ";
                            if (cliInfo.FileName7 == null || cliInfo.FileName7 == "V")
                                sqlcommandstring += @", Isfile7=CliInfoCredentials_Temp.Isfile7 , file7=CliInfoCredentials_Temp.file7 ";
                            if (cliInfo.FileName8 == null || cliInfo.FileName8 == "V")
                                sqlcommandstring += @", Isfile8=CliInfoCredentials_Temp.Isfile8 , file8=CliInfoCredentials_Temp.file8 ";
                            if (cliInfo.FileName9 == null || cliInfo.FileName9 == "V")
                                sqlcommandstring += @", Isfile9=CliInfoCredentials_Temp.Isfile9 , file9=CliInfoCredentials_Temp.file9 ";

                            sqlcommandstring += @" from CliInfoCredentials, CliInfoCredentials_Temp
                                                    where CliInfoCredentials.Cli_ID=@Cli_ID and CliInfoCredentials_Temp.Temp_ID=@Temp_ID ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@Temp_ID", cliInfo.TempID==null?"":cliInfo.TempID)
                            });
                            sqlcommand.ExecuteNonQuery();

                            //更新CliInfo_temp為已建檔
                            sqlcommandstring = @" update CliInfo_Temp set IsTemp=0, UPDATE_DATE=@date where Temp_ID=@Temp_ID";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Temp_ID", cliInfo.TempID==null?"":cliInfo.TempID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                            sqlcommand.ExecuteNonQuery();

                            //將原本該tempID下的檔案搬至正式資料夾內
                            FileInfo[] fileList = null;
                            string tmpPath= Server.MapPath("~/Content/files/CliCredential_temp/" + cliInfo.TempID.Trim() + "/");
                            string desPath = Server.MapPath("~/Content/files/CliCredential/" + Cli_ID.Trim() + "/");
                            
                            //正式資料夾不存在則新增資料夾
                            if (!Directory.Exists(desPath))
                                Directory.CreateDirectory(desPath);

                            //取的Temp資料夾下所有檔名
                            if (Directory.Exists(tmpPath))
                            {
                                DirectoryInfo di = new DirectoryInfo(tmpPath);
                                fileList = di.GetFiles();
                                for (int i = 0; i < fileList.Length; i++)
                                {
                                    string fileName = tmpPath + @"\" + fileList.GetValue(i).ToString();
                                    string desFileName = desPath + @"\" + fileList.GetValue(i).ToString();
                                    System.IO.File.Copy(fileName, desFileName, true);
                                }
                            }

                        }
                        log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddCli", "新增報客戶資料ID為" + Cli_ID + "的資料");

                        scope.Complete();
                    }
                    return Cli_ID;
                }
            }
            catch (Exception e)
            {
                log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/AddCli", e.ToString());
                return "0";
            }
        }
        [HttpPost]
        public int SaveCliInfo(CliInfoDetail cliInfo)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @" update CliInfoDetail set Cli_ChiName_Last=@Cli_ChiName_Last, Cli_ChiName_First=@Cli_ChiName_First, 
                                                Cli_EngName_Last=@Cli_EngName_Last, Cli_EngNAME_First=@Cli_EngNAME_First, Cli_Gender=@Cli_Gender,
                                                Cli_ID_Num=@Cli_ID_Num, Cli_PassPort=@Cli_PassPort, Cli_BirthDay=@Cli_BirthDay, Cli_Census_Country=@Cli_Census_Country,
                                                Cli_Phone_site=@Cli_Phone_site, Cli_Phone=@Cli_Phone, Cli_Email=@Cli_Email,
                                                Cli_Live_Address=@Cli_Live_Address, Cli_Eng_Address=@Cli_Eng_Address,Cli_PostalCode=@Cli_PostalCode, UPDATE_DATE=@date 
                                                where  Cli_ID=@Cli_ID";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Cli_ID", cliInfo.Cli_ID==null?"": cliInfo.Cli_ID),
                        new SqlParameter("@Cli_ChiName_Last", cliInfo.Cli_ChiNAME_Last==null?"":cliInfo.Cli_ChiNAME_Last),
                        new SqlParameter("@Cli_ChiName_First", cliInfo.Cli_ChiNAME_First==null?"":cliInfo.Cli_ChiNAME_First),
                        new SqlParameter("@Cli_EngName_Last", cliInfo.Cli_EngNAME_Last==null?"":cliInfo.Cli_EngNAME_Last),
                        new SqlParameter("@Cli_EngNAME_First", cliInfo.Cli_EngNAME_First==null?"":cliInfo.Cli_EngNAME_First),
                        new SqlParameter("@Cli_Gender", cliInfo.Cli_Gender==null?"":cliInfo.Cli_Gender),
                        new SqlParameter("@Cli_ID_Num", cliInfo.Cli_ID_Num==null?"":cliInfo.Cli_ID_Num),
                        new SqlParameter("@Cli_PassPort", cliInfo.Cli_PassPort==null?"":cliInfo.Cli_PassPort),
                        new SqlParameter("@Cli_BirthDay", cliInfo.Cli_BirthDay==null?System.Data.SqlTypes.SqlDateTime.Null:(object)cliInfo.Cli_BirthDay),
                        new SqlParameter("@Cli_Census_Country", cliInfo.Cli_Census_Country==null?"":cliInfo.Cli_Census_Country),
                        new SqlParameter("@Cli_Phone_site", cliInfo.Cli_Phone_site==null?"":cliInfo.Cli_Phone_site),
                        new SqlParameter("@Cli_Phone", cliInfo.Cli_Phone==null?"":cliInfo.Cli_Phone),
                        new SqlParameter("@Cli_Email", cliInfo.Cli_Email==null?"":cliInfo.Cli_Email),
                        new SqlParameter("@Cli_Live_Address", cliInfo.Cli_Live_Address==null?"":cliInfo.Cli_Live_Address),
                        new SqlParameter("@Cli_Eng_Address", cliInfo.Cli_Eng_Address==null?"":cliInfo.Cli_Eng_Address),
                        new SqlParameter("@Cli_PostalCode", cliInfo.Cli_PostalCode==null?"":cliInfo.Cli_PostalCode),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
                log.writeLogToDB(cliInfo.LoginACCOUNT, "CliInfo/SaveCliInfo", "修改客戶編號為"+ cliInfo.Cli_ID + "的資料");

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(cliInfo.LoginACCOUNT,"CliInfo/SaveCliInfo", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateCredentialStatus(string Cli_ID, string ID, bool IsHave, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (ID == "1")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile1=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "2")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile2=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "3")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile3=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "4")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile4=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "5")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile5=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "6")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile6=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "7")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile7=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "8")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile8=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (ID == "9")
                        sqlcommandstring = @" update CliInfoCredentials set Isfile9=@IsHave, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Cli_ID", Cli_ID==null?"": Cli_ID),
                        new SqlParameter("@IsHave", IsHave?false:true),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "CliInfo/UpdateCredentialStatus", "更新客戶編號:" + Cli_ID + ",證件資料ID為" + ID);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "CliInfo/UpdateCredentialStatus", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int ChangeParentCon(string Cli_ID, string Con_ID, string LoginACCOUNT)
        {
            try
            {
                string OriginConID = "";

                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection sqlconnection = new SqlConnection(conn))
                    {
                        sqlconnection.Open();

                        //取得原本parent的ConID
                        string sqlcommandstring = @" select Con_ID from CliInfo where Cli_ID=@Cli_ID";
                        SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Cli_ID", Cli_ID)
                        });
                        OriginConID = Convert.ToString(sqlcommand.ExecuteScalar());

                        //更新此顧問的parentConID及path
                        sqlcommandstring = @"update CliInfo set Con_ID=@Con_ID, UPDATE_DATE=@date where  Cli_ID=@Cli_ID;
                                                    update CliInfoCredentials set Con_ID=@Con_ID, UPDATE_DATE=@date where  Cli_ID=@Cli_ID;
                                                    update CliInfoDetail set Con_ID=@Con_ID, UPDATE_DATE=@date where  Cli_ID=@Cli_ID;
                                                    update DepositList set Con_ID=@Con_ID, UPDATE_DATE=@date where  Cli_ID=@Cli_ID;
                                                    update WithdrawalList set Con_ID=@Con_ID, UPDATE_DATE=@date where  Cli_ID=@Cli_ID; ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Con_ID),
                                new SqlParameter("@Cli_ID", Cli_ID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();

                        //寫入異動紀錄表
                        sqlcommandstring = @" insert into TransferRecord(Type, ConIDOrCliID, OriginParentConID, NewParentConID, CREATE_DATE)
                                                values('2', @Con_ID, @OriginParentConID, @NewParentConID, @date) ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Con_ID", Cli_ID),
                                new SqlParameter("@OriginParentConID", OriginConID.Trim()),
                                new SqlParameter("@NewParentConID", Con_ID),
                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                            });
                        sqlcommand.ExecuteNonQuery();
                    }
                    log.writeLogToDB(LoginACCOUNT, "CliInfo/ChangeParentCon", Cli_ID + "更新所屬顧問編號為:" + Con_ID);

                    scope.Complete();
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "CliInfo/ChangeParentCon", e.ToString());
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
                log.writeLogToDB("", "CliInfo/chkFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult chkFileForBigFile()
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
                log.writeLogToDB("", "CliInfo/chkFile6", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult UploadFile(string Cli_ID, string CredentialID, string OriFileName, string LoginACCOUNT)
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
                    string filePath = Server.MapPath("~/Content/files/CliCredential/" + Cli_ID.Trim() + "/");
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

                        if (!updateCredentials(Cli_ID, CredentialID, fileName, LoginACCOUNT))//更新資料庫
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
                log.writeLogToDB(LoginACCOUNT, "CliInfo/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult UploadTempFile(string Temp_ID,string fileNo, string LoginACCOUNT)
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
                    string filePath = Server.MapPath("~/Content/files/CliCredential_temp/" + Temp_ID.Trim() + "/");
                    fileName = fileContent.FileName;
                    string fileLocation = filePath + "CliCredential_" + fileNo + Path.GetExtension(fileContent.FileName);

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

                        if (!updateTempCredentials(Temp_ID, fileNo, "CliCredential_" + fileNo + Path.GetExtension(fileContent.FileName), LoginACCOUNT))//更新資料庫
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
                log.writeLogToDB(LoginACCOUNT, "CliInfo/UploadTempFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult UploadNewFile(string Cli_ID, string fileNo, string LoginACCOUNT)
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
                    string filePath = Server.MapPath("~/Content/files/CliCredential/" + Cli_ID.Trim() + "/");
                    fileName = fileContent.FileName;
                    string fileLocation = filePath + "CliCredential_" + fileNo + Path.GetExtension(fileContent.FileName);

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

                        if (!updateNewCredentials(Cli_ID, fileNo, "CliCredential_" + fileNo + Path.GetExtension(fileContent.FileName), LoginACCOUNT))//更新資料庫
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
                log.writeLogToDB(LoginACCOUNT, "CliInfo/UploadTempFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool updateCredentials(string Cli_ID, string CredentialID, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (CredentialID == "1")
                        sqlcommandstring = @" update CliInfoCredentials set file1=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "2")
                        sqlcommandstring = @" update CliInfoCredentials set file2=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "3")
                        sqlcommandstring = @" update CliInfoCredentials set file3=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "4")
                        sqlcommandstring = @" update CliInfoCredentials set file4=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "5")
                        sqlcommandstring = @" update CliInfoCredentials set file5=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "6")
                        sqlcommandstring = @" update CliInfoCredentials set file6=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "7")
                        sqlcommandstring = @" update CliInfoCredentials set file7=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "8")
                        sqlcommandstring = @" update CliInfoCredentials set file8=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "9")
                        sqlcommandstring = @" update CliInfoCredentials set file9=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Cli_ID", Cli_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "CliInfo/updateCredentials", "更新客戶編號:" + Cli_ID.Trim() + ",證件資料ID為" + CredentialID+",檔名為"+ fileName);

                }
                return true;
            }
            catch(Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "CliInfo/updateCredentials", ex.ToString());
                return false;
            }
        }
        public bool updateNewCredentials(string Cli_ID, string CredentialID, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (CredentialID == "1" || CredentialID == "7")
                        sqlcommandstring = @" update CliInfoCredentials set file1=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "2" || CredentialID == "8")
                        sqlcommandstring = @" update CliInfoCredentials set file2=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "3" || CredentialID == "9")
                        sqlcommandstring = @" update CliInfoCredentials set file3=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "4" || CredentialID == "10")
                        sqlcommandstring = @" update CliInfoCredentials set file4=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "5" || CredentialID == "11")
                        sqlcommandstring = @" update CliInfoCredentials set file5=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "6" || CredentialID == "12")
                        sqlcommandstring = @" update CliInfoCredentials set file6=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "13")
                        sqlcommandstring = @" update CliInfoCredentials set file7=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "14")
                        sqlcommandstring = @" update CliInfoCredentials set file8=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";
                    if (CredentialID == "15")
                        sqlcommandstring = @" update CliInfoCredentials set file9=@fileName, UPDATE_DATE=@date  where  Cli_ID=@Cli_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Cli_ID", Cli_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "CliInfo/updateCredentials", "更新客戶編號:" + Cli_ID.Trim() + ",證件資料ID為" + CredentialID + ",檔名為" + fileName);

                }
                return true;
            }
            catch (Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "CliInfo/updateCredentials", ex.ToString());
                return false;
            }
        }
        public bool updateTempCredentials(string Temp_ID, string fileNo, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = "";
                    if (fileNo == "1"|| fileNo == "7")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file1=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "2" || fileNo == "8")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file2=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "3" || fileNo == "9")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file3=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "4" || fileNo == "10")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file4=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "5" || fileNo == "11")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file5=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "6" || fileNo == "12")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file6=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "13")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file7=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "14")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file8=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";
                    if (fileNo == "15")
                        sqlcommandstring = @" update CliInfoCredentials_Temp set file9=@fileName, UPDATE_DATE=@date  where  Temp_ID=@Temp_ID ;";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Temp_ID", Temp_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "CliInfo/updateTempCredentials", "更新客戶編號:" + Temp_ID.Trim() + ",證件資料ID為" + fileNo + ",檔名為" + fileName);

                }
                return true;
            }
            catch (Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "CliInfo/updateCredentials", ex.ToString());
                return false;
            }
        }
        public ActionResult Open(string FileName, string Cli_ID)
        {

            var path = Server.MapPath("/Content/files/CliCredential/" + Cli_ID.Trim() + "/" + FileName);
            var mime = MimeMapping.GetMimeMapping(FileName);
            return File(path, mime, FileName);
        }
        public ActionResult OpenCertiZip(string FileName)
        {

            var path = Server.MapPath("/Content/files/CliCerti/" + FileName.Trim());
            var mime = MimeMapping.GetMimeMapping(FileName);
            return File(path, mime, FileName);
        }
        public virtual ActionResult UploadCertiZip()
        {
            try
            {
                var fileName = "";

                var fileContent = Request.Files["UploadFile"];
                if (fileContent != null && fileContent.ContentLength > 0)
                {
                    //判斷檔案大小,不能超過3MB
                    //if (fileContent.ContentLength > 3145728)
                    //    return Json(new { isUploaded = false, result = "檔案大小不能超過3MB", filename = "" }, "text/html");

                    string extension = Path.GetExtension(fileContent.FileName).ToLower();
                    string filePath = Server.MapPath("~/Content/files/CliCerti/");
                    fileName = fileContent.FileName;
                    //fileName = DateTime.Now.ToString("yyyyMMdd") + extension; //以當天日期為檔名
                    string fileLocation = filePath + fileName;

                    //判斷檔名是否含有特殊字元
                    Validate validate = new Validate();
                    if (!validate.ValidateString(fileName))
                        return Json(new { isUploaded = false, result = "檔名不能包含特殊字元'&'", filename = "" }, "text/html");

                    if (extension == ".zip" || extension == ".7z" || extension == ".rar")
                    {
                        //路徑不存在則新增資料夾
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (System.IO.File.Exists(fileLocation))// 驗證檔案是否存在
                        { 
                            fileLocation = filePath + Path.GetFileNameWithoutExtension(fileContent.FileName) + "(1)" + extension;
                            fileName= Path.GetFileNameWithoutExtension(fileContent.FileName) + "(1)" + extension;
                        }
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
                log.writeLogToDB("", "CliInfo/UploadCertiZip", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public virtual ActionResult UploadCertiFile()
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
                    string filePath = Server.MapPath("~/Content/files/CliCerti/");
                    fileName = fileContent.FileName;
                    string fileLocation = filePath + fileName;

                    //判斷檔名是否含有特殊字元
                    Validate validate = new Validate();
                    if (!validate.ValidateString(fileName))
                        return Json(new { isUploaded = false, result = "檔名不能包含特殊字元'&'", filename = "" }, "text/html");

                    if (extension == ".xls" || extension == ".xlsx")
                    {
                        //路徑不存在則新增資料夾
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (System.IO.File.Exists(fileLocation)) // 驗證檔案是否存在
                            System.IO.File.Delete(fileLocation);

                        Request.Files["UploadFile"].SaveAs(fileLocation); // 存放檔案到伺服器上

                        if (!insertData(fileLocation))
                            return Json(new { isUploaded = false, result = "憑證狀態更新失敗", filename = "" }, "text/html");
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
                log.writeLogToDB("", "CliInfo/UploadCertiFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool insertData(string fileLocation)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                // 建立一個工作簿
                XSSFWorkbook excel;

                // 檔案讀取
                using (FileStream files = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
                {
                    excel = new XSSFWorkbook(files); // 將剛剛的Excel 讀取進入到工作簿中
                }
                // Excel 的哪一個活頁簿
                ISheet sheet = excel.GetSheetAt(0);
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    SqlCommand sqlcommand;


                    for (int row = 1; row <= sheet.LastRowNum; row++) // 使用For 走訪所有的資料列
                    {
                        //--------------insert value--------
                        string Cli_ID = "";
                        string IsBuyCerti = "";
                        string CertiFileName = "";
                        string CertiZipName = "";
                        //----------------------------------

                        if (sheet.GetRow(row) != null) // 驗證是不是空白列
                        {
                            if (sheet.GetRow(row).GetCell(0) != null)
                                Cli_ID = sheet.GetRow(row).GetCell(0).ToString(); // 字串
                            if (Cli_ID == "") //空白行跳過
                                continue;
                            if (sheet.GetRow(row).GetCell(1) != null)
                                IsBuyCerti = sheet.GetRow(row).GetCell(1).ToString();
                            if (sheet.GetRow(row).GetCell(2) != null)
                                CertiFileName = sheet.GetRow(row).GetCell(2).ToString();
                            if (sheet.GetRow(row).GetCell(3) != null)
                                CertiZipName = sheet.GetRow(row).GetCell(3).ToString();

                            sqlcommandstring = @" update CliInfo set IsBuyCerti=@IsBuyCerti, CertiFileName=@CertiFileName, CertiZipName=@CertiZipName, UPDATE_DATE=@date 
                                                    where  Cli_ID=@Cli_ID";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                                new SqlParameter("@Cli_ID", Cli_ID),
                                                new SqlParameter("@IsBuyCerti", IsBuyCerti=="Y"?true:false),
                                                new SqlParameter("@CertiFileName", CertiFileName),
                                                new SqlParameter("@CertiZipName", CertiZipName),
                                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                            });
                            sqlcommand.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.writeLogToDB("", "CliInfo/insertData", ex.ToString());
                return false;
            }
        }
        public class CliInfoDetail
        {
            public string Cli_ID { get; set; }
            public string Cli_ChiNAME_Last { get; set; }
            public string Cli_ChiNAME_First { get; set; }
            public string Cli_EngNAME_Last { get; set; }
            public string Cli_EngNAME_First { get; set; }
            public string Cli_Gender { get; set; }
            public string Cli_ID_Num { get; set; }
            public string Cli_PassPort { get; set; }
            public string Cli_BirthDay { get; set; }
            public string Cli_Census_Country { get; set; }
            public string Cli_Phone_site { get; set; }
            public string Cli_Phone { get; set; }
            public string Cli_Email { get; set; }
            public string Cli_Live_Address { get; set; }
            public string Cli_Eng_Address { get; set; }
            public string Cli_PostalCode { get; set; }
            public string LoginACCOUNT { get; set; }
        }
        public class CliInfoTempDetail
        {
            public string TempID { get; set; }
            public string Cli_ChiNAME_Last { get; set; }
            public string Cli_ChiNAME_First { get; set; }
            public string Cli_EngNAME_Last { get; set; }
            public string Cli_EngNAME_First { get; set; }
            public string Cli_Gender { get; set; }
            public string Cli_ID_Num { get; set; }
            public string Cli_PassPort { get; set; }
            public string Cli_BirthDay { get; set; }
            public string Cli_Census_Country { get; set; }
            public string Cli_Phone_site { get; set; }
            public string Cli_Phone { get; set; }
            public string Cli_Email { get; set; }
            public string Cli_Live_Address { get; set; }
            public string Cli_Eng_Address { get; set; }
            public string Cli_PostalCode { get; set; }        
            public string Type { get; set; }
            public string FileName1 { get; set; }
            public string FileName2 { get; set; }
            public string FileName3 { get; set; }
            public string FileName4 { get; set; }
            public string FileName5 { get; set; }
            public string FileName6 { get; set; }
            public string FileName7 { get; set; }
            public string FileName8 { get; set; }
            public string FileName9 { get; set; }
            public string LoginACCOUNT { get; set; }
        }
        public class MyDateSorter : IComparer
        {
            #region IComparer Members
            public int Compare(object x, object y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                FileInfo xInfo = (FileInfo)x;
                FileInfo yInfo = (FileInfo)y;


                //依名稱排序
                return xInfo.FullName.CompareTo(yInfo.FullName);//遞增
                                                                //return yInfo.FullName.CompareTo(xInfo.FullName);//遞減

                //依修改日期排序
                //return xInfo.LastWriteTime.CompareTo(yInfo.LastWriteTime);//遞增
                //return yInfo.LastWriteTime.CompareTo(xInfo.LastWriteTime);//遞減
            }
            #endregion
        }
    }
}