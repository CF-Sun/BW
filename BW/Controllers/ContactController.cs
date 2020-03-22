using BW.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    public class ContactController : Controller
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

        public JsonResult SendContact(string Code, string Name, string Email, string Mobile, string Content, string Gender, string Identity, string ContactType)
        {
            try
            {
                string ans = TempData["code"].ToString();
                if (Code != ans)
                {
                    return Json(false);
                }
                else
                {
                    SendMail send = new SendMail();
                    string content = "";
                    switch (Gender)
                    {
                        case "0":
                            content += "客戶姓名：" + Name + "先生" + Environment.NewLine;
                            break;
                        case "1":
                            content += "客戶姓名：" + Name + "女士" + Environment.NewLine;
                            break;
                        default:
                            content += "客戶姓名：" + Environment.NewLine;
                            break;
                    }
                    content += "Email：" + Email + Environment.NewLine;
                    content += "手機：" + Mobile + Environment.NewLine;
                    switch (Identity)
                    {
                        case "1":
                            content += "身分：有客戶編號" + Environment.NewLine;
                            break;
                        case "2":
                            content += "身分：無客戶編號" + Environment.NewLine;
                            break;
                        default:
                            content += "身分：" + Environment.NewLine;
                            break;
                    }
                    switch (ContactType)
                    {
                        case "1":
                            content += "諮詢類別：申購文件相關問題" + Environment.NewLine;
                            break;
                        case "2":
                            content += "諮詢類別：申購流程相關問題" + Environment.NewLine;
                            break;
                        case "3":
                            content += "諮詢類別：資金進出問題" + Environment.NewLine;
                            break;
                        case "4":
                            content += "諮詢類別：其他問題" + Environment.NewLine;
                            break;
                        default:
                            content += "諮詢類別：" + Environment.NewLine;
                            break;
                    }
                    content += "諮詢內容：" + Content;

                    if (!send.Send(ConfigurationManager.AppSettings["ServiceMail"].ToString(), "聯絡我們", content))
                        return Json(1);
                }
                return Json(2);
            }
            catch(Exception ex)
            {
                log.writeLogToDB("", "Contact/SendContact", ex.ToString());
                return Json(1);
            }

        }
    }
}