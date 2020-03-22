using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace BW.Helpers
{
    public class SendSMS
    {
        private string sendSMSUrl = ConfigurationManager.AppSettings["SMSUrl"].ToString();
        private string getCreditUrl = "http://api.every8d.com/API21/HTTP/getCredit.ashx";
        private string processMsg = "";
        private string batchID = "";
        private double credit = 0;

        private string userID = ConfigurationManager.AppSettings["SMSUserID"].ToString();
        private string password = ConfigurationManager.AppSettings["SMSPW"].ToString();

        public SendSMS()
        {
        }

        /// <summary>
        /// 傳送簡訊
        /// </summary>
        /// <param name="subject">簡訊主旨，主旨不會隨著簡訊內容發送出去。用以註記本次發送之用途。可傳入空字串。</param>
        /// <param name="content">簡訊發送內容</param>
        /// <param name="mobile">接收人之手機號碼。格式為: +886912345678或09123456789。多筆接收人時，請以半形逗點隔開( , )，如0912345678,0922333444。</param>
        /// <param name="sendTime">簡訊預定發送時間。-立即發送：請傳入空字串。-預約發送：請傳入預計發送時間，若傳送時間小於系統接單時間，將不予傳送。格式為YYYYMMDDhhmnss；例如:預約2009/01/31 15:30:00發送，則傳入20090131153000。若傳遞時間已逾現在之時間，將立即發送。</param>
        /// <returns>true:傳送成功；false:傳送失敗</returns>
        public bool sendSMS(string subject, string content, string mobile, string sendTime)
        {
            bool success = false;
            try
            {
                if (!string.IsNullOrEmpty(sendTime))
                {
                    try
                    {
                        //檢查傳送時間格式是否正確
                        DateTime checkDt = DateTime.ParseExact(sendTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                        if (!sendTime.Equals(checkDt.ToString("yyyyMMddHHmmss")))
                        {
                            this.processMsg = "傳送時間格式錯誤";
                            return false;
                        }
                    }
                    catch
                    {
                        this.processMsg = "傳送時間格式錯誤";
                        return false;
                    }
                }
                StringBuilder postDataSb = new StringBuilder();
                postDataSb.Append("UID=").Append(userID);
                postDataSb.Append("&PWD=").Append(password);
                postDataSb.Append("&SB=").Append(subject);
                postDataSb.Append("&MSG=").Append(content);
                postDataSb.Append("&DEST=").Append(mobile);
                postDataSb.Append("&ST=").Append(sendTime);

                string resultString = this.httpPost(this.sendSMSUrl, postDataSb.ToString());
                if (!resultString.StartsWith("-"))
                {
                    /* 
					 * 傳送成功 回傳字串內容格式為：CREDIT,SENDED,COST,UNSEND,BATCH_ID，各值中間以逗號分隔。
					 * CREDIT：發送後剩餘點數。負值代表發送失敗，系統無法處理該命令
					 * SENDED：發送通數。
					 * COST：本次發送扣除點數
					 * UNSEND：無額度時發送的通數，當該值大於0而剩餘點數等於0時表示有部份的簡訊因無額度而無法被發送。
					 * BATCH_ID：批次識別代碼。為一唯一識別碼，可藉由本識別碼查詢發送狀態。格式範例：220478cc-8506-49b2-93b7-2505f651c12e
					 */
                    string[] split = resultString.Split(',');
                    this.credit = Convert.ToDouble(split[0]);
                    this.batchID = split[4];
                    success = true;
                }
                else
                {
                    //傳送失敗
                    this.processMsg = resultString;
                }

            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return success;
        }

        /// <summary>
        /// 取得帳號餘額
        /// </summary>
        /// <returns>true:取得成功；false:取得失敗</returns>
        public bool getCredit(string userID, string password)
        {
            bool success = false;
            try
            {
                StringBuilder postDataSb = new StringBuilder();
                postDataSb.Append("UID=").Append(userID);
                postDataSb.Append("&PWD=").Append(password);

                string resultString = this.httpPost(this.getCreditUrl, postDataSb.ToString());
                if (!resultString.StartsWith("-"))
                {
                    this.credit = Convert.ToDouble(resultString);
                    success = true;
                }
                else
                {
                    this.processMsg = resultString;
                }
            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return success;
        }

        private string httpPost(string url, string postData)
        {
            string result = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(postData);
                request.ContentLength = bs.Length;
                request.GetRequestStream().Write(bs, 0, bs.Length);
                //取得 WebResponse 的物件 然後把回傳的資料讀出
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                this.processMsg = ex.ToString();
            }
            return result;
        }

        public string ProcessMsg
        {
            get
            {
                return this.processMsg;
            }
        }

        public string BatchID
        {
            get
            {
                return this.batchID;
            }
        }

        public double Credit
        {
            get
            {
                return this.credit;
            }
        }
    }
}