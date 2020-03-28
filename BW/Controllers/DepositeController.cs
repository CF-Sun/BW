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
    public class DepositeController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetCliNewDeposite()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Deposit_ID,A.Cli_ID,A.Deposit_Amount, A.Status,A.CREATE_DATE,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE, DepositListFileName, Isfile,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                                (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName 
                                                from DepositList A
                                                join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                join ConInfoDetail C on A.Con_ID=C.Con_ID
                                                where A.Status=0 and DATEDIFF(day,A.CREATE_DATE,'"+ convertTime.UStoTW(DateTime.Now).ToString("yyyy/MM/dd hh:mm:ss") + @"')<=31
                                                order by A.CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliDepositeStatus(string CliID)
        {
            DataTable dt = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select Cli_ID, SUM(Deposit_Amount)as Deposit,C.CODE_DESC as Deposit_Type, SUM(ISNULL(Withdrawal_Amount,0)) as Withdrawal,
                                        SUM(Deposit_Amount)-SUM(ISNULL(Withdrawal_Amount,0)) as NetDeposit
                                        from DepositList A 
                                        left join
                                        (select Deposit_ID, SUM(Withdrawal_Amount)as Withdrawal_Amount from WithdrawalList
		                                        where Cli_ID=@CliID and Status !=0 and ExpectDate<=@ExpectDate
		                                        group by Deposit_ID) B on A.Deposit_ID=B.Deposit_ID
                                        left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) C on A.Deposit_Type=C.CODE_NO
                                        where A.Cli_ID=@CliID and A.Status=2
                                        group by Cli_ID,CODE_DESC ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID.Trim()),
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetCliDepositeDetail(string CliID, int DeporWith)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = "";
                string Deposite = @" select Deposit_ID as ID, N'入金'as Kind, Deposit_Amount as Amount, CL1.CODE_DESC as Type, convert(varchar, CREATE_DATE, 111) as CREATE_DATE , DepositListFileName as FileName, Isfile,
                                                convert(varchar, Arrival_DATE, 111) as Arrival_DATE,N'入金單' as WithdrawalFrom, CL2.CODE_DESC as Status, DL.Status as StatusNo
                                                from DepositList DL
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Type')CL1 on DL.Deposit_Type=CL1.CODE_NO
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Status')CL2 on DL.Status=CL2.CODE_NO
                                                where DL.Cli_ID=@CliID ";
                string Withdraw= @" select Withdrawal_ID as ID, N'出金'as Kind, Withdrawal_Amount as Amount, CL1.CODE_DESC as Type, convert(varchar, WL.CREATE_DATE, 111) as CREATE_DATE , WithdrawalListFileName as FileName, WL.Isfile,
                                                convert(varchar, WL.Arrival_DATE, 111) as Arrival_DATE, WL.Deposit_ID as WithdrawalFrom, CL2.CODE_DESC as Status, WL.Status as StatusNo
                                                from WithdrawalList WL
                                                left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Type')CL1 on DL.Deposit_Type=CL1.CODE_NO
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Withdrawal_Status')CL2 on WL.Status=CL2.CODE_NO
                                                where WL.Cli_ID=@CliID ";
                switch (DeporWith)
                {
                    case 0:
                        sqlcommandstring = Deposite + " union all " + Withdraw;
                        break;
                    case 1:
                        sqlcommandstring = Withdraw;
                        break;
                    case 2:
                        sqlcommandstring = Deposite;
                        break;
                }

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@CliID", CliID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDepoWithStatus(string Kind)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = "";
                if (Kind == "入金")
                {
                    sqlcommandstring = @"  select CODE_NO, CODE_DESC  
                                            from CodeList where CODE_TYPE='Deposit_Status'";
                }else if (Kind == "出金")
                {
                    sqlcommandstring = @"select CODE_NO, CODE_DESC  
                                            from CodeList where CODE_TYPE='Withdrawal_Status'";
                }

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetDepositeDataTotal(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select sum(A.Deposit_Amount) Deposit_Amount ,CL.CODE_DESC, A.Deposit_Type
                                                from DepositList A
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on A.Deposit_Type=CL.CODE_NO
												where A.Status=2 ";
                if (startDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE >= '" + startDate + "' ";
                }
                if (endDate != "")
                {
                    sqlcommandstring += "and A.Arrival_DATE <= '" + endDate + "' ";
                }
                sqlcommandstring += " group by CL.CODE_DESC, A.Deposit_Type order by A.Deposit_Type asc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetDepositeData(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Deposit_ID,A.Cli_ID, A.Con_ID, A.Deposit_Amount, A.Status,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE, CL.CODE_DESC, A.Deposit_Type,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName,
                                                (C.Con_ChiNAME_Last+C.Con_ChiNAME_First)as ConName 
                                                from DepositList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join ConInfoDetail C on A.Con_ID=C.Con_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on A.Deposit_Type=CL.CODE_NO
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
        public ActionResult GetDepositeDataByConId(string ConID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Deposit_ID,A.Cli_ID, A.Deposit_Amount, CL2.CODE_DESC as Status,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE,
                                                convert(varchar, A.CREATE_DATE, 111) as CREATE_DATE, CL.CODE_DESC as Deposit_Type,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName
                                                from DepositList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on A.Deposit_Type=CL.CODE_NO
												left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Status' and CODE_Status=1) CL2 on A.Status=CL2.CODE_NO
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
        public ActionResult GetDepositeDataByCliId(string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Deposit_ID,A.Cli_ID, A.Deposit_Amount, CL2.CODE_DESC as Status,
                                                convert(varchar, A.Arrival_DATE, 111) as Arrival_DATE,
                                                convert(varchar, A.CREATE_DATE, 111) as CREATE_DATE, CL.CODE_DESC as Deposit_Type,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName
                                                from DepositList A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Type' and CODE_Status=1) CL on A.Deposit_Type=CL.CODE_NO
												left join (select CODE_NO, CODE_DESC from CodeList 
			                                        where CODE_TYPE='Deposit_Status' and CODE_Status=1) CL2 on A.Status=CL2.CODE_NO
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
        [HttpGet]
        public ActionResult GetDepositeListByConId(string ConID, string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Deposit_ID
                                                from DepositList
												where Con_ID=@ConID and Cli_ID=@CliID and Status=2
												order by CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID.Trim()),
                    new SqlParameter("@CliID", CliID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetDepositeListByCliId(string CliID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Deposit_ID
                                                from DepositList
												where Cli_ID=@CliID and Status=2
												order by CREATE_DATE desc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@CliID", CliID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetIndividualDataTotal(string Con_NO, string Con_Name, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"    select sum(Deposit_Amount) Deposit_Amount,sum(Withdrawal_Amount) Withdrawal_Amount, sum(Amount) Amount, CODE_DESC, Deposit_Type
                                                from
                                                    (select ISNULL(Deposit_Amount,0) as Deposit_Amount,
                                                    ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount,
                                                    CL.CODE_DESC, A.Deposit_Type
                                                    from
                                                    (select Cli_ID, Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
                                                    from DepositList DL
                                                    left join ConInfoDetail CD on DL.Con_ID=CD.Con_ID
                                                    where Status=2 ";
                //where Status =2 and DL.Con_ID like '%" + Con_NO.Trim() + "%' and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First like N'%" + Con_Name.Trim() + @"%' ";
                if (Con_NO.Trim() != "")
                    sqlcommandstring += " and DL.Con_ID = '" + Con_NO.Trim() + "' ";
                if (Con_Name.Trim() != "")
                    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by Cli_ID,Deposit_Type) A
                                                full outer join
                                                (select WL.Cli_ID,DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
                                                from WithdrawalList WL
                                                left join ConInfoDetail CD on WL.Con_ID=CD.Con_ID
												left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                where WL.Status !=0 and ExpectDate<=@ExpectDate ";
                //where Status=2 and WL.Con_ID like '%" + Con_NO.Trim() + "%' and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First like N'%" + Con_Name.Trim() + @"%'";
                if (Con_NO.Trim() != "")
                    sqlcommandstring += " and WL.Con_ID = '" + Con_NO.Trim() + "' ";
                if (Con_Name.Trim() != "")
                    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by WL.Cli_ID,DL.Deposit_Type) B on A.Cli_ID=B.Cli_ID and A.Deposit_Type=B.Deposit_Type
										left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
                                        ) main group by CODE_DESC, Deposit_Type order by Deposit_Type";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConIndividualPerform(string Con_NO, string Con_Name, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select ISNULL(A.Cli_ID,B.Cli_ID) as Cli_ID, ISNULL(Deposit_Amount,0) as Deposit_Amount,
                                                ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount,
                                                (Cli.Cli_ChiNAME_Last+Cli.Cli_ChiNAME_First)as ChiName, CL.CODE_DESC, A.Deposit_Type
                                                from
                                                (select Cli_ID, Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
                                                from DepositList DL
                                                left join ConInfoDetail CD on DL.Con_ID=CD.Con_ID
                                                where Status=2 ";
                //where Status =2 and DL.Con_ID like '%" + Con_NO.Trim() + "%' and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First like N'%" + Con_Name.Trim() + @"%' ";
                if (Con_NO.Trim() != "")
                    sqlcommandstring += " and DL.Con_ID = '" + Con_NO.Trim() + "' ";
                if (Con_Name.Trim() != "")
                    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by Cli_ID,Deposit_Type) A
                                                full outer join
                                                (select WL.Cli_ID,DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
                                                from WithdrawalList WL
                                                left join ConInfoDetail CD on WL.Con_ID=CD.Con_ID
												left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                where WL.Status !=0 and ExpectDate<=@ExpectDate ";
                //where Status=2 and WL.Con_ID like '%" + Con_NO.Trim() + "%' and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First like N'%" + Con_Name.Trim() + @"%'";
                if (Con_NO.Trim() != "")
                    sqlcommandstring += " and WL.Con_ID = '" + Con_NO.Trim() + "' ";
                if (Con_Name.Trim() != "")
                    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by WL.Cli_ID,DL.Deposit_Type) B on A.Cli_ID=B.Cli_ID and A.Deposit_Type=B.Deposit_Type
                                        left join CliInfoDetail Cli on ISNULL(A.Cli_ID,B.Cli_ID)=CLi.Cli_ID
										left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
                                        order by Cli_ID asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetOrganDataTotal(string Con_NO, string Con_Name, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtResult = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取得所有顧問業績
                string sqlcommandstring = @"  select ISNULL(A.Con_ID,B.Con_ID) as Con_ID, ISNULL(Deposit_Amount,0) as Deposit_Amount,
                                                ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount,
                                                CL.CODE_DESC, A.Deposit_Type
                                                from
                                                (select DL.Con_ID,Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
                                                from DepositList DL
                                                left join ConInfoDetail CD on DL.Con_ID=CD.Con_ID
                                                where Status=2  ";
                //if (Con_NO.Trim() != "")
                //    sqlcommandstring += " and DL.Con_ID = '" + Con_NO.Trim() + "' ";
                //if (Con_Name.Trim() != "")
                //    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by DL.Con_ID,Deposit_Type) A
                                                full outer join
                                                (select WL.Con_ID,DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
                                                from WithdrawalList WL
                                                left join ConInfoDetail CD on WL.Con_ID=CD.Con_ID
												left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                where WL.Status !=0 and ExpectDate<=@ExpectDate  ";
                //if (Con_NO.Trim() != "")
                //    sqlcommandstring += " and WL.Con_ID = '" + Con_NO.Trim() + "' ";
                //if (Con_Name.Trim() != "")
                //    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by WL.Con_ID,DL.Deposit_Type) B on A.Con_ID=B.Con_ID
										left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
                                        order by Con_ID asc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                //取得該顧問下所有顧問
                getChildMemInfo(Con_NO, Con_Name);

                if (dtConInfoById.Rows.Count == 0)
                    return Json(dtConInfoById.ToJson(), JsonRequestBehavior.AllowGet);

                dtResult = dt.Clone();
                string condition = "";

                for (int i = 0; i < dtConInfoById.Rows.Count; i++)
                {
                    condition = "Con_ID='" + dtConInfoById.Rows[i]["Con_ID"] + "'";

                    foreach (DataRow dr in dt.Select(condition))
                    {
                        dtResult.ImportRow(dr);
                    }
                }
                //var grouped = from row in dtResult.AsEnumerable()
                //              group row by new { CODE = row.Field<string>("CODE_DESC"), TYPE = row.Field<string>("Deposit_Type") } into g
                //              select new
                //              {
                //                  CODE_DESC = g.Key.CODE,
                //                  Deposit_Type = g.Key.TYPE,
                //                  Deposit_Amount = g.Sum(p => (decimal)p["Deposit_Amount"]),
                //                  Withdrawal_Amount = g.Sum(p => (decimal)p["Withdrawal_Amount"]),
                //                  Amount = g.Sum(p => (decimal)p["Amount"])
                //              };
                dtResult = dtResult.AsEnumerable()
                    .GroupBy(r => new { Col1 = r["CODE_DESC"], Col2 = r["Deposit_Type"] })
                    .Select(g =>
                    {
                        var row = dtResult.NewRow();

                        row["Deposit_Amount"] = g.Sum(r => r.Field<decimal>("Deposit_Amount"));
                        row["Withdrawal_Amount"] = g.Sum(r => r.Field<decimal>("Withdrawal_Amount"));
                        row["Amount"] = g.Sum(r => r.Field<decimal>("Amount"));
                        row["CODE_DESC"] = g.Key.Col1;
                        row["Deposit_Type"] = g.Key.Col2;

                        return row;

                    })
                    .CopyToDataTable();
                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConOrganPerform(string Con_NO, string Con_Name, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtResult = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取得所有顧問業績
                string sqlcommandstring = @"  select ISNULL(A.Con_ID,B.Con_ID) as Con_ID, ISNULL(Deposit_Amount,0) as Deposit_Amount,
                                                ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount,
                                                (Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First)as ChiName, CL.CODE_DESC, A.Deposit_Type
                                                from
                                                (select DL.Con_ID,Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
                                                from DepositList DL
                                                left join ConInfoDetail CD on DL.Con_ID=CD.Con_ID
                                                where Status=2  ";
                //if (Con_NO.Trim() != "")
                //    sqlcommandstring += " and DL.Con_ID = '" + Con_NO.Trim() + "' ";
                //if (Con_Name.Trim() != "")
                //    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by DL.Con_ID,Deposit_Type) A
                                                full outer join
                                                (select WL.Con_ID,DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
                                                from WithdrawalList WL
                                                left join ConInfoDetail CD on WL.Con_ID=CD.Con_ID
												left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                where WL.Status !=0 and ExpectDate<=@ExpectDate  ";
                //if (Con_NO.Trim() != "")
                //    sqlcommandstring += " and WL.Con_ID = '" + Con_NO.Trim() + "' ";
                //if (Con_Name.Trim() != "")
                //    sqlcommandstring += " and CD.Con_ChiNAME_Last+CD.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                if (startDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by WL.Con_ID,DL.Deposit_Type) B on A.Con_ID=B.Con_ID
                                        left join ConInfoDetail Con on ISNULL(A.Con_ID,B.Con_ID)=Con.Con_ID
										left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
                                        order by Con_ID asc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                //取得該顧問下所有顧問
                getChildMemInfo(Con_NO, Con_Name);

                if(dtConInfoById.Rows.Count==0)
                    return Json(dtConInfoById.ToJson(), JsonRequestBehavior.AllowGet);

                dtResult=dt.Clone();
                string condition = "";

                for(int i=0; i< dtConInfoById.Rows.Count; i++)
                {
                    condition = "Con_ID='" + dtConInfoById.Rows[i]["Con_ID"] + "'";

                    foreach (DataRow dr in dt.Select(condition))
                    {
                        dtResult.ImportRow(dr);
                    }
                }
                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConTotalData(string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DateTime dtime = convertTime.UStoTW(DateTime.Now);

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取得所有業績
                string sqlcommandstring = @"  select ISNULL(Deposit_Amount,0) as Deposit_Amount,ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, 
                                                ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount, CL.CODE_DESC
                                                from
                                                (select Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
                                                from DepositList DL
                                                where Status=2  ";
                if (startDate != "")
                    sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by Deposit_Type) A
                                                full outer join
                                                (select DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
                                                from WithdrawalList WL
												left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                where WL.Status !=0 and ExpectDate<=@ExpectDate   ";
                if (startDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
                if (endDate != "")
                    sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

                sqlcommandstring += @" group by DL.Deposit_Type) B on A.Deposit_Type=B.Deposit_Type
										left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
                                        order by A.Deposit_Type asc ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@ExpectDate", dtime.AddMonths(-1).AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
             
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpGet]
        //public ActionResult GetConTotalDataTotal(string startDate, string endDate)
        //{
        //    DataTable dt = new DataTable();
        //    string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
        //    using (SqlConnection sqlconnection = new SqlConnection(conn))
        //    {
        //        sqlconnection.Open();

        //        //取得所有業績
        //        string sqlcommandstring = @"  select ISNULL(Deposit_Amount,0) as Deposit_Amount,ISNULL(Withdrawal_Amount,0) as Withdrawal_Amount, 
        //                                        ISNULL(Deposit_Amount,0)-ISNULL(Withdrawal_Amount,0) as Amount, CL.CODE_DESC
        //                                        from
        //                                        (select Deposit_Type, sum(Deposit_Amount) as Deposit_Amount
        //                                        from DepositList DL
        //                                        where Status=2  ";
        //        if (startDate != "")
        //            sqlcommandstring += " and Arrival_DATE >= '" + startDate + "' ";
        //        if (endDate != "")
        //            sqlcommandstring += " and Arrival_DATE <= '" + endDate + "' ";

        //        sqlcommandstring += @" group by Deposit_Type) A
        //                                        full outer join
        //                                        (select DL.Deposit_Type, sum(Withdrawal_Amount) as Withdrawal_Amount
        //                                        from WithdrawalList WL
								//				left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
        //                                        where WL.Status=2   ";
        //        if (startDate != "")
        //            sqlcommandstring += " and WL.Arrival_DATE >= '" + startDate + "' ";
        //        if (endDate != "")
        //            sqlcommandstring += " and WL.Arrival_DATE <= '" + endDate + "' ";

        //        sqlcommandstring += @" group by DL.Deposit_Type) B on A.Deposit_Type=B.Deposit_Type
								//		left join CodeList CL on CL.CODE_NO=A.Deposit_Type and CL.CODE_TYPE='Deposit_Type' and CL.CODE_Status=1
        //                                order by A.Deposit_Type asc ";
        //        SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
        //        SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
        //        da.Fill(dt);

        //        return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
        //    }
        //}
        [HttpGet]
        public ActionResult GetCliDepositeByID(string ConID, int DeporWith, string StartDate, string EndDate)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = "";
                string Deposite = @" select Deposit_ID as ID, N'入金'as Kind, Deposit_Amount as Amount, CL1.CODE_DESC as Type, convert(varchar, DL.CREATE_DATE, 111) as CREATE_DATE , DepositListFileName as FileName, Isfile,
                                                convert(varchar, Arrival_DATE, 111) as Arrival_DATE,N'入金單' as WithdrawalFrom, CL2.CODE_DESC as Status, DL.Status as StatusNo,
                                                DL.Cli_ID, (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)as CliName
                                                from DepositList DL
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Type')CL1 on DL.Deposit_Type=CL1.CODE_NO
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Status')CL2 on DL.Status=CL2.CODE_NO
                                                left join CliInfoDetail B on DL.Cli_ID=B.Cli_ID
                                                where DL.Con_ID=@ConID ";
                if (StartDate != "")
                    Deposite += " and Arrival_DATE >= '" + StartDate + "' ";
                if (EndDate != "")
                    Deposite += " and Arrival_DATE <= '" + EndDate + "' ";


                string Withdraw = @" select Withdrawal_ID as ID, N'出金'as Kind, Withdrawal_Amount as Amount, CL1.CODE_DESC as Type, convert(varchar, WL.CREATE_DATE, 111) as CREATE_DATE , WithdrawalListFileName as FileName, WL.Isfile,
                                                convert(varchar, WL.Arrival_DATE, 111) as Arrival_DATE, WL.Deposit_ID as WithdrawalFrom, CL2.CODE_DESC as Status, WL.Status as StatusNo,
                                                DL.Cli_ID, (C.Cli_ChiNAME_Last+C.Cli_ChiNAME_First)as CliName
                                                from WithdrawalList WL
                                                left join DepositList DL on WL.Deposit_ID=DL.Deposit_ID
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Deposit_Type')CL1 on DL.Deposit_Type=CL1.CODE_NO
                                                left join (select CODE_NO, CODE_DESC from CodeList 
			                                                where CODE_TYPE='Withdrawal_Status')CL2 on WL.Status=CL2.CODE_NO
                                                left join CliInfoDetail C on WL.Cli_ID=C.Cli_ID
                                                where WL.Con_ID=@ConID ";
                if (StartDate != "")
                    Withdraw += " and Arrival_DATE >= '" + StartDate + "' ";
                if (EndDate != "")
                    Withdraw += " and Arrival_DATE <= '" + EndDate + "' ";

                switch (DeporWith)
                {
                    case 0:
                        sqlcommandstring = Deposite + " union all " + Withdraw;
                        break;
                    case 1:
                        sqlcommandstring = Withdraw;
                        break;
                    case 2:
                        sqlcommandstring = Deposite;
                        break;
                }


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {

                    new SqlParameter("@ConID", ConID.Trim())
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int UpdateStatus(string Deposit_ID, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update DepositList set Status=2,UPDATE_DATE=@date  where Deposit_ID=@Deposit_ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Deposit_ID", Deposit_ID==null?"": Deposit_ID),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateStatus", "更新入金狀態為已入金,編號為" + Deposit_ID);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateStatus", e.ToString());
                return 0;
            }
        }

        [HttpPost]
        public int UpdateArrivalDate(string ID, string Kind, string Date, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    if (Kind == "出金")
                    {
                        sqlcommandstring = @"update WithdrawalList set Arrival_DATE=@Arrival_DATE, UPDATE_DATE=@date
                                                where Withdrawal_ID=@ID ";
                    }
                    else if (Kind == "入金")
                    {
                        sqlcommandstring = @"update DepositList set Arrival_DATE=@Arrival_DATE, UPDATE_DATE=@date
                                                where Deposit_ID=@ID ";
                    }

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ID", ID),
                        new SqlParameter("@Arrival_DATE", Date==""?System.Data.SqlTypes.SqlDateTime.Null:(object)Date),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateArrivalDate", "更新"+ Kind +"單號:"+ ID+","+ Kind +"日為"+ Date);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateArrivalDate", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateWithdrawalFrom(string ID, string From, string CliID, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    //查詢該客戶編號下有無該入金編號
                    string sqlcommandstring = @"select count(*) from DepositList where Deposit_ID=@Deposit_ID and Cli_ID=@Cli_ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Deposit_ID", From),
                        new SqlParameter("@Cli_ID", CliID)
                    });
                    int count = Convert.ToInt32(sqlcommand.ExecuteScalar());

                    if (count == 0)
                        return 2; //表示無此編號

                    sqlcommandstring = @"update WithdrawalList set Deposit_ID=@Deposit_ID, UPDATE_DATE=@date  where Withdrawal_ID=@Withdrawal_ID";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Deposit_ID", From==null?"": From),
                        new SqlParameter("@Withdrawal_ID", ID==null?"": ID),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateWithdrawalFrom", "設定出金單編號:" + ID + "的入金來源為" + From);
                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateWithdrawalFrom", e.ToString());
                return 0;
            }
        }

        [HttpPost]
        public int UpdateDepoWithStatus(string ID, string Kind, string Status, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    if (Kind == "出金")
                    {
                        sqlcommandstring = @"update WithdrawalList set Status=@Status, UPDATE_DATE=@date
                                                where Withdrawal_ID=@ID ";
                    }
                    else if (Kind == "入金")
                    {
                        sqlcommandstring = @"update DepositList set Status=@Status, UPDATE_DATE=@date
                                                where Deposit_ID=@ID ";
                    }

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ID", ID),
                        new SqlParameter("@Status", Status),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateDepoWithStatus", "更新" + Kind + "單號為" + ID + "狀態為" + Status);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateDepoWithStatus", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public int UpdateDepoWithAmount(string ID, string Kind, string newAmount, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";

                    newAmount = newAmount.Replace(",", "");

                    if (Kind == "出金")
                    {
                        sqlcommandstring = @"update WithdrawalList set Withdrawal_Amount=@newAmount, UPDATE_DATE=@date 
                                                where Withdrawal_ID=@ID ";
                    }
                    else if (Kind == "入金")
                    {
                        sqlcommandstring = @"update DepositList set Deposit_Amount=@newAmount, UPDATE_DATE=@date 
                                                where Deposit_ID=@ID ";
                    }

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ID", ID),
                        new SqlParameter("@newAmount", newAmount),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateDepoWithAmount", "更新" + Kind + "單號為" + ID + ",金額為" + newAmount);

                }
                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/UpdateDepoWithStatus", e.ToString());
                return 0;
            }
        }
        [HttpPost]
        public string DepositeRegi(string CliNo, string ConNo, string FileName, string DepositeDay, string DepositeAmount, string DepositeType, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string Deposit_ID = "";

                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = @" select ISNULL(max(Deposit_ID),0) from DepositList";
                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    Deposit_ID = Convert.ToString(Convert.ToInt32(Convert.ToString(sqlcommand.ExecuteScalar()).Substring(1)) + 1);


                    while (Deposit_ID.Length < 9)  //ID長度為9 不足長度補0
                    {
                        Deposit_ID = "0" + Deposit_ID;
                    }
                    Deposit_ID = "D" + Deposit_ID;

                    if (String.IsNullOrEmpty(ConNo))
                    {
                        sqlcommandstring = @" select Con_ID from CliInfo where Cli_ID=@Cli_ID ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Cli_ID", CliNo)
                        });
                        ConNo = Convert.ToString(sqlcommand.ExecuteScalar());
                    }
                    DepositeAmount = DepositeAmount.Replace(",", "");
                    sqlcommandstring = @" Insert DepositList values(@Deposit_ID,@Cli_ID,@ConNo,@DepositAmount,@DepositType,@DepositDATE,@ArrivalDATE,'0',@date,@date,@FileName,1) ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Deposit_ID", Deposit_ID),
                        new SqlParameter("@Cli_ID", CliNo),
                        new SqlParameter("@ConNo", ConNo),
                        new SqlParameter("@DepositAmount", DepositeAmount),
                        new SqlParameter("@DepositType", DepositeType),
                        new SqlParameter("@DepositDATE", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@ArrivalDATE", DepositeDay),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now)),
                        new SqlParameter("@FileName", FileName)
                    });
                    sqlcommand.ExecuteNonQuery();

                    log.writeLogToDB(LoginACCOUNT, "Deposite/DepositeRegi", "新增入金登記編號為" + Deposit_ID);
                }
                return Deposit_ID;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/DepositeRegi", e.ToString());
                return "0";
            }
        }
        public DataTable dtAllMemInfo = new DataTable();
        public DataTable dtConInfoById = new DataTable();
        DataRow row;
        public void getChildMemInfo(string Con_NO, string Con_Name)
        {
            
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                //取得所有顧問名單
                string sqlcommandstring = @" select Con_ID,Parent_Con_ID from ConInfo where Con_ID!='000' ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dtAllMemInfo);

                //取得欲查詢的顧問ID
                sqlcommandstring = @" select CI.Con_ID 
                                        from ConInfo CI 
                                        left join ConInfoDetail Con on CI.Con_ID=Con.Con_ID
                                        where 1=1  ";
                if (Con_NO.Trim() != "")
                    sqlcommandstring += " and CI.Con_ID = '" + Con_NO.Trim() + "' ";
                if (Con_Name.Trim() != "")
                    sqlcommandstring += " and Con.Con_ChiNAME_Last+Con.Con_ChiNAME_First= N'" + Con_Name.Trim() + "' ";
                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);

                Con_NO = Convert.ToString(sqlcommand.ExecuteScalar());

                if(Con_NO.Trim() != "")
                {
                    dtConInfoById.Columns.Add("Con_ID", typeof(string));
                    row = dtConInfoById.NewRow();
                    row["Con_ID"] = Con_NO.Trim();
                    dtConInfoById.Rows.Add(row);
                    getParentMemInfo(Con_NO);
                }
            }
        }
        public void getParentMemInfo(string Parent_ID)
        {

            string condition = "Parent_Con_ID='" + Parent_ID + "'";

            foreach (DataRow dr in dtAllMemInfo.Select(condition))
            {
                string Con_ID = dr["Con_ID"].ToString();

                row = dtConInfoById.NewRow();
                row["Con_ID"] = Con_ID;
                dtConInfoById.Rows.Add(row);
                getParentMemInfo(Con_ID);
            }
        }

        public virtual ActionResult UploadFile(string Deposit_ID, string OriFileName, string LoginACCOUNT)
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
                    string filePath = Server.MapPath("~/Content/files/DepositList/" + Deposit_ID.Trim() + "/");
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

                        if (!updateDepositList(Deposit_ID, fileName, LoginACCOUNT))//更新資料庫
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
                log.writeLogToDB("", "Deposite/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public bool updateDepositList(string Deposit_ID, string fileName, string LoginACCOUNT)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();

                    string sqlcommandstring = @"update DepositList set DepositListFileName=@fileName, Isfile=1, UPDATE_DATE=@date  where  Deposit_ID=@Deposit_ID";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Deposit_ID", Deposit_ID.Trim()),
                        new SqlParameter("@fileName", fileName),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                    log.writeLogToDB(LoginACCOUNT, "Deposite/updateDepositList", "更新入金單號為" + Deposit_ID.Trim() + ",檔案名稱為" + fileName);

                }
                return true;
            }
            catch (Exception ex)
            {
                log.writeLogToDB(LoginACCOUNT, "Deposite/updateDepositList", ex.ToString());
                return false;
            }
        }

        //開啟檔案
        public ActionResult Open(string Deposit_ID, string FileName)
        {
            var path = Server.MapPath("/Content/files/DepositList/" + Deposit_ID.Trim() + "/" + FileName);
            var mime = MimeMapping.GetMimeMapping(FileName);
            return File(path, mime, FileName);
        }
    }
}