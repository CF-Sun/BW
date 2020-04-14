using BW.Helpers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class PayedRecordController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public ActionResult GetCliPayedRecord(string CliID, string CliName)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select CI.Cli_ID, (CID.Cli_ChiNAME_Last+CID.Cli_ChiNAME_First)as ChiName, 
                                                (CID.Cli_EngNAME_Last+CID.Cli_EngNAME_First)as EngNAME, Interest_Amount,
                                                convert(varchar, InterestInterval_Start, 111) as InterestInterval_Start,
                                                convert(varchar, InterestInterval_End, 111) as InterestInterval_End,
                                                convert(varchar, InterestPayedDate, 111) as InterestPayedDate
                                                from CliInterest CI
                                                left join CliInfoDetail CID on CI.Cli_ID=CID.Cli_ID
                                                where CI.Cli_ID like '%" + CliID.Trim() + "%' and CID.Cli_ChiNAME_Last+CID.Cli_ChiNAME_First like N'%" + CliName.Trim() + @"%'
                                                order by InterestPayedDate desc, CI.Cli_ID asc  ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetConReportData(string startDate, string endDate)
        {
            DataTable dtTop5 = new DataTable();
            DataTable dtReportVer = new DataTable();
            DataTable dtBonusSum = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取出最近的前五次季報表
                string sqlcommandstring = @"  select distinct top 5  Report_ID, Calculate_Year, Calculate_Quarterly, 0 as Bonus
                                                from BonusReportMaster order by Report_ID desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@startDate", startDate),
                        new SqlParameter("@endDate", endDate)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dtTop5);

                if (dtTop5.Rows.Count > 0)
                {
                    //季報表所有資料
                    sqlcommandstring = @"  select * from BonusReportMaster ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtReportVer);

                    //計算每個月報表總和
                    sqlcommandstring = @"  select MonthlyReport_ID, sum(Bonus)as Bonus 
                                        from BonusReportDetail group by MonthlyReport_ID ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtBonusSum);

                    for(int i=0; i < dtTop5.Rows.Count; i++)
                    {
                        foreach (DataRow dr in dtReportVer.Select("Report_ID='"+ dtTop5.Rows[i]["Report_ID"].ToString() + "'", "Report_Ver DESC"))
                        {
                            decimal BonusCount = 0;
                            foreach (DataRow dr2 in dtBonusSum.Select("MonthlyReport_ID='"+dr["MonthlyReport1_ID"] +"'"))
                            {
                                BonusCount += Convert.ToDecimal(dr2["Bonus"]);
                            }
                            foreach (DataRow dr2 in dtBonusSum.Select("MonthlyReport_ID='" + dr["MonthlyReport2_ID"] + "'"))
                            {
                                BonusCount += Convert.ToDecimal(dr2["Bonus"]);
                            }
                            foreach (DataRow dr2 in dtBonusSum.Select("MonthlyReport_ID='" + dr["MonthlyReport3_ID"] + "'"))
                            {
                                BonusCount += Convert.ToDecimal(dr2["Bonus"]);
                            }
                            dtTop5.Rows[i]["Bonus"] = BonusCount;
                            break;
                        }
                    }

                }

                return Json(dtTop5.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetReportMonth()
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select distinct (Calculate_Year+'/'+Calculate_Month) as ReportMonth
                                                from BonusReportDetail order by ReportMonth desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetReportDataById(string ConID, string ConName)
        {
            DataTable dtResult = new DataTable();
            DataTable dtBonusSum = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //取出ConID
                string sqlcommandstring = @"  select Con_ID from ConInfoDetail
                                                where 1=1 ";

                if (ConID.Trim() != "")
                    sqlcommandstring += @" and Con_ID=@ConID";
                if(ConName.Trim()!="")
                    sqlcommandstring += @" and Con_ChiNAME_Last+Con_ChiNAME_First=N'"+ConName+"'";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", ConID),
                        new SqlParameter("@ConName", ConName)
                    });
                string Con_ID = Convert.ToString(sqlcommand.ExecuteScalar());

                if (Con_ID.Trim() != "" && Con_ID.Trim() != null)
                {
                    //取得該顧問存在的季報表
                    sqlcommandstring = @"  select B.* from
                                            (select Max(Report_Ver) as Report_Ver, Report_ID, Calculate_Year,Calculate_Quarterly
                                            from BonusReportMaster
                                            where MonthlyReport1_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            or MonthlyReport2_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            or MonthlyReport3_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            group by Report_ID,Calculate_Year,Calculate_Quarterly) A
                                            left join BonusReportMaster B on A.Report_ID=B.Report_ID and A.Report_Ver=B.Report_Ver 
                                            order by Report_ID desc";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtResult);
                    dtResult.Columns.Add("Bonus", typeof(decimal));
                    dtResult.Columns.Add("Report_Con_ID", typeof(string));
                    //取得每月報表總和
                    sqlcommandstring = @"  select MonthlyReport_ID,Report_Con_ID, SUM(Bonus) Bonus 
                                            from BonusReportDetail where Report_Con_ID=@Con_ID
                                            group by MonthlyReport_ID,Report_Con_ID ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID.Trim())
                    });
                    da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtBonusSum);

                    //取出每筆季報表,根據月報表ID抓出獎金計算總合並寫回dtResult
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        decimal totalBonus = 0;
                        string condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport1_ID"] + "'";
                        DataRow[] drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport2_ID"] + "'";
                        drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport3_ID"] + "'";
                        drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        dtResult.Rows[i]["Bonus"] = totalBonus;
                        dtResult.Rows[i]["Report_Con_ID"] = Con_ID.Trim();
                    }
                }

                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetReportDataById_Con(string ConID)
        {
            DataTable dtResult = new DataTable();
            DataTable dtBonusSum = new DataTable();
            DataTable newResult = new DataTable();
            DateTime dt = convertTime.UStoTW(DateTime.Now);
            string year = dt.ToString("yyyy");
            string month = dt.ToString("MM");
            string day = dt.ToString("dd");
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                if (ConID.Trim() != "" && ConID.Trim() != null)
                {
                    //取得該顧問存在的季報表
                    string sqlcommandstring = @"  select B.* from
                                            (select Max(Report_Ver) as Report_Ver, Report_ID, Calculate_Year,Calculate_Quarterly
                                            from BonusReportMaster
                                            where MonthlyReport1_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            or MonthlyReport2_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            or MonthlyReport3_ID in (select distinct MonthlyReport_ID from BonusReportDetail where Report_Con_ID=@Con_ID)
                                            group by Report_ID,Calculate_Year,Calculate_Quarterly) A
                                            left join BonusReportMaster B on A.Report_ID=B.Report_ID and A.Report_Ver=B.Report_Ver 
                                            order by Report_ID desc";

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", ConID.Trim())
                    });
                    SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtResult);
                    dtResult.Columns.Add("Bonus", typeof(decimal));
                    dtResult.Columns.Add("Report_Con_ID", typeof(string));
                    dtResult.Columns.Add("IsVisible", typeof(int));
                    //取得每月報表總和
                    sqlcommandstring = @"  select MonthlyReport_ID,Report_Con_ID, SUM(Bonus) Bonus 
                                            from BonusReportDetail where Report_Con_ID=@Con_ID
                                            group by MonthlyReport_ID,Report_Con_ID ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", ConID.Trim())
                    });
                    da = new SqlDataAdapter(sqlcommand);
                    da.Fill(dtBonusSum);

                    //取出每筆季報表,根據月報表ID抓出獎金計算總合並寫回dtResult
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        int Calculate_Year = Convert.ToInt32(dtResult.Rows[i]["Calculate_Year"]);
                        string Calculate_Quarterly = Convert.ToString(dtResult.Rows[i]["Calculate_Quarterly"]);

                        //年度小於系統年直接計算(第四季除外),若等於或大於系統年則繼續判斷季
                        if(Calculate_Year>=Convert.ToInt32(year))
                        {
                            if (Calculate_Quarterly == "1")
                            {
                                if (Convert.ToInt32(month) < 4)
                                {
                                    dtResult.Rows[i]["IsVisible"] = 0;
                                    continue;
                                }                            
                                if (Convert.ToInt32(month) == 4)
                                {
                                    if (Convert.ToInt32(day) < 15)
                                    {
                                        dtResult.Rows[i]["IsVisible"] = 0;
                                        continue;
                                    }
                                }
                            }else if (Calculate_Quarterly == "2")
                            {
                                if (Convert.ToInt32(month) < 7)
                                {
                                    dtResult.Rows[i]["IsVisible"] = 0;
                                    continue;
                                }
                                if (Convert.ToInt32(month) == 7)
                                {
                                    if (Convert.ToInt32(day) < 15)
                                    {
                                        dtResult.Rows[i]["IsVisible"] = 0;
                                        continue;
                                    }
                                }
                            }
                            else if (Calculate_Quarterly == "3")
                            {
                                if (Convert.ToInt32(month) < 10)
                                {
                                    dtResult.Rows[i]["IsVisible"] = 0;
                                    continue;
                                }
                                if (Convert.ToInt32(month) == 10)
                                {
                                    if (Convert.ToInt32(day) < 15)
                                    {
                                        dtResult.Rows[i]["IsVisible"] = 0;
                                        continue;
                                    }
                                }
                            }
                            else if (Calculate_Quarterly == "4")
                            {
                                {
                                    dtResult.Rows[i]["IsVisible"] = 0;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (Calculate_Quarterly == "4")
                            {
                                if (Convert.ToInt32(month) == 1)
                                {
                                    if (Convert.ToInt32(day) < 15)
                                    {
                                        dtResult.Rows[i]["IsVisible"] = 0;
                                        continue;
                                    }
                                }
                            }
                        }
                        decimal totalBonus = 0;
                        string condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport1_ID"] + "'";
                        DataRow[] drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport2_ID"] + "'";
                        drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        condition = "MonthlyReport_ID='" + dtResult.Rows[i]["MonthlyReport3_ID"] + "'";
                        drs = dtBonusSum.Select(condition);
                        if (drs.Length > 0)
                            totalBonus += Convert.ToDecimal(drs[0]["Bonus"]);

                        dtResult.Rows[i]["Bonus"] = totalBonus;
                        dtResult.Rows[i]["Report_Con_ID"] = ConID.Trim();
                        dtResult.Rows[i]["IsVisible"] = 1;
                    }
                    newResult = dtResult.Clone();
                    //依照IsVisible filter
                    foreach (DataRow dr in dtResult.Select("IsVisible=1"))
                    {
                        newResult.ImportRow(dr);
                    }
                }

                return Json(newResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetReportView(string Report_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select distinct Report_Con_ID, Calculate_Year, Calculate_Month, 1 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where MonthlyReport_ID=(select MonthlyReport1_ID from BonusReportMaster where Report_ID=@Report_ID 
						                                                and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster where Report_ID=@Report_ID )) 
                                                union
                                                select distinct Report_Con_ID, Calculate_Year, Calculate_Month, 2 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where MonthlyReport_ID=(select MonthlyReport2_ID from BonusReportMaster where Report_ID=@Report_ID
						                                                and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster where Report_ID=@Report_ID )) 
                                                union all
                                                select distinct Report_Con_ID, Calculate_Year, Calculate_Month, 3 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where MonthlyReport_ID=(select MonthlyReport3_ID from BonusReportMaster where Report_ID=@Report_ID
						                                                and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster where Report_ID=@Report_ID ))   ";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetReportViewByID(string ID, string Report_Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select distinct Con_ID, Calculate_Year, Calculate_Month, 1 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where Report_Con_ID=@ConID and 
                                                MonthlyReport_ID=(select MonthlyReport1_ID from BonusReportMaster where ID=@ID) 
                                                union 
                                                select distinct Con_ID, Calculate_Year, Calculate_Month, 2 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where Report_Con_ID=@ConID and 
                                                MonthlyReport_ID=(select MonthlyReport2_ID from BonusReportMaster where ID=@ID) 
                                                union 
                                                select distinct Con_ID, Calculate_Year, Calculate_Month, 3 as MonNo,MonthlyReport_ID from BonusReportDetail
                                                where Report_Con_ID=@ConID and 
                                                MonthlyReport_ID=(select MonthlyReport3_ID from BonusReportMaster where ID=@ID)  ";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", Report_Con_ID),
                        new SqlParameter("@ID", ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult TableReport(string MonthlyReport_ID, string Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.*, B.Total_Amount from
                                                (select A.Report_Con_ID, sum(Bonus)Bonus,
                                                    (B.Con_ChiNAME_Last+B.Con_ChiNAME_First)as ConName  
                                                    from BonusReportDetail A
                                                    left join ConInfoDetail B on A.Report_Con_ID=B.Con_ID
                                                    where MonthlyReport_ID=@MonthlyReport_ID and A.Report_Con_ID=@Con_ID
                                                    group by A.Report_Con_ID, B.Con_ChiNAME_Last, B.Con_ChiNAME_First)A
											    left join (select top 1 Report_Con_ID, Total_Amount from BonusReportDetail
											    where MonthlyReport_ID=@MonthlyReport_ID and Report_Con_ID=@Con_ID) B on A.Report_Con_ID=B.Report_Con_ID  ";
                    

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Con_ID", Con_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult TableDetailReport(string MonthlyReport_ID, string Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Cli_ID, A.Deposit_Amount, A.Withdrawal_Amount, A.Total_Amount,
                                                C.CODE_DESC, A.BonusType_Rate, A.DepositType_Rate, A.Bonus,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First) as CliName  
                                                from BonusReportDetail A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join CodeList C on A.Bonus_Type=C.CODE_NO and C.CODE_Status=1 and C.CODE_TYPE='Bonus_Type'
                                                where MonthlyReport_ID=@MonthlyReport_ID and A.Report_Con_ID=@Con_ID  
                                                order by A.Cli_ID asc, A.Bonus_Type asc";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Con_ID", Con_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult TableReportIndivi(string MonthlyReport_ID, string Con_ID, string Report_Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Con_ID, Total_Amount, sum(Bonus)Bonus,
                                                (B.Con_ChiNAME_Last+B.Con_ChiNAME_First)as ConName  
                                                from BonusReportDetail A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where MonthlyReport_ID=@MonthlyReport_ID and A.Report_Con_ID=@Report_Con_ID and A.Con_ID=@Con_ID
                                                group by A.Con_ID, B.Con_ChiNAME_Last, B.Con_ChiNAME_First,Total_Amount";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Report_Con_ID", Report_Con_ID),
                        new SqlParameter("@Con_ID", Con_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult TableDetailReportIndivi(string MonthlyReport_ID, string Con_ID, string Report_Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select A.Cli_ID, A.Deposit_Amount, A.Withdrawal_Amount, A.Total_Amount,
                                                C.CODE_DESC, A.BonusType_Rate, A.DepositType_Rate, A.Bonus,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)  as CliName  , A.Bonus_Type
                                                from BonusReportDetail A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join CodeList C on A.Bonus_Type=C.CODE_NO and C.CODE_Status=1 and C.CODE_TYPE='Bonus_Type'
                                                where MonthlyReport_ID=@MonthlyReport_ID and A.Con_ID=@Con_ID and A.Report_Con_ID=@Report_Con_ID  ";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Con_ID", Con_ID),
                        new SqlParameter("@Report_Con_ID", Report_Con_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult TableReportIndivi_POST(string MonthlyReport_ID, string Con_ID, string Report_Con_ID)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("MonthlyReport_ID", typeof(string));
            dtResult.Columns.Add("Con_ID", typeof(string));
            dtResult.Columns.Add("Total_Amount", typeof(double));
            dtResult.Columns.Add("Bonus", typeof(double));
            dtResult.Columns.Add("ConName", typeof(string));


            string[] arryMonthlyReport_ID = MonthlyReport_ID.Split(',').Distinct().ToArray();
            string[] arryCon_ID = Con_ID.Split(',');
            string MonthlyReport_IDList = "";
            DataTable dt = new DataTable();
            
            for (int i = 0; i < arryMonthlyReport_ID.Length; i++)
            {
                if (i == 0)
                    MonthlyReport_IDList = "'"+arryMonthlyReport_ID[i]+"'";
                else
                    MonthlyReport_IDList += ",'" + arryMonthlyReport_ID[i]+"'";
                //foreach(DataRow dr in dt.Rows)
                //{
                //    dtResult.ImportRow(dr);
                //}
            }
            dt = GetReportIndivi(MonthlyReport_IDList);

            return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            
        }
        public DataTable GetReportIndivi(string MonthlyReport_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select MonthlyReport_ID, A.Con_ID, Total_Amount, sum(Bonus)Bonus,
                                                (B.Con_ChiNAME_Last+B.Con_ChiNAME_First)as ConName  
                                                from BonusReportDetail A
                                                left join ConInfoDetail B on A.Con_ID=B.Con_ID
                                                where MonthlyReport_ID in ("+ MonthlyReport_ID + @") 
                                                group by  MonthlyReport_ID, A.Con_ID, B.Con_ChiNAME_Last, B.Con_ChiNAME_First,Total_Amount
                                                order by MonthlyReport_ID, A.Con_ID ";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return dt;
            }
        }
        [HttpPost]
        public ActionResult TableDetailReportIndivi_POST(string MonthlyReport_ID, string Con_ID, string Report_Con_ID)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("MonthlyReport_ID", typeof(string));
            dtResult.Columns.Add("Con_ID", typeof(string));
            dtResult.Columns.Add("Cli_ID", typeof(string));
            dtResult.Columns.Add("Deposit_Amount", typeof(double));
            dtResult.Columns.Add("Withdrawal_Amount", typeof(double));
            dtResult.Columns.Add("Total_Amount", typeof(double));
            dtResult.Columns.Add("CODE_DESC", typeof(string));
            dtResult.Columns.Add("BonusType_Rate", typeof(double));
            dtResult.Columns.Add("DepositType_Rate", typeof(double));
            dtResult.Columns.Add("Bonus", typeof(double));
            dtResult.Columns.Add("CliName", typeof(string));
            dtResult.Columns.Add("Bonus_Type", typeof(int));
            DataTable dt = new DataTable();

            string[] arryMonthlyReport_ID = MonthlyReport_ID.Split(',');
            string[] arryCon_ID = Con_ID.Split(',');

            for (int i = 0; i < arryCon_ID.Length; i++)
            {
                dt = GetDetailReportIndivi(arryMonthlyReport_ID[i], arryCon_ID[i], Report_Con_ID);
                foreach (DataRow dr in dt.Rows)
                {
                    dtResult.ImportRow(dr);
                }
            }
            return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
        }
        public DataTable GetDetailReportIndivi(string MonthlyReport_ID, string Con_ID, string Report_Con_ID)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select MonthlyReport_ID, A.Con_ID, A.Cli_ID, A.Deposit_Amount, A.Withdrawal_Amount, A.Total_Amount,
                                                C.CODE_DESC, A.BonusType_Rate, A.DepositType_Rate, A.Bonus,
                                                (B.Cli_ChiNAME_Last+B.Cli_ChiNAME_First)  as CliName  , A.Bonus_Type
                                                from BonusReportDetail A
                                                left join CliInfoDetail B on A.Cli_ID=B.Cli_ID
                                                left join CodeList C on A.Bonus_Type=C.CODE_NO and C.CODE_Status=1 and C.CODE_TYPE='Bonus_Type'
                                                where MonthlyReport_ID=@MonthlyReport_ID and A.Con_ID=@Con_ID and A.Report_Con_ID=@Report_Con_ID  ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Report_Con_ID", Report_Con_ID),
                        new SqlParameter("@Con_ID", Con_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return dt;
            }
        }
        [HttpGet]
        public ActionResult GetReportMasterID(string Report_ID, string Report_Ver)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select ID from BonusReportMaster
                                                where Report_ID=@Report_ID and Report_Ver=@Report_Ver";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID),
                        new SqlParameter("@Report_Ver", Report_Ver)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                return Json(dt.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetReportSumById(string ID, string Report_Con_ID)
        {
            DataRow dr;
            DataTable dt = new DataTable();
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Con_ROLE", typeof(string));
            dtResult.Columns.Add("Type1Bouns", typeof(double));//差階
            dtResult.Columns.Add("Type2Bouns", typeof(double));//輔導
            dtResult.Columns.Add("Type3Bouns", typeof(double));//體系
            dtResult.Columns.Add("Type4Bouns", typeof(double));//未領
            dtResult.Columns.Add("Type5Bouns", typeof(double));//共享
            dtResult.Columns.Add("TotalBouns", typeof(double));//合計

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Con_ROLE from ConInfo where Con_ID=@ConID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", Report_Con_ID)
                    });

                string ConRole = Convert.ToString(sqlcommand.ExecuteScalar());

                sqlcommandstring = @"  select A.Bonus_Type, sum(A.Bonus) as Bonus
                                                    from (select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport1_ID from BonusReportMaster where ID=@ID) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport2_ID from BonusReportMaster where ID=@ID) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport3_ID from BonusReportMaster where ID=@ID)) A
                                                    group by A.Bonus_Type ";


                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", Report_Con_ID),
                        new SqlParameter("@ID", ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                //因查出來的獎金為直向,要轉為橫向並加入顧問腳色和獎金總和
                if (dt.Rows.Count > 0)
                {
                    decimal bouns1 = 0;
                    decimal bouns2 = 0;
                    decimal bouns3 = 0;
                    decimal bouns4 = 0;
                    decimal bouns5 = 0;
                    decimal bouns6 = 0;
                    dr = dtResult.NewRow();
                    dr["Con_ROLE"] = ConRole;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["Bonus_Type"].ToString() == "1")
                            bouns1 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "2")
                            bouns2 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "3")
                            bouns3 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "4")
                            bouns4 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "5")
                            bouns5 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "6")
                            bouns6 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                    }
                    dr["Type1Bouns"] = bouns1;
                    dr["Type2Bouns"] = bouns2;
                    dr["Type3Bouns"] = bouns3;
                    dr["Type4Bouns"] = bouns4;
                    dr["Type5Bouns"] = bouns5 + bouns6;
                    dr["TotalBouns"] = bouns1 + bouns2 + bouns3 + bouns4 + bouns5 + bouns6;
                    dtResult.Rows.Add(dr);
                }
                else
                {
                    dr = dtResult.NewRow();
                    dr["Con_ROLE"] = ConRole;
                    dr["Type1Bouns"] = 0;
                    dr["Type2Bouns"] = 0;
                    dr["Type3Bouns"] = 0;
                    dr["Type4Bouns"] = 0;
                    dr["Type5Bouns"] = 0;
                    dr["TotalBouns"] = 0;
                    dtResult.Rows.Add(dr);
                }
                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetReportSum(string Report_ID)
        {
            DataRow dr;
            DataTable dt = new DataTable();
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Con_ROLE", typeof(string));
            dtResult.Columns.Add("Type1Bouns", typeof(double));//差階
            dtResult.Columns.Add("Type2Bouns", typeof(double));//輔導
            dtResult.Columns.Add("Type3Bouns", typeof(double));//體系
            dtResult.Columns.Add("Type4Bouns", typeof(double));//未領
            dtResult.Columns.Add("Type5Bouns", typeof(double));//共享
            dtResult.Columns.Add("TotalBouns", typeof(double));//合計

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                //string sqlcommandstring = @"  select Con_ROLE from ConInfo where Con_ID=@ConID ";

                //SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                //sqlcommand.Parameters.AddRange(new SqlParameter[] {
                //        new SqlParameter("@ConID", Report_Con_ID)
                //    });

                //string ConRole = Convert.ToString(sqlcommand.ExecuteScalar());

                string sqlcommandstring = @"  select A.Bonus_Type, sum(A.Bonus) as Bonus
                                                    from (select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport1_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport2_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport3_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc)) A
                                                    group by A.Bonus_Type ";


                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                //因查出來的獎金為直向,要轉為橫向並加入顧問腳色和獎金總和
                if (dt.Rows.Count > 0)
                {
                    decimal bouns1 = 0;
                    decimal bouns2 = 0;
                    decimal bouns3 = 0;
                    decimal bouns4 = 0;
                    decimal bouns5 = 0;
                    decimal bouns6 = 0;
                    dr = dtResult.NewRow();
                    //dr["Con_ROLE"] = ConRole;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["Bonus_Type"].ToString() == "1")
                            bouns1 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "2")
                            bouns2 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "3")
                            bouns3 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "4")
                            bouns4 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "5")
                            bouns5 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "6")
                            bouns6 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                    }
                    dr["Type1Bouns"] = bouns1;
                    dr["Type2Bouns"] = bouns2;
                    dr["Type3Bouns"] = bouns3;
                    dr["Type4Bouns"] = bouns4;
                    dr["Type5Bouns"] = bouns5+ bouns6;
                    dr["TotalBouns"] = bouns1 + bouns2 + bouns3 + bouns4 + bouns5 + bouns6;
                    dtResult.Rows.Add(dr);
                }
                else
                {
                    dr = dtResult.NewRow();
                    //dr["Con_ROLE"] = ConRole;
                    dr["Type1Bouns"] = 0;
                    dr["Type2Bouns"] = 0;
                    dr["Type3Bouns"] = 0;
                    dr["Type4Bouns"] = 0;
                    dr["Type5Bouns"] = 0;
                    dr["TotalBouns"] = 0;
                    dtResult.Rows.Add(dr);
                }
                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetReportConList(string Report_ID)
        {
            DataRow dr;
            DataTable dt = new DataTable();
            DataTable dtSum= new DataTable();
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("ChiName", typeof(string));
            dtResult.Columns.Add("Con_ID", typeof(string));
            dtResult.Columns.Add("Type1Bouns", typeof(double));//差階
            dtResult.Columns.Add("Type2Bouns", typeof(double));//輔導
            dtResult.Columns.Add("Type3Bouns", typeof(double));//體系
            dtResult.Columns.Add("Type4Bouns", typeof(double));//未領
            dtResult.Columns.Add("Type5Bouns", typeof(double));//共享
            dtResult.Columns.Add("TotalBouns", typeof(double));//合計

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" select distinct Report_Con_ID, (CID.Con_ChiNAME_Last+CID.Con_ChiNAME_First)as ChiName
                                                    from (select Report_Con_ID
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport1_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc) 
                                                    union all
                                                    select Report_Con_ID
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport2_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc) 
                                                    union all
                                                    select Report_Con_ID
                                                    from BonusReportDetail
                                                    where MonthlyReport_ID=(select top 1 MonthlyReport3_ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc)) A
                                                    left join ConInfoDetail CID on A.Report_Con_ID=CID.Con_ID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID)
                    });
                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                dt.Columns.Add("ID", typeof(string));

                sqlcommandstring = @"  	select top 1 ID from BonusReportMaster where Report_ID=@Report_ID order by Report_Ver desc ";

                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID)
                    });
                string ID = Convert.ToString(sqlcommand.ExecuteScalar());

                for(int i=0; i < dt.Rows.Count; i++)
                {
                    dtSum = GetReportConListById(ID, dt.Rows[i]["Report_Con_ID"].ToString());
                    dr = dtResult.NewRow();
                    dr["ChiName"] = dt.Rows[i]["ChiName"].ToString();
                    dr["Con_ID"] = dt.Rows[i]["Report_Con_ID"].ToString();
                    dr["Type1Bouns"] = dtSum.Rows[0]["Type1Bouns"];
                    dr["Type2Bouns"] = dtSum.Rows[0]["Type2Bouns"];
                    dr["Type3Bouns"] = dtSum.Rows[0]["Type3Bouns"];
                    dr["Type4Bouns"] = dtSum.Rows[0]["Type4Bouns"]; 
                    dr["Type5Bouns"] = dtSum.Rows[0]["Type5Bouns"];
                    dr["TotalBouns"] = dtSum.Rows[0]["TotalBouns"];
                    dtResult.Rows.Add(dr);
                }
                return Json(dtResult.ToJson(), JsonRequestBehavior.AllowGet);
            }
        }
        public DataTable GetReportConListById(string ID, string Report_Con_ID)
        {
            DataRow dr;
            DataTable dt = new DataTable();
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("Con_ROLE", typeof(string));
            dtResult.Columns.Add("Type1Bouns", typeof(double));//差階
            dtResult.Columns.Add("Type2Bouns", typeof(double));//輔導
            dtResult.Columns.Add("Type3Bouns", typeof(double));//體系
            dtResult.Columns.Add("Type4Bouns", typeof(double));//未領
            dtResult.Columns.Add("Type5Bouns", typeof(double));//共享
            dtResult.Columns.Add("TotalBouns", typeof(double));//合計

            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select Con_ROLE from ConInfo where Con_ID=@ConID ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", Report_Con_ID)
                    });

                string ConRole = Convert.ToString(sqlcommand.ExecuteScalar());

                sqlcommandstring = @"  select A.Bonus_Type, sum(A.Bonus) as Bonus
                                                    from (select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport1_ID from BonusReportMaster where ID=@ID) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport2_ID from BonusReportMaster where ID=@ID) 
                                                    union all
                                                    select Bonus_Type, Bonus
                                                    from BonusReportDetail
                                                    where Report_Con_ID=@ConID and MonthlyReport_ID=(select MonthlyReport3_ID from BonusReportMaster where ID=@ID)) A
                                                    group by A.Bonus_Type ";


                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@ConID", Report_Con_ID),
                        new SqlParameter("@ID", ID)
                    });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);

                //因查出來的獎金為直向,要轉為橫向並加入顧問腳色和獎金總和
                if (dt.Rows.Count > 0)
                {
                    decimal bouns1 = 0;
                    decimal bouns2 = 0;
                    decimal bouns3 = 0;
                    decimal bouns4 = 0;
                    decimal bouns5 = 0;
                    decimal bouns6 = 0;
                    dr = dtResult.NewRow();
                    dr["Con_ROLE"] = ConRole;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["Bonus_Type"].ToString() == "1")
                            bouns1 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "2")
                            bouns2 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "3")
                            bouns3 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "4")
                            bouns4 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "5")
                            bouns5 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                        else if (dt.Rows[i]["Bonus_Type"].ToString() == "6")
                            bouns6 = Convert.ToDecimal(dt.Rows[i]["Bonus"]);
                    }
                    dr["Type1Bouns"] = bouns1;
                    dr["Type2Bouns"] = bouns2;
                    dr["Type3Bouns"] = bouns3;
                    dr["Type4Bouns"] = bouns4;
                    dr["Type5Bouns"] = bouns5 + bouns6;
                    dr["TotalBouns"] = bouns1 + bouns2 + bouns3 + bouns4 + bouns5 + bouns6;
                    dtResult.Rows.Add(dr);
                }
                else
                {
                    dr = dtResult.NewRow();
                    dr["Con_ROLE"] = ConRole;
                    dr["Type1Bouns"] = 0;
                    dr["Type2Bouns"] = 0;
                    dr["Type3Bouns"] = 0;
                    dr["Type4Bouns"] = 0;
                    dr["Type5Bouns"] = 0;
                    dr["TotalBouns"] = 0;
                    dtResult.Rows.Add(dr);
                }
                return dtResult;
            }
        }
        public virtual ActionResult UploadFile()
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
                    string filePath = Server.MapPath("~/Content/files/CliPayed/");
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

                        string insertResult = insertData(fileLocation);
                        if (insertResult != "")
                            return Json(new { isUploaded = false, result = "第" + insertResult + "行資料有誤或無該編號或mail發送失敗", filename = "" }, "text/html");
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
                log.writeLogToDB("", "PayedRecord/UploadFile", ex.ToString());
                return Json(new { isUploaded = false, result = "系統發生錯誤", filename = "" }, "text/html");
            }
        }
        public string insertData(string fileLocation)
        {
            string result = "";

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

                SendMail send = new SendMail();

                for (int row = 1; row <= sheet.LastRowNum; row++) // 使用For 走訪所有的資料列
                {
                    //--------------insert value--------
                    string Cli_ID = "";
                    DateTime InterestInterval_Start;
                    DateTime InterestInterval_End;
                    string Interest_Amount = "";
                    DateTime InterestPayedDate;
                    //----------------------------------

                    if (sheet.GetRow(row) != null) // 驗證是不是空白列
                    {
                        try
                        {
                            if (sheet.GetRow(row).GetCell(0) != null)
                                Cli_ID = sheet.GetRow(row).GetCell(0).ToString().Trim(); // 字串
                            else
                            {
                                result += row + 1 + ". "; continue;
                            }
                            if (Cli_ID == "") //空白行跳過
                                continue;
                            if (sheet.GetRow(row).GetCell(1) != null)
                                InterestInterval_Start = sheet.GetRow(row).GetCell(1).DateCellValue;
                            else
                            {
                                result += row + 1 + ". "; continue;
                            }
                            if (sheet.GetRow(row).GetCell(2) != null)
                                InterestInterval_End = sheet.GetRow(row).GetCell(2).DateCellValue;
                            else
                            {
                                result += row + 1 + ". "; continue;
                            }
                            if (sheet.GetRow(row).GetCell(3) != null)
                                Interest_Amount = sheet.GetRow(row).GetCell(3).NumericCellValue.ToString();
                            else
                            {
                                result += row + 1 + ". "; continue;
                            }
                            if (sheet.GetRow(row).GetCell(4) != null)
                                InterestPayedDate = sheet.GetRow(row).GetCell(4).DateCellValue;
                            else
                            {
                                result += row + 1 + ". "; continue;
                            }
                        }
                        catch
                        {
                            result += row + 1 + ". "; continue;
                        }
                        //檢查是否有該客戶編號
                        sqlcommandstring = @" select count(*) from CliInfo  
                                                    where  Cli_ID=@Cli_ID";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                                new SqlParameter("@Cli_ID", Cli_ID)
                                            });
                        int count = Convert.ToInt32(sqlcommand.ExecuteScalar());

                        if (count == 0)
                        {
                            result += row + 1 + ". "; continue;
                        }
                        else
                        {
                            try
                            {
                                //寫入資料表
                                sqlcommandstring = @" insert CliInterest values(@Cli_ID, @InterestInterval_Start, @InterestInterval_End,
                                                        @Interest_Amount, @InterestPayedDate, @date)";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                                new SqlParameter("@Cli_ID", Cli_ID.Trim()),
                                                new SqlParameter("@InterestInterval_Start", InterestInterval_Start),
                                                new SqlParameter("@InterestInterval_End", InterestInterval_End),
                                                new SqlParameter("@Interest_Amount", Interest_Amount),
                                                new SqlParameter("@InterestPayedDate", InterestPayedDate),
                                                new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                                            });
                                sqlcommand.ExecuteNonQuery();

                                //取得客戶或顧問的email
                                sqlcommandstring = @" select Cli_Email from CliInfoDetail where  Cli_ID=@Cli_ID";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                        new SqlParameter("@Cli_ID", Cli_ID.Trim())
                                    });
                                string receEmail = Convert.ToString(sqlcommand.ExecuteScalar()); //收件信箱

                                if (receEmail.Trim() == "") { result += row + 1 + ". "; continue;  }

                                string content = "客戶" + Cli_ID.Trim() + "您好: 您的發息期間" + InterestInterval_Start + "到" + InterestInterval_End + "已於" + InterestPayedDate + "發息。";
                                if (!send.Send(receEmail, "發息通知", content))
                                {
                                    result += row + 1 + ". "; continue;
                                }
                            }
                            catch
                            {
                                result += row + 1 + ". "; continue;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}