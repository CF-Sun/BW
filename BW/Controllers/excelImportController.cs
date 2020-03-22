using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]

    public class excelImportController : Controller
    {
        [HttpPost]
        public bool clear(int type)
        {
            try
            {
                string conn = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection sqlconnection = new SqlConnection(conn))
                {
                    sqlconnection.Open();
                    string sqlcommandstring = "";
                    if (type == 0)
                    {
                        sqlcommandstring = @" delete ConInfo where Con_ID !='000'";
                    }
                    else
                    {
                        sqlcommandstring = @" delete CliInfo; delete DepositDetail";
                    }

                    SqlCommand sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                    sqlcommand.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            string type = Request["uploadType"];
            if (Request.Files["file"].ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;

                if (extension == ".xls" || extension == ".xlsx")
                {
                    if (System.IO.File.Exists(fileLocation)) // 驗證檔案是否存在
                    {
                        System.IO.File.Delete(fileLocation);
                    }

                    Request.Files["file"].SaveAs(fileLocation); // 存放檔案到伺服器上
                }

                if (!insertData(fileLocation, type))
                    return Json(false);

            }
            //return this.RedirectToAction("Index");
            return Json(true);
        }
        public bool insertData(string fileLocation, string importType)
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
                        string Con_ID = "";
                        string Con_ChiNAME = "";
                        string Con_EngNAME = "";
                        string Parent_Con_ID = "";
                        string Role = "CON";
                        bool IsTeach = false;

                        string Cli_ID = "";
                        int InvestType = 2;
                        DateTime InvestDate;
                        string new_InvestDate = "";
                        double Amount = 0;
                        //----------------------------------

                        if (sheet.GetRow(row) != null) // 驗證是不是空白列
                        {
                            //for (int c = 0; c <= sheet.GetRow(row).LastCellNum; c++) // 使用For 走訪資料欄
                            //{
                            //}
                            if (importType == "0")
                            {
                                if (sheet.GetRow(row).GetCell(0) != null)
                                    Con_ID = sheet.GetRow(row).GetCell(0).StringCellValue; // 字串
                                if (Con_ID == "") //空白行跳過
                                    continue;

                                if (sheet.GetRow(row).GetCell(1) != null)
                                    Con_ChiNAME = sheet.GetRow(row).GetCell(1).StringCellValue;
                                if (sheet.GetRow(row).GetCell(2) != null)
                                    Con_EngNAME = sheet.GetRow(row).GetCell(2).StringCellValue;
                                if (sheet.GetRow(row).GetCell(3) != null)
                                    Parent_Con_ID = sheet.GetRow(row).GetCell(3).StringCellValue;
                                //double Con_ChiNAME = sheet.GetRow(row).GetCell(1).NumericCellValue; // 布林
                                if (Parent_Con_ID == "股東")
                                {
                                    Parent_Con_ID = "000";
                                    Role = "SHA";
                                }
                                sqlcommandstring = @" delete ConInfo where Con_ID=N'" + Con_ID + @"';
                                                        delete ConInfoDetail where Con_ID=N'" + Con_ID + @"';
                                                    insert into ConInfoDetail(Con_ID,Con_ChiNAME_Last,Con_ChiNAME_First) values(N'" + Con_ID + @"',N'" + Con_ChiNAME + @"',' ');
                                                    insert into ConInfo(Con_ID,Parent_Con_ID,Con_ROLE,Con_PATH,IsAuto,CREATE_DATE,UPDATE_DATE)
                                                    select N'" + Con_ID + "',N'" + Parent_Con_ID + "','" + Role + "',Con_PATH+'/" + Con_ID + @"',1,GETDATE(),GETDATE() 
                                                    from ConInfo where Con_ID=N'" + Parent_Con_ID + "' ";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.ExecuteNonQuery();
                            }
                            else if (importType == "1")
                            {
                                if (sheet.GetRow(row).GetCell(0) != null)
                                    Cli_ID = sheet.GetRow(row).GetCell(0).StringCellValue; // 字串
                                if (Cli_ID == "") //空白行跳過
                                    continue;

                                if (sheet.GetRow(row).GetCell(1) != null)
                                    Con_ChiNAME = sheet.GetRow(row).GetCell(1).StringCellValue;
                                if (sheet.GetRow(row).GetCell(2) != null)
                                    Con_EngNAME = sheet.GetRow(row).GetCell(2).StringCellValue;
                                if (sheet.GetRow(row).GetCell(3) != null)
                                    Parent_Con_ID = sheet.GetRow(row).GetCell(3).StringCellValue;
                                if (sheet.GetRow(row).GetCell(4) != null)
                                    InvestType = (int)sheet.GetRow(row).GetCell(4).NumericCellValue;
                                if (sheet.GetRow(row).GetCell(5) != null)
                                {
                                    InvestDate = sheet.GetRow(row).GetCell(5).DateCellValue;
                                    new_InvestDate = InvestDate.ToString("yyyy/MM/dd");
                                    if (new_InvestDate == "0001/01/01")
                                        new_InvestDate = null;
                                }
                                else
                                {
                                    InvestDate = DateTime.Now;
                                    new_InvestDate = null;
                                }
                                if (sheet.GetRow(row).GetCell(6) != null)
                                    Amount = sheet.GetRow(row).GetCell(6).NumericCellValue;

                                sqlcommandstring = @"SELECT ISNULL(max(Deposit_ID),0) as Deposit_ID FROM DepositList";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                string Deposit_ID = Convert.ToString(Convert.ToInt32(Convert.ToString(sqlcommand.ExecuteScalar()).Substring(1)) + 1);

                                while (Deposit_ID.Length < 9)
                                    Deposit_ID = "0" + Deposit_ID;
                                Deposit_ID = "D" + Deposit_ID;

                                sqlcommandstring = @" delete CliInfo where Cli_ID='" + Cli_ID + @"';
                                                    delete CliInfoDetail where Cli_ID='" + Cli_ID + @"';                                                  
                                                insert into CliInfo (Cli_ID,Con_ID,CREATE_DATE,UPDATE_DATE)
                                                values('" + Cli_ID + "','" + Parent_Con_ID + @"',GETDATE(),GETDATE()) ;
                                                insert into CliInfoDetail (Cli_ID,Con_ID,Cli_ChiNAME_Last)values('" + Cli_ID + "','" + Parent_Con_ID + @"',N'" + Con_ChiNAME + @"')
                                                insert into DepositList(Deposit_ID,Cli_ID,Con_ID,Deposit_Amount,Deposit_Type,Deposit_DATE,Arrival_DATE,Status,CREATE_DATE)
                                                values('" + Deposit_ID + "','" + Cli_ID + "','" + Parent_Con_ID + "','" + Amount + "','" + InvestType + "','" + new_InvestDate + "','" + new_InvestDate + "','2',GETDATE())";
                                sqlcommand = new SqlCommand(sqlcommandstring, sqlconnection);
                                sqlcommand.ExecuteNonQuery();

                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}