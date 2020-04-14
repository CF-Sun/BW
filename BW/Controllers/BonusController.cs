using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class BonusController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();

        [HttpGet]
        public int GetIsCalculate()
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @" SELECT count(*)
                                              FROM BonusReportMaster 
                                              where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly ";

                DateTime dtime = convertTime.UStoTW(DateTime.Now);
                string year = dtime.ToString("yyyy");//年
                string mon = dtime.AddMonths(-1).ToString("MM");//上個月月份
                string quarterly = "";

                if (mon == "01" || mon == "02" || mon == "03") //Q1
                    quarterly = "1";
                else if (mon == "04" || mon == "05" || mon == "06") //Q2
                    quarterly = "2";
                else if (mon == "07" || mon == "08" || mon == "09") //Q3
                    quarterly = "3";
                else if (mon == "10" || mon == "11" || mon == "12") //Q4
                    quarterly = "4";

                //如果是12月 年份就要往前推
                if (mon == "12")
                    year = dtime.AddYears(-1).ToString("yyyy");

                if (mon == "01" || mon == "04" || mon == "07" || mon == "10")
                    sqlcommandstring += " and MonthlyReport1_ID is not null and MonthlyReport1_ID !='' ";
                else if (mon == "02" || mon == "05" || mon == "08" || mon == "11")
                    sqlcommandstring += " and MonthlyReport2_ID is not null and MonthlyReport2_ID !='' ";
                else if (mon == "03" || mon == "06" || mon == "09" || mon == "12")
                    sqlcommandstring += " and MonthlyReport3_ID is not null and MonthlyReport3_ID !='' ";

                sqlcommandstring += @" and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster 
				                                            where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly )";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@Calculate_Year", year),
                    new SqlParameter("@Calculate_Quarterly", quarterly)
                    });

                return Convert.ToInt32(sqlcommand.ExecuteScalar());
            }
        }
        [HttpPost]
        public int ReCalculateBonus(bool IsReCalculate, string ReCalYeay, string ReCalMonth)
        {
            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"  select distinct Calculate_Year, Calculate_Month
                                               from BonusReportDetail 
	                                           where Calculate_Year+Calculate_Month>=@YearMonth
	                                           order by Calculate_Year asc, Calculate_Month asc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@YearMonth", ReCalYeay+ReCalMonth)
                                });

                SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                da.Fill(dt);
            }

            for(int i=0; i<dt.Rows.Count; i++)
            {
                int result= CalculateBonus(IsReCalculate, dt.Rows[i]["Calculate_Year"].ToString(), dt.Rows[i]["Calculate_Month"].ToString());
                if (result == 1)
                {
                    if(i == dt.Rows.Count-1)
                        return 1;
                    continue;
                }
                else
                {
                    return 0;
                }             
            }
            return 1;

        }

        //宣告共用變數-------------------------------------------------------
        public DataTable dtConInfoById = new DataTable();
        public DataTable dtConInfo = new DataTable();//儲存顧問資料
        public DataTable dtDepositDetail = new DataTable();//儲存存款資料
        public DataTable dtBonusDistribution = new DataTable();//儲存獎金分配
        public DataTable dtResult = new DataTable();//儲存計算結果
        DataRow row;
        int MEM_HIERA = 0;
        public string Report_Con_ID; //此筆報表的所屬顧問
        public string MonthlyReport_ID;
        DateTime dtime;
        public string year = "";//年
        public string mon = "";//上個月月份
        public string quarterly = "";
        //--------------------------------------------------------------------
        [HttpPost]
        public int CalculateBonus(bool IsReCalculate, string ReCalYeay, string ReCalMonth)
        {
            //初始化宣告
            dtime = convertTime.UStoTW(DateTime.Now);
            dtConInfoById = new DataTable();
            dtConInfo = new DataTable();//儲存顧問資料
            dtDepositDetail = new DataTable();//儲存存款資料
            dtBonusDistribution = new DataTable();//儲存獎金分配
            dtResult = new DataTable();//儲存計算結果
            MEM_HIERA = 0;
            Report_Con_ID = ""; //此筆報表的所屬顧問
            MonthlyReport_ID = "";
            year = "";//年
            mon = "";//上個月月份
            quarterly = "";
            //dtime = dtime.AddDays(-dtime.Day);
            try
            {
                DataRow dr;
                dtResult.Columns.Add("Con_ID", typeof(string));
                dtResult.Columns.Add("CounselBonus", typeof(double));
                dtResult.Columns.Add("TutorBonus", typeof(double));

                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                string condition = ""; //datatable篩選條件

                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection sqlconnection = new SqlConnection(conn))
                    {
                        sqlconnection.Open();
                        ////取得尚未計算的月份
                        //List<monthList> monLists = getMonLists();

                        string sqlcommandstring = "";
                        SqlCommand sqlcommand;

                        #region 是否新增報表或重算
                        if (!IsReCalculate)
                        {
                            //取得月報表ID最大值+1為此次計算的報表ID
                            sqlcommandstring = @" select ISNULL(max(MonthlyReport_ID),0) from BonusReportDetail ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            MonthlyReport_ID = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                            while (MonthlyReport_ID.Length < 10)  //ID長度為10 不足長度補0
                            {
                                MonthlyReport_ID = "0" + MonthlyReport_ID;
                            }

                            #region //新增或更新主表
                            year = dtime.ToString("yyyy");//年
                            mon = dtime.AddMonths(-1).ToString("MM");//上個月月份
                            quarterly = "";

                            if (mon == "01" || mon == "02" || mon == "03") //Q1
                                quarterly = "1";
                            else if (mon == "04" || mon == "05" || mon == "06") //Q2
                                quarterly = "2";
                            else if (mon == "07" || mon == "08" || mon == "09") //Q3
                                quarterly = "3";
                            else if (mon == "10" || mon == "11" || mon == "12") //Q4
                                quarterly = "4";

                            //如果是12月 年份就要往前推
                            if (mon == "12")
                                year = dtime.AddYears(-1).ToString("yyyy");

                            //查詢是否有該季資料
                            sqlcommandstring = " select count(*) from BonusReportMaster where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                new SqlParameter("@Calculate_Year", year),
                                new SqlParameter("@Calculate_Quarterly", quarterly)
                                });
                            int count = Convert.ToInt32(sqlcommand.ExecuteScalar());

                            //判斷要寫入還是更新
                            if (count == 0) //寫入
                            {
                                //取得主報表ID+1
                                sqlcommandstring = @" select ISNULL(max(Report_ID),0) from BonusReportMaster ";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                string Report_ID = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                                while (Report_ID.Length < 10)  //ID長度為10 不足長度補0
                                {
                                    Report_ID = "0" + Report_ID;
                                }

                                sqlcommandstring = @" Insert BonusReportMaster values(@Report_ID, '1', ";
                                if (mon == "01" || mon == "04" || mon == "07" || mon == "10")
                                    sqlcommandstring += " @MonthlyReport_ID,'','',@Calculate_Year, @Calculate_Quarterly) ";
                                else if (mon == "02" || mon == "05" || mon == "08" || mon == "11")
                                    sqlcommandstring += " '', @MonthlyReport_ID,'', @Calculate_Year, @Calculate_Quarterly) ";
                                else if (mon == "03" || mon == "06" || mon == "09" || mon == "12")
                                    sqlcommandstring += " '','',@MonthlyReport_ID, @Calculate_Year, @Calculate_Quarterly) ";

                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@Report_ID", Report_ID),
                                    new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                                    new SqlParameter("@Calculate_Year", year),
                                    new SqlParameter("@Calculate_Quarterly", quarterly)
                                });
                                sqlcommand.ExecuteNonQuery();
                            }
                            else //更新
                            {
                                sqlcommandstring = @" update BonusReportMaster ";

                                if (mon == "01" || mon == "04" || mon == "07" || mon == "10")
                                    sqlcommandstring += " set MonthlyReport1_ID=@MonthlyReport_ID ";
                                else if (mon == "02" || mon == "05" || mon == "08" || mon == "11")
                                    sqlcommandstring += " set MonthlyReport2_ID=@MonthlyReport_ID ";
                                else if (mon == "03" || mon == "06" || mon == "09" || mon == "12")
                                    sqlcommandstring += " set MonthlyReport3_ID=@MonthlyReport_ID ";

                                sqlcommandstring += @"  where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly 
                                            and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster 
				                                            where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly )";

                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                                    new SqlParameter("@Calculate_Year", year),
                                    new SqlParameter("@Calculate_Quarterly", quarterly)
                                });
                                sqlcommand.ExecuteNonQuery();
                            }
                            #endregion
                            //InsertReportMaster();//新增或更新主表
                        }
                        else
                        {
                            int Report_Ver = 1;
                            year = ReCalYeay;
                            mon = ReCalMonth;

                            if (ReCalMonth == "01" || ReCalMonth == "02" || ReCalMonth == "03") //Q1
                                quarterly = "1";
                            else if (ReCalMonth == "04" || ReCalMonth == "05" || ReCalMonth == "06") //Q2
                                quarterly = "2";
                            else if (ReCalMonth == "07" || ReCalMonth == "08" || ReCalMonth == "09") //Q3
                                quarterly = "3";
                            else if (ReCalMonth == "10" || ReCalMonth == "11" || ReCalMonth == "12") //Q4
                                quarterly = "4";

                            //取得重算月份的Master的資料最大版本號
                            sqlcommandstring = " select MAX(Report_Ver) from BonusReportMaster where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@Calculate_Year", ReCalYeay),
                                    new SqlParameter("@Calculate_Quarterly", quarterly)
                                    });
                            Report_Ver = Convert.ToInt16(sqlcommand.ExecuteScalar()) + 1;

                            if (Report_Ver == 1)
                                return 0;

                            //取得月報表ID最大值+1為此次計算的報表ID
                            sqlcommandstring = @" select ISNULL(max(MonthlyReport_ID),0) from BonusReportDetail ";
                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            MonthlyReport_ID = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                            while (MonthlyReport_ID.Length < 10)  //ID長度為10 不足長度補0
                            {
                                MonthlyReport_ID = "0" + MonthlyReport_ID;
                            }

                            //新報表ID寫入主表
                            sqlcommandstring += @" Insert BonusReportMaster
                                            select top 1 Report_ID, '" + Report_Ver + "', ";

                            if (ReCalMonth == "01" || ReCalMonth == "04" || ReCalMonth == "07" || ReCalMonth == "10")
                                sqlcommandstring += " '" + MonthlyReport_ID + "', MonthlyReport2_ID, MonthlyReport3_ID, ";
                            else if (ReCalMonth == "02" || ReCalMonth == "05" || ReCalMonth == "08" || ReCalMonth == "11")
                                sqlcommandstring += " MonthlyReport1_ID, '" + MonthlyReport_ID + "', MonthlyReport3_ID, ";
                            else if (ReCalMonth == "03" || ReCalMonth == "06" || ReCalMonth == "09" || ReCalMonth == "12")
                                sqlcommandstring += " MonthlyReport1_ID, MonthlyReport2_ID, '" + MonthlyReport_ID + "', ";

                            sqlcommandstring += @" Calculate_Year,Calculate_Quarterly
                                            from BonusReportMaster where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly and Report_Ver=@Report_Ver ";

                            sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                            sqlcommand.Parameters.AddRange(new SqlParameter[] {
                                    new SqlParameter("@Calculate_Year", ReCalYeay),
                                    new SqlParameter("@Calculate_Quarterly", quarterly),
                                    new SqlParameter("@Report_Ver", Report_Ver-1)
                                    });
                            sqlcommand.ExecuteNonQuery();
                            dtime = DateTime.Parse(ReCalYeay + "/" + ReCalMonth).AddMonths(1);

                        }
                        #endregion
                        //取出顧問資料表,及總存款
                        //sqlcommandstring = @" Select CoI.*, ISNULL(DD.Deposit_Amount,0)-ISNULL(WA.Withdrawal_Amount,0) Amount,
                        //                        CH.Con_Hiera, EffectiveStartDate,EffectiveEndDate
                        //                        from ConInfo CoI
                        //                        left join (select Con_ID, sum(Deposit_Amount)as Deposit_Amount 
                        //                        from DepositList where Status=2 and Arrival_DATE<=@Arrival_DATE group by Con_ID) DD on CoI.Con_ID=DD.Con_ID
                        //                        left join (select Con_ID, sum(Withdrawal_Amount)as Withdrawal_Amount 
                        //                        from WithdrawalList where Status=2 and Arrival_DATE<=@Arrival_DATE group by Con_ID) WA  on CoI.Con_ID=WA.Con_ID
                        //                        left join ConHieraSetting CH on CoI.Con_ID=CH.Con_ID
                        //                        where CoI.Con_ID != '000' order by CoI.Con_LEVEL asc "; //排除公司
                        sqlcommandstring = @" Select CoI.*, ISNULL(DD.Deposit_Amount,0)-ISNULL(WA.Withdrawal_Amount,0) Amount,
                                                CH.Con_Hiera, EffectiveStartDate,EffectiveEndDate
                                                from ConInfo CoI
                                                left join (select Con_ID, sum(Deposit_Amount)as Deposit_Amount 
                                                from DepositList where Status=2 and Arrival_DATE<=@Arrival_DATE group by Con_ID) DD on CoI.Con_ID=DD.Con_ID
                                                left join (select Con_ID, sum(Withdrawal_Amount)as Withdrawal_Amount 
                                                from WithdrawalList where ExpectDate<=@ExpectDate group by Con_ID) WA  on CoI.Con_ID=WA.Con_ID
                                                left join ConHieraSetting CH on CoI.Con_ID=CH.Con_ID
                                                where CoI.Con_ID != '000' order by CoI.Con_LEVEL asc "; //排除公司
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Arrival_DATE", dtime.AddMonths(-2).AddDays(-dtime.Day).AddDays(23).ToString("yyyy/MM/dd")),
                            new SqlParameter("@ExpectDate", dtime.AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                            });
                        SqlDataAdapter da = new SqlDataAdapter(sqlcommand);
                        da.Fill(dtConInfo);
                        //string ddd = dtime.AddMonths(-2).AddDays(-dtime.Day).AddDays(23).ToString("yyyy/MM/dd");
                        //取出客戶存款資料表(含扣除出金)
                        //sqlcommandstring = @" select DL.*,ISNULL(DL.Deposit_Amount,0)DepositAmount, ISNULL(WL.Withdrawal_Amount,0) WithdrawalAmount,
                        //                    (ISNULL(DL.Deposit_Amount,0)-ISNULL(WL.Withdrawal_Amount,0))Amount
                        //                    from DepositList DL
                        //                    left join (select Con_ID, Deposit_ID, sum(Withdrawal_Amount)as Withdrawal_Amount 
                        //                       from WithdrawalList where Status=2 and Arrival_DATE<=@Arrival_DATE group by Con_ID,Deposit_ID) WL
                        //                       on DL.Con_ID=WL.Con_ID and DL.Deposit_ID=WL.Deposit_ID
                        //                    where DL.Status=2 and DL.Arrival_DATE<=@Arrival_DATE ";
                        sqlcommandstring = @" select DL.*, ISNULL(CASE WHEN DL.Arrival_DATE<=@Arrival_DATE THEN DL.Deposit_Amount ELSE 0 END,0) DepositAmount, 
				                            ISNULL(WL.Withdrawal_Amount,0) WithdrawalAmount,
                                            (ISNULL(CASE WHEN DL.Arrival_DATE<=@Arrival_DATE THEN DL.Deposit_Amount ELSE 0 END,0)-ISNULL(WL.Withdrawal_Amount,0))Amount,
                                            DT.Type_RATE
                                            from DepositList DL
                                            left join (select Con_ID, Deposit_ID, sum(Withdrawal_Amount)as Withdrawal_Amount 
			                                            from WithdrawalList where ExpectDate<=@ExpectDate group by Con_ID,Deposit_ID) WL
			                                            on DL.Con_ID=WL.Con_ID and DL.Deposit_ID=WL.Deposit_ID
                                            left join DepositType DT on DL.Deposit_Type=DT.Type_NO
                                            where DL.Status=2  ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        sqlcommand.Parameters.AddRange(new SqlParameter[] {
                            new SqlParameter("@Arrival_DATE", dtime.AddMonths(-2).AddDays(-dtime.Day).AddDays(23).ToString("yyyy/MM/dd")),
                            new SqlParameter("@ExpectDate", dtime.AddDays(-dtime.Day).ToString("yyyy/MM/dd"))
                            });
                        da = new SqlDataAdapter(sqlcommand);
                        da.Fill(dtDepositDetail);

                        //取出顧問獎金分配表
                        sqlcommandstring = @" select * from BonusDistribution ";
                        sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                        da = new SqlDataAdapter(sqlcommand);
                        da.Fill(dtBonusDistribution);

                        dtConInfoById = dtConInfo.Clone();
                        dtConInfoById.Columns.Add("Con_HIERA", typeof(int));
                        dtConInfoById.Columns.Add("Con_Rate", typeof(decimal));

                        sqlconnection.Close();
                        sqlconnection.Dispose();
                    }

                    //依序取出每個顧問進行處理
                    for (int i = 0; i < dtConInfo.Rows.Count; i++)
                    {
                        dtConInfoById.Clear();
                        MEM_HIERA = 1;

                        decimal Deposit_Amount = Convert.ToDecimal(dtConInfo.Rows[i]["Amount"]);
                        decimal rate;
                        string Con_ID = dtConInfo.Rows[i]["Con_ID"].ToString();
                        Report_Con_ID = Con_ID;
                        //判斷是否為自動,自動則以總存款計算rate, 若非自動再判斷有效日,若再有效日內則抓資料rate
                        if (!Convert.ToBoolean(dtConInfo.Rows[i]["IsAuto"]))
                        {
                            if (dtime.AddDays(-dtime.Day).CompareTo(Convert.ToDateTime(dtConInfo.Rows[i]["EffectiveStartDate"])) >= 0 && dtime.AddDays(-dtime.Day).CompareTo(Convert.ToDateTime(dtConInfo.Rows[i]["EffectiveEndDate"])) <= 0)
                                rate = Convert.ToDecimal(dtConInfo.Rows[i]["Con_Hiera"]);
                            else
                                rate = rateMapping(Deposit_Amount);
                        }
                        else
                        {
                            rate = rateMapping(Deposit_Amount);
                        }

                        //取得rate後,更新ConInfo rate, 並比對職級變更表該顧問最新的一筆,若rate不同則寫一筆新的到ConHieraRecord
                        updateHieraRecord(Con_ID, rate);

                        row = dtConInfoById.NewRow();
                        row["Con_ID"] = Con_ID;
                        row["Parent_Con_ID"] = dtConInfo.Rows[i]["Parent_Con_ID"].ToString();
                        row["Con_ROLE"] = dtConInfo.Rows[i]["Con_ROLE"].ToString();
                        row["Con_PATH"] = dtConInfo.Rows[i]["Con_PATH"].ToString();
                        row["Amount"] = Deposit_Amount;
                        row["Con_HIERA"] = MEM_HIERA;
                        row["Con_Rate"] = rate;
                        dtConInfoById.Rows.Add(row);

                        getParentMemInfo(Con_ID); //取出的顧問ID丟進fun去撈出他組織下所有顧問

                        //計算前先將該顧問寫進結果表 獎金為0
                        row = dtResult.NewRow();
                        row["Con_ID"] = Con_ID;
                        row["CounselBonus"] = 0;
                        row["TutorBonus"] = 0;
                        dtResult.Rows.Add(row);

                        condition = "Con_ID='" + Con_ID + "'";

                        //1. 計算差階獎金--------------START-----------------
                       
                        //自己的差階及以下每個人的差階獎接
                        decimal HIERABonus = (DepositeBonus(Con_ID, rate, "1") * rate / 100.00m) + calHIERABonus(rate, 1, Con_ID);

                        if (rate == 0)//若差階為0% 則算下一位顧問
                            continue;

                        //-----------------END---------------------------

                        //2. 計算輔導獎金------------START----------------
                        if (rate != 0.3m)//若差階不為0.3% 則算下一位顧問
                            continue;

                        decimal CounselBonus = (DepositeBonus(Con_ID, 0.05m, "2") * 0.05m / 100.00m) + calCounselBonus(0.05m, Con_ID);

                        //更新結果表
                        dr = dtResult.Select(condition).FirstOrDefault();
                        if (dr != null)
                        {
                            dr["CounselBonus"] = CounselBonus;
                        }
                        //-----------------END----------------------------

                        //3. 計算教育獎金---------START-------------------
                        //判斷自己直推下有幾人rate=0.3,這些0.3的是否直推也有0.3,直到找到下面所有人都沒0.3

                        decimal TutorBonus = calTutorBonus(Con_ID, 1);

                        //更新結果表
                        dr = dtResult.Select(condition).FirstOrDefault();
                        if (dr != null)
                        {
                            dr["TutorBonus"] = TutorBonus;
                        }

                        //--------------------END-------------------------
                    }

                    //4. 計算輔導獎金、教育獎金是否有發完,沒發完到股東上-----------
                    decimal total_CounselBonus = 0;
                    decimal total_TutorBonus = 0;

                    condition = "Con_ROLE='SHA'";  //判斷此次計算有無股東在內 有的話則要算輔導獎金,教育獎金總和 扣除大家拿的後加回股東

                    foreach (DataRow drs in dtConInfo.Select(condition))
                    {
                        dtConInfoById.Clear();
                        MEM_HIERA = 1;

                        decimal Deposit_Amount = Convert.ToDecimal(drs["Amount"]);
                        string Con_ID = drs["Con_ID"].ToString();
                        Report_Con_ID = Con_ID;

                        row = dtConInfoById.NewRow();
                        row["Con_ID"] = Con_ID;
                        row["Parent_Con_ID"] = drs["Parent_Con_ID"].ToString();
                        row["Con_ROLE"] = drs["Con_ROLE"].ToString();
                        row["Con_PATH"] = drs["Con_PATH"].ToString();
                        row["Amount"] = Deposit_Amount;
                        row["Con_HIERA"] = MEM_HIERA;
                        row["Con_Rate"] = 0;
                        dtConInfoById.Rows.Add(row);

                        getParentMemInfo(Con_ID); //取出的顧問ID丟進fun去撈出他組織下所有顧問

                        //計算輔導獎金總和
                        for (int i = 0; i < dtConInfoById.Rows.Count; i++)
                        {
                            total_CounselBonus = total_CounselBonus + (DepositeBonusForSum(dtConInfoById.Rows[i]["Con_ID"].ToString()) * 0.1m / 100);
                            total_TutorBonus = total_TutorBonus + (DepositeBonusForSum(dtConInfoById.Rows[i]["Con_ID"].ToString()) * 0.1m / 100);

                            //扣除已經拿的獎金,計算剩餘獎金
                            condition = "Con_ID='" + dtConInfoById.Rows[i]["Con_ID"].ToString() + "'";
                            DataRow[] drss = dtResult.Select(condition);

                            total_CounselBonus = total_CounselBonus - Convert.ToDecimal(drss[0]["CounselBonus"]);
                            total_TutorBonus = total_TutorBonus - Convert.ToDecimal(drss[0]["TutorBonus"]);
                        }
                        //將每筆計算結果寫入DB
                        InsertBonusReportDetail(Report_Con_ID, Report_Con_ID, 0, 0, Deposit_Amount, total_CounselBonus, "4", 0, 0);
                        InsertBonusReportDetail(Report_Con_ID, Report_Con_ID, 0, 0, Deposit_Amount, total_TutorBonus, "4", 0, 0);
                    }

                    scope.Complete();
                }

                return 1;
            }
            catch (Exception e)
            {
                log.writeLogToDB("", "Bonus/CalculateBonus", e.ToString());
                return 0;
            }
        }
        
        public void InsertReportMaster()
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                year = dtime.ToString("yyyy");//年
                mon = dtime.AddMonths(-1).ToString("MM");//上個月月份
                quarterly = "";

                if (mon == "01" || mon == "02" || mon == "03") //Q1
                    quarterly = "1";
                else if (mon == "04" || mon == "05" || mon == "06") //Q2
                    quarterly = "2";
                else if (mon == "07" || mon == "08" || mon == "09") //Q3
                    quarterly = "3";
                else if (mon == "10" || mon == "11" || mon == "12") //Q4
                    quarterly = "4";

                //如果是12月 年份就要往前推
                if (mon == "12")
                    year = dtime.AddYears(-1).ToString("yyyy");

                //查詢是否有該季資料
                string sqlcommandstring = " select count(*) from BonusReportMaster where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly ";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@Calculate_Year", year),
                    new SqlParameter("@Calculate_Quarterly", quarterly)
                    });
                int count = Convert.ToInt32(sqlcommand.ExecuteScalar());

                //判斷要寫入還是更新
                if (count == 0) //寫入
                {
                    //取得主報表ID+1
                    sqlcommandstring = @" select ISNULL(max(Report_ID),0) from BonusReportMaster ";
                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    string Report_ID = Convert.ToString(Convert.ToInt32(sqlcommand.ExecuteScalar()) + 1);

                    while (Report_ID.Length < 10)  //ID長度為10 不足長度補0
                    {
                        Report_ID = "0" + Report_ID;
                    }

                    sqlcommandstring = @" Insert BonusReportMaster values(@Report_ID, '1', ";
                    if (mon == "01" || mon == "04" || mon == "07" || mon == "10")
                        sqlcommandstring += " @MonthlyReport_ID,'','',@Calculate_Year, @Calculate_Quarterly) ";
                    else if (mon == "02" || mon == "05" || mon == "08" || mon == "11")
                        sqlcommandstring += " '', @MonthlyReport_ID,'', @Calculate_Year, @Calculate_Quarterly) ";
                    else if (mon == "03" || mon == "06" || mon == "09" || mon == "12")
                        sqlcommandstring += " '','',@MonthlyReport_ID, @Calculate_Year, @Calculate_Quarterly) ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Report_ID", Report_ID),
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Calculate_Year", year),
                        new SqlParameter("@Calculate_Quarterly", quarterly)
                    });
                    sqlcommand.ExecuteNonQuery();
                }
                else //更新
                {
                    sqlcommandstring = @" update BonusReportMaster ";

                    if (mon == "01" || mon == "04" || mon == "07" || mon == "10")
                        sqlcommandstring += " set MonthlyReport1_ID=@MonthlyReport_ID ";
                    else if (mon == "02" || mon == "05" || mon == "08" || mon == "11")
                        sqlcommandstring += " set MonthlyReport2_ID=@MonthlyReport_ID ";
                    else if (mon == "03" || mon == "06" || mon == "09" || mon == "12")
                        sqlcommandstring += " set MonthlyReport3_ID=@MonthlyReport_ID ";

                    sqlcommandstring += @"  where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly 
                                            and Report_Ver=(select MAX(Report_Ver) from BonusReportMaster 
				                                            where Calculate_Year=@Calculate_Year and Calculate_Quarterly=@Calculate_Quarterly )";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),
                        new SqlParameter("@Calculate_Year", year),
                        new SqlParameter("@Calculate_Quarterly", quarterly)
                    });
                    sqlcommand.ExecuteNonQuery();
                }
            }
        }
        public void getParentMemInfo(string Parent_ID)
        {

            string condition = "Parent_Con_ID='" + Parent_ID + "'";
            MEM_HIERA++;

            foreach (DataRow dr in dtConInfo.Select(condition))
            {
                decimal Deposit_Amount = Convert.ToDecimal(dr["Amount"]);
                decimal rate;
                string Con_ID = dr["Con_ID"].ToString();

                //判斷是否為自動,自動則以總存款計算rate, 若非自動再判斷有效日,若再有效日內則抓資料rate
                if (!Convert.ToBoolean(dr["IsAuto"]))
                {
                    if (dtime.AddDays(-dtime.Day).CompareTo(Convert.ToDateTime(dr["EffectiveStartDate"])) >= 0 && dtime.AddDays(-dtime.Day).CompareTo(Convert.ToDateTime(dr["EffectiveEndDate"])) <= 0)
                        rate = Convert.ToDecimal(dr["Con_Hiera"]);
                    else
                        rate = rateMapping(Deposit_Amount);
                }
                else
                {
                    rate = rateMapping(Deposit_Amount);
                }
                //取得rate後,更新ConInfo rate, 並比對職級變更表該顧問最新的一筆,若rate不同則寫一筆新的到ConHieraRecord
                updateHieraRecord(Con_ID, rate);

                row = dtConInfoById.NewRow();
                row["Con_ID"] = Con_ID;
                row["Parent_Con_ID"] = dr["Parent_Con_ID"].ToString();
                row["Con_ROLE"] = dr["Con_ROLE"].ToString();
                row["Con_PATH"] = dr["Con_PATH"].ToString();
                row["Amount"] = Deposit_Amount;
                row["Con_HIERA"] = MEM_HIERA;
                row["Con_Rate"] = rate;
                dtConInfoById.Rows.Add(row);
                getParentMemInfo(Con_ID);

            }
            MEM_HIERA--;
        }
        public void updateHieraRecord(string Con_ID, decimal rate)
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = @" select top 1 Con_Hiera from ConHieraRecord where Con_ID=@Con_ID order by Record_DATE desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID)
                    });
                decimal Con_Hiera = Convert.ToDecimal(sqlcommand.ExecuteScalar());

                sqlcommandstring = @" update ConInfo set Con_Hiera=@Con_Hiera, UPDATE_DATE=@date where Con_ID=@Con_ID ";
                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID),
                        new SqlParameter("@Con_Hiera", rate),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                sqlcommand.ExecuteNonQuery();

                if (Con_Hiera != rate)
                {
                    sqlcommandstring += @" Insert ConHieraRecord values(@Con_ID, @Con_Hiera, @date); ";

                    sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.Parameters.AddRange(new SqlParameter[] {
                        new SqlParameter("@Con_ID", Con_ID),
                        new SqlParameter("@Con_Hiera", rate),
                        new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                    sqlcommand.ExecuteNonQuery();
                }
            }
        }

        #region HIERABonus
        //計算第二層顧問的差階獎金
        public decimal calHIERABonus(decimal rate, int Con_HIERA, string Parent_ID)
        {

            string condition = "Con_Rate<" + rate + " and Con_HIERA='" + (Con_HIERA + 1) + "' and Parent_Con_ID='" + Parent_ID + "'";

            decimal result = 0;

            foreach (DataRow dr in dtConInfoById.Select(condition))
            {
                result = result + (DepositeBonus(dr["Con_ID"].ToString(), (rate - Convert.ToDecimal(dr["Con_Rate"])), "1") * ((rate - Convert.ToDecimal(dr["Con_Rate"])) / 100)) + calnextHIERABonus(rate, Convert.ToDecimal(dr["Con_Rate"]), Convert.ToInt16(dr["Con_HIERA"]), dr["Con_ID"].ToString());
            }
            return result;
        }
        //計算下一層顧問的差階獎金  傳入第一層及第二層利率,及要查詢的利率, 如果第二層為0則用上一層利率當被扣除的
        public decimal calnextHIERABonus(decimal Firstrate, decimal rate, int Con_HIERA, string Parent_ID)
        {

            string condition = "Con_Rate<" + Firstrate + " and Con_HIERA='" + (Con_HIERA + 1) + "' and Parent_Con_ID='" + Parent_ID + "'";

            decimal result = 0;
            decimal new_rate = rate;
            foreach (DataRow dr in dtConInfoById.Select(condition))
            {
                //if (Convert.ToDecimal(dr["Con_Rate"]) == Firstrate)
                //    continue;
                //if (rate == 0)
                //    new_rate = Convert.ToDecimal(dr["Con_Rate"]);

                //if (Convert.ToDecimal(dr["Con_Rate"]) == 0)
                //    result = result + (DepositeBonus(dr["Con_ID"].ToString(), (Firstrate - new_rate),"1") * ((Firstrate - new_rate) / 100)) + calnextHIERABonus(Firstrate, rate, Convert.ToInt16(dr["Con_HIERA"]), dr["Con_ID"].ToString());
                //else
                //    result = result + (DepositeBonus(dr["Con_ID"].ToString(), (Firstrate - new_rate), "1") * ((Firstrate - new_rate) / 100)) + calnextHIERABonus(Firstrate, Convert.ToDecimal(dr["Con_Rate"]), Convert.ToInt16(dr["Con_HIERA"]), dr["Con_ID"].ToString());
                if (Convert.ToDecimal(dr["Con_Rate"]) == Firstrate)
                    continue;
                if (Convert.ToDecimal(dr["Con_Rate"]) > rate)
                    rate = Convert.ToDecimal(dr["Con_Rate"]);
                if (rate == 0)
                    new_rate = Convert.ToDecimal(dr["Con_Rate"]);

                if (Convert.ToDecimal(dr["Con_Rate"]) == 0)
                    result = result + (DepositeBonus(dr["Con_ID"].ToString(), (Firstrate - new_rate), "1") * ((Firstrate - new_rate) / 100)) + calnextHIERABonus(Firstrate, rate, Convert.ToInt16(dr["Con_HIERA"]), dr["Con_ID"].ToString());
                else
                    result = result + (DepositeBonus(dr["Con_ID"].ToString(), (Firstrate - rate), "1") * ((Firstrate - rate) / 100)) + calnextHIERABonus(Firstrate, rate, Convert.ToInt16(dr["Con_HIERA"]), dr["Con_ID"].ToString());

            }
            return result;
        }

        #endregion

        #region CounselBonus
        //CounselRate為輔導獎金階層的趴數 第一層0.05% 第二層0.03% 第三層0.02%
        //找到自己存款及自己推薦顧問的存款 計算第一層(若rate等於0.3%跳過當下一層)
        //第二算法一樣, 第三層碰到下一個rate=0.3%則結束計算
        public decimal calCounselBonus(decimal CounselRate, string Parent_ID)
        {
            string condition = "Parent_Con_ID='" + Parent_ID + "'";

            decimal result = 0;
            foreach (DataRow dr in dtConInfoById.Select(condition))
            {

                if (Convert.ToDecimal(dr["Con_Rate"]) == 0.3m)
                {
                    if (CounselRate == 0.05m)
                        result = result + (DepositeBonus(dr["Con_ID"].ToString(), 0.03m, "2") * (0.03m / 100)) + calCounselBonus(0.03m, dr["Con_ID"].ToString());
                    if (CounselRate == 0.03m)
                        result = result + (DepositeBonus(dr["Con_ID"].ToString(), 0.02m, "2") * (0.02m / 100)) + calCounselBonus(0.02m, dr["Con_ID"].ToString());
                }
                else
                {
                    result = result + (DepositeBonus(dr["Con_ID"].ToString(), CounselRate, "2") * (CounselRate / 100)) + calCounselBonus(CounselRate, dr["Con_ID"].ToString());
                }
            }
            return result;
        }
        #endregion

        #region TutorBonus
        public decimal calTutorBonus(string Parent_ID, int Con_HIERA)
        {
            string condition = " Con_HIERA='" + (Con_HIERA + 1) + "' and Parent_Con_ID='" + Parent_ID + "'";

            decimal result = 0;

            decimal sysHiera = getTutorSysHiera(Parent_ID);

            if (sysHiera != 0)
            {
                result = (DepositeBonus(Parent_ID, sysHiera, "3") * (sysHiera / 100));
                foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
                {
                    result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), sysHiera);
                }
            }
            return result;
        }
        public decimal calnextTutorBonus(string Parent_ID, int Con_HIERA, decimal rate)
        {
            string condition = " Con_HIERA='" + (Con_HIERA + 1) + "' and Parent_Con_ID='" + Parent_ID + "'";

            decimal result = 0;

            decimal sysHiera = getTutorSysHiera(Parent_ID);

            //if (sysHiera == 0.1m)//0.1%, 表示該顧問底下接拿不到差階獎金,不用在往下
            //{
            //    return result;
            //}
            //else if (sysHiera == 0.08m)//0.08%  
            //{
            //    if (rate <= 0.08m)//表示該顧問底下接拿不到差階獎金,不用在往下
            //    {
            //        return result;
            //    }
            //    else
            //    {
            //        result = (DepositeBonus(Parent_ID, (rate - 0.08m), "3") * ((rate - 0.08m) / 100));
            //        foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
            //        {
            //            result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), (rate - 0.08m));
            //        }
            //    }
            //}
            //else if (sysHiera == 0.05m)//0.05%
            //{
            //    if (rate <= 0.05m)//表示該顧問底下接拿不到差階獎金,不用在往下
            //    {
            //        return result;
            //    }
            //    else
            //    {
            //        result = (DepositeBonus(Parent_ID, (rate - 0.05m), "3") * ((rate - 0.05m) / 100));
            //        foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
            //        {
            //            result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), (rate - 0.05m));
            //        }
            //    }
            //}
            //else
            //{
            //    return (DepositeBonus(Parent_ID, rate, "3") * (rate / 100));
            //}
            if (sysHiera == 0.1m)//0.1%, 表示該顧問底下接拿不到差階獎金,不用在往下
            {
                return result;
            }
            else if (sysHiera == 0.08m)//0.08%  
            {
                if (rate <= 0.08m)//表示該顧問底下接拿不到差階獎金,不用在往下
                {
                    return result;
                }
                else
                {
                    result = (DepositeBonus(Parent_ID, (rate - 0.08m), "3") * ((rate - 0.08m) / 100));
                    foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
                    {
                        result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), (rate - 0.08m));
                    }
                }
            }
            else if (sysHiera == 0.05m)//0.05%
            {
                if (rate <= 0.05m)//表示該顧問底下接拿不到差階獎金,不用在往下
                {
                    return result;
                }
                else
                {
                    result = (DepositeBonus(Parent_ID, (rate - 0.05m), "3") * ((rate - 0.05m) / 100));
                    foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
                    {
                        result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), (rate - 0.05m));
                    }
                }
            }
            else
            {
                //return (DepositeBonus(Parent_ID, rate, "3") * (rate / 100));
                result = (DepositeBonus(Parent_ID, rate, "3") * (rate / 100));
                foreach (DataRow dr in dtConInfoById.Select(condition))//取出下層的顧問
                {
                    result = result + calnextTutorBonus(dr["Con_ID"].ToString(), Convert.ToInt16(dr["Con_HIERA"]), (rate));
                }
            }
            return result;
        }

        public decimal getTutorSysHiera(string Con_ID)
        {
            string condition = "Con_ID='" + Con_ID + "'";
            DataRow[] drs = dtConInfo.Select(condition);
            if (drs.Length > 0)
                return Convert.ToDecimal(drs[0]["Con_SysHiera"]);

            return 0;
        }
        #endregion

        #region 共用
        public List<monthList> getMonLists()
        {
            List<monthList> monLists = new List<monthList>();
            string _year;
            string _mon;

            DataTable dt = new DataTable();
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();

                string sqlcommandstring = @"   select top 1 MonthlyReport1_ID,MonthlyReport2_ID,MonthlyReport3_ID,Calculate_Year,Calculate_Quarterly  
                                                  from BonusReportMaster order by Calculate_Year desc, Calculate_Quarterly desc, Report_Ver desc ";

                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);

                SqlDataReader reader = sqlcommand.ExecuteReader();
                if (reader.HasRows)
                {

                }
                else
                {
                    //如果都沒有就從前個月開始
                    _year = dtime.ToString("yyyy");//年
                    _mon = dtime.AddMonths(-1).ToString("MM");//上個月月份
                                                             //如果是12月 年份就要往前推
                    if (_mon == "12")
                        _year = dtime.AddYears(-1).ToString("yyyy");

                    monLists.Add(new monthList { year = _year, month = _mon });
                }


            }
            return monLists;
        }
        public void InsertBonusReportDetail(string Cli_ID, string Con_ID, decimal Deposit_Amount, decimal Withdrawal_Amount,
            decimal Total_Amount, decimal Bonus, string Bonus_Type, decimal BonusType_Rate, decimal DepositType_Rate)
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = @" INSERT BonusReportDetail values(@MonthlyReport_ID, @Report_Con_ID, @Cli_ID, @Con_ID, @Deposit_Amount, @Withdrawal_Amount,
                                             @Total_Amount, @Bonus, @Bonus_Type, @BonusType_Rate, @DepositType_Rate, @Calculate_Year, @Calculate_Month, @date)";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),new SqlParameter("@Report_Con_ID", Report_Con_ID),
                    new SqlParameter("@Cli_ID", Cli_ID),new SqlParameter("@Con_ID", Con_ID), new SqlParameter("@Deposit_Amount", Deposit_Amount),
                    new SqlParameter("@Withdrawal_Amount", Withdrawal_Amount), new SqlParameter("@Total_Amount", Total_Amount),
                    new SqlParameter("@Bonus", Bonus), new SqlParameter("@Bonus_Type", Bonus_Type),new SqlParameter("@BonusType_Rate", BonusType_Rate),
                    new SqlParameter("@DepositType_Rate", DepositType_Rate), new SqlParameter("@Calculate_Year", year),new SqlParameter("@Calculate_Month", mon),
                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                sqlcommand.ExecuteNonQuery();
            }
        }
        public void InsertBonusReportDetailForDistri(string Cli_ID, string Distri_Con_ID, decimal Deposit_Amount, decimal Withdrawal_Amount,
    decimal Total_Amount, decimal Bonus, string Bonus_Type, decimal BonusType_Rate, decimal DepositType_Rate)
        {
            string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (SqlConnection sqlconnection = new SqlConnection(conn))
            {
                sqlconnection.Open();
                string sqlcommandstring = @" INSERT BonusReportDetail values(@MonthlyReport_ID, @Report_Con_ID, @Cli_ID, @Con_ID, @Deposit_Amount, @Withdrawal_Amount,
                                             @Total_Amount, @Bonus, @Bonus_Type, @BonusType_Rate, @DepositType_Rate, @Calculate_Year, @Calculate_Month, @date)";
                SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                sqlcommand.Parameters.AddRange(new SqlParameter[] {
                    new SqlParameter("@MonthlyReport_ID", MonthlyReport_ID),new SqlParameter("@Report_Con_ID", Distri_Con_ID),
                    new SqlParameter("@Cli_ID", Cli_ID),new SqlParameter("@Con_ID", Report_Con_ID), new SqlParameter("@Deposit_Amount", Deposit_Amount),
                    new SqlParameter("@Withdrawal_Amount", Withdrawal_Amount), new SqlParameter("@Total_Amount", Total_Amount),
                    new SqlParameter("@Bonus", Bonus), new SqlParameter("@Bonus_Type", Bonus_Type),new SqlParameter("@BonusType_Rate", BonusType_Rate),
                    new SqlParameter("@DepositType_Rate", DepositType_Rate), new SqlParameter("@Calculate_Year", year),new SqlParameter("@Calculate_Month", mon),
                    new SqlParameter("@date", convertTime.UStoTW(DateTime.Now))
                    });
                sqlcommand.ExecuteNonQuery();
            }
        }
        public decimal DepositeBonus(string Parent_ID, decimal rate, string Bonus_Type) 
        {
            string condition = "Con_ID='" + Parent_ID + "'";
            decimal result = 0;

            foreach (DataRow dr in dtDepositDetail.Select(condition))
            {
                string Cli_ID = Convert.ToString(dr["Cli_ID"]);
                string Con_ID = Convert.ToString(dr["Con_ID"]);
                decimal Deposit_Amount = Convert.ToDecimal(dr["DepositAmount"]);
                decimal Withdrawal_Amount = Convert.ToDecimal(dr["WithdrawalAmount"]);
                decimal Amount = Convert.ToDecimal(dr["Amount"]);
                int type = Convert.ToInt16(dr["Deposit_Type"]);
                decimal typeRate = Convert.ToDecimal(dr["Type_RATE"]);

                //decimal typeRate = 1;
                //if (type == 1) { typeRate = 0.7m; }
                //if (type == 3) { typeRate = 1.3m; }

                //判斷此筆要計算的存款之客戶是否有被分配
                condition = "Con_ID='" + Con_ID + "' and Parent_Con_ID='" + Report_Con_ID + "'";
                DataRow[] drs = dtBonusDistribution.Select(condition);
                if (drs.Length > 0)
                {
                    string Distri_Con_ID = Convert.ToString(drs[0]["Distri_Con_ID"]);
                    decimal Distri_Percentage = Convert.ToDecimal(drs[0]["Distri_Percentage"]);
                    result = result + (Amount * typeRate);

                    //將每筆計算結果寫入DB
                    InsertBonusReportDetail(Cli_ID, Con_ID, Deposit_Amount, Withdrawal_Amount, Amount, (Amount * typeRate * ((100 - Distri_Percentage) / 100.00m)*(rate / 100.00m)), "5", rate, typeRate);
                    InsertBonusReportDetailForDistri(Cli_ID, Distri_Con_ID, Deposit_Amount, Withdrawal_Amount, Amount, (Amount * typeRate * (Distri_Percentage / 100.00m) * (rate / 100.00m)), "6", rate, typeRate);
                }
                else
                {
                    result = result + (Amount * typeRate);

                    //將每筆計算結果寫入DB
                    InsertBonusReportDetail(Cli_ID, Con_ID, Deposit_Amount, Withdrawal_Amount, Amount, (Amount * typeRate * (rate / 100.00m)), Bonus_Type, rate, typeRate);
                }
            }
            return result;
        }
        public decimal DepositeBonusForSum(string Parent_ID)
        {
            string condition = "Con_ID='" + Parent_ID + "'";
            decimal result = 0;

            foreach (DataRow dr in dtDepositDetail.Select(condition))
            {
                decimal Amount = Convert.ToDecimal(dr["Amount"]);
                int type = Convert.ToInt16(dr["Deposit_Type"]);
                decimal typeRate = Convert.ToDecimal(dr["Type_RATE"]);
                //decimal typeRate = 1;
                //if (type == 1) { typeRate = 0.7m; }
                //if (type == 3) { typeRate = 1.3m; }

                result = result + (Amount * typeRate);

            }
            return result;
        }
        public decimal rateMapping(decimal amount)
        {
            if (amount < 1000000)
                return 0;
            else if (amount < 2000000)
                return 0.1m;
            else if (amount < 3000000)
                return 0.15m;
            else if (amount < 4000000)
                return 0.2m;
            else if (amount < 5000000)
                return 0.25m;
            else
                return 0.3m;
        }
        #endregion
    }
    public class monthList
    {
        public string year { get; set; }
        public string month { get; set; }
    }
}