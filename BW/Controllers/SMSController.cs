using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]

    public class SMSController : Controller
    {
        Log log = new Log();
        ConvertTime convertTime = new ConvertTime();
        SendSMS sendSMS = new SendSMS();

        [HttpPost]
        public string SendSMSCode(string site, string phone, string LoginACCOUNT)
        {
            try
            {
                string code = createCode();
                string content = @"親愛的NewSafety夥伴您好: 您的簡訊驗證碼為" + code + ",請至網頁輸入驗證，謝謝!";
                
                string cellPhone = "";

                if (phone.Substring(0, 1) == "0")
                    cellPhone = site + phone.Substring(1);
                else
                    cellPhone = site + phone;

                if (!sendSMS.sendSMS("簡訊驗證碼", content, cellPhone, ""))
                    return "";

                log.writeLogToDB(LoginACCOUNT, "SMS/SendSMSCode", "報客戶,發送簡訊驗證碼" + code + "至"+ cellPhone);

                return code;
            }
            catch (Exception e)
            {
                log.writeLogToDB(LoginACCOUNT, "SMS/SendSMSCode", e.ToString());
                return "";
            }
        }

        public string createCode()
        {
            string[] list = new string[6];
            string result = "";
            //隨機取六碼
            //string reg = "123456789abcdefghijklmnpqrstuvwxyz";
            string reg = "1234567890";
            StringBuilder code = new StringBuilder();
            Random rand = new Random();
            int index;
            for (int i = 0; i < 6; i++)
            {
                index = rand.Next(0, reg.Length);
                code.Append(reg[index]);
                list[i] = reg[index].ToString();
            }

            foreach (string i in list)
            {
                result = result + i;
            }
            return result;
        }
    }
}