using BW.Helpers;
using EO.Pdf;
using SautinSoft.Document;
using SautinSoft.Document.Drawing;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class ExportController : Controller
    {
        ConvertTime convertTime = new ConvertTime();

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult BonusReport2Pdf(FormCollection post)
        {
            //bool result = License.IsValidLicense("IRONPDF-10049321C2-178445-FACB89-826487FB46-8B01AAD7-UEx294DA74D37048D8-RMPCHINA.IRO190814.3891.33136.PRO.1DEV.1YR.SUPPORTED.UNTIL.14.AUG.2020");

            string ReportMasterID = post["ReportMasterID"];
            string Report_Con_ID = post["Report_Con_ID"];
            string ReportType = post["ReportType"];

            string URL = ConfigurationManager.AppSettings["ReportURL"];

            if (ReportType == "Individual")
                URL += "BackEnd/IndividualBonusReport?ReportMasterID=" + ReportMasterID + "&Report_Con_ID=" + Report_Con_ID;
            else if (ReportType == "Quarterly")
                URL += "BackEnd/QuarterlyBonusReport?ReportMasterID=" + ReportMasterID;
            else
                return Json("匯出發生錯誤");

            HtmlToPdfOptions pdfOptions = new HtmlToPdfOptions();
            pdfOptions.HeaderHtmlFormat = "<div style='width:100%;text-align:center'><h1>佣金報表</h1></div>";
            pdfOptions.FooterHtmlFormat = "<div style='width:100%;text-align:center'>{page_number}</div>";
            pdfOptions.PageSize = new SizeF(PdfPageSizes.A4.Height, PdfPageSizes.A4.Width);
            //pdfOptions.OutputArea = new RectangleF(0.1f, 1f, 9f, 13f);

            MemoryStream fileStream = new MemoryStream();
            HtmlToPdf.ConvertUrl(URL, fileStream, pdfOptions);

            string pdfName = post["Report_Con_ID"] == "" ? "佣金報表" : post["Report_Con_ID"];
            return File(fileStream.ToArray(), "application/pdf", pdfName + ".Pdf");


            //PdfPrintOptions ppo = new PdfPrintOptions();
            //ppo.Header = new HtmlHeaderFooter()
            //{
            //    HtmlFragment = "<div style='width:100%;text-align:center'><h1>佣金報表</h1></div>"
            //};
            //ppo.Footer = new HtmlHeaderFooter()
            //{
            //    HtmlFragment = "<i>{page}<i>"
            //};
            //ppo.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
            //ppo.FirstPageNumber = 1;
            //ppo.PaperOrientation = PdfPrintOptions.PdfPaperOrientation.Portrait;
            //ppo.MarginTop = 20;
            //ppo.MarginBottom = 20;
            //ppo.MarginLeft = 10;
            //ppo.MarginRight = 10;
            ////ppo.CustomCssUrl = new Uri(HttpContext.Server.MapPath(@"~\Content/css/style.css"));
            //ppo.CssMediaType = PdfPrintOptions.PdfCssMediaType.Screen;
            //ppo.Zoom = 100;
            //var PDF = HtmlToPdf.StaticRenderUrlAsPdf(new Uri(URL), ppo);
        
            //string pdfName = post["Report_Con_ID"] == "" ? "佣金報表" : post["Report_Con_ID"];
            //return File(PDF.BinaryData, "application/pdf", pdfName + ".Pdf");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AgreeMent2PDF(string PruchaseType,string txt2,string txt3,string txt3_1,string txt4,string txt5,string txt6,string txt7,string txt8,
             string txt9,string txt10,string txt11, string txt12,string txt13,string txt14, string txt15, string txt16,string jobRadio,string txtjobRadio_1,
             string txtjobRadio_2,string txtjobRadio_3,string txtjobRadio_4,string txtjobRadio_5, string txtjobRadio_6,string txtjobRadio_7, string txt17,
             string txt18, string txt19, string txt20, string txt21, string txt22,string txt23, string txt24,string purTypeRadio,string permitRadio,
             string txt25, string txt26, string txt27, string txt28, string txt29, string txt30, string txt31, string txt32, string txt25_1,string txt26_1,
             string txt27_1,string txt28_1, string txt29_1, string txt30_1,string txt31_1, string txt32_1,string AmericRadio,string txt33,string txt34,
             string txt35,string txt36, string txt37, string txt38, string txt39,string txt40, string txt41, string txt42, string txt38_1,string txt39_1,
             string txt40_1, string txt41_1,string txt42_1, string FATCARadio,string txt43,string txt44,string txt45, string txt46,string txt47,string txt48,
             string txt49, string txt50,string txt51,string txt52,string txt53,string txt54,string txt55, string txt56, string txt57,string txt58,string txt59,
             string txt60, string txt61,string txt62, string txt63,string txt64,string txt65, string txt66, string txt67)
        {
            try
            {
                ControllerContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                // Path to a loadable document.
                string loadPath = Server.MapPath("~/Content/files/AgreeMent/AgreeTemplate/Subscription Agreement.pdf");

                //key
                DocumentCore.Serial = "40028541286";

                // Load a document intoDocumentCore.
                DocumentCore dc = DocumentCore.Load(loadPath);
                #region txt2
                string txt1_1 = txt2;
                string txt1_2 = "";

                Regex regex = new Regex(@"Rreplace1_________________________________________11R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt1_1);

                if (txt2.Length > 50)
                {
                    txt1_1 = txt2.Substring(0, 50);
                    txt1_2 = txt2.Substring(50);
                }
                regex = new Regex(@"Rreplace1_1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt1_1);

                regex = new Regex(@"Rreplace1_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt1_2);
                #endregion

                #region txt3
                string newtxt3_1 = txt3;
                string txt3_2 = "";
                string txt3_3 = "";
                string txt3_4 = "";
                if (txt3.Length > 150)
                {
                    newtxt3_1 = txt3.Substring(0, 50);
                    txt3_2 = txt3.Substring(50,50);
                    txt3_3 = txt3.Substring(100,50);
                    txt3_4 = txt3.Substring(150);
                }else if (txt3.Length > 100)
                {
                    newtxt3_1 = txt3.Substring(0, 50);
                    txt3_2 = txt3.Substring(50, 50);
                    txt3_3 = txt3.Substring(100);
                }
                else if (txt3.Length > 50)
                {
                    newtxt3_1 = txt3.Substring(0, 50);
                    txt3_2 = txt3.Substring(50);
                }
                regex = new Regex(@"Rreplace2_1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(newtxt3_1);
                regex = new Regex(@"Rreplace2_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_2);
                regex = new Regex(@"Rreplace2_3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_3);
                regex = new Regex(@"Rreplace2_4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_4);
                #endregion

                #region txt3
                string txt3_1_1 = "";
                string txt3_1_2 = "";
                string txt3_1_3 = "";
                string txt3_1_4 = "";

                if (!String.IsNullOrEmpty(txt3_1))
                {
                    txt3_1_1 = txt3_1;
                    txt3_1_2 = "";
                    txt3_1_3 = "";
                    txt3_1_4 = "";
                    if (txt3_1.Length > 150)
                    {
                        txt3_1_1 = txt3_1.Substring(0, 50);
                        txt3_1_2 = txt3_1.Substring(50, 50);
                        txt3_1_3 = txt3_1.Substring(100, 50);
                        txt3_1_4 = txt3_1.Substring(150);
                    }
                    else if (txt3_1.Length > 100)
                    {
                        txt3_1_1 = txt3_1.Substring(0, 50);
                        txt3_1_2 = txt3_1.Substring(50, 50);
                        txt3_1_3 = txt3_1.Substring(100);
                    }
                    else if (txt3_1.Length > 50)
                    {
                        txt3_1_1 = txt3_1.Substring(0, 50);
                        txt3_1_2 = txt3_1.Substring(50);
                    }
                }
                regex = new Regex(@"Rreplace3_1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_1_1);
                regex = new Regex(@"Rreplace3_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_1_2);
                regex = new Regex(@"Rreplace3_3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_1_3);
                regex = new Regex(@"Rreplace3_4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt3_1_4);

                #endregion

                #region txt4~txt15, txt17~24
                regex = new Regex(@"Rreplace4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt4);
                regex = new Regex(@"Rreplace5R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt5);
                regex = new Regex(@"Rreplace6R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt6);
                regex = new Regex(@"Rreplace7R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt7);
                regex = new Regex(@"Rreplace8R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt8);
                regex = new Regex(@"Rreplace9R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt9);
                regex = new Regex(@"Rreplace10R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt10);
                regex = new Regex(@"Rreplace11R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt11);
                regex = new Regex(@"Rreplace12R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt12);
                regex = new Regex(@"Rreplace13R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt13);
                regex = new Regex(@"Rreplace14R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt14);
                regex = new Regex(@"Rreplace15R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt15);
                regex = new Regex(@"Rreplace17R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt17);
                regex = new Regex(@"Rreplace18R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt18);
                regex = new Regex(@"Rreplace19_____________________________________________R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt19);
                regex = new Regex(@"Rreplace20R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt20);
                regex = new Regex(@"Rreplace21R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt21);
                regex = new Regex(@"Rreplace22R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt22);
                regex = new Regex(@"Rreplace23R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt23);
                regex = new Regex(@"Rreplace24R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt24);
                #endregion

                #region txt16
                string newtxt16 = txt16.Replace("\n", " ");
                string txt16_1 = newtxt16;
                string txt16_2 = "";
                string txt16_3 = "";
                if (newtxt16.Length > 140)
                {
                    txt16_1 = newtxt16.Substring(0, 70);
                    txt16_2 = newtxt16.Substring(70, 70);
                    txt16_3 = newtxt16.Substring(140);
                }
                else if (newtxt16.Length > 70)
                {
                    txt16_1 = newtxt16.Substring(0, 70);
                    txt16_2 = newtxt16.Substring(70);
                }
                regex = new Regex(@"Rreplace16_1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt16_1);
                regex = new Regex(@"Rreplace16_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt16_2);
                regex = new Regex(@"Rreplace16_3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt16_3);

                #endregion

                #region jobRadio
                string checkPicPath = Server.MapPath("~/Content/files/Image/check.jpg");
                string uncheckPicPath = Server.MapPath("~/Content/files/Image/uncheck.jpg");
                Picture checkpicture = new Picture(dc, InlineLayout.Inline(new SautinSoft.Document.Drawing.Size(13, 13)), checkPicPath);
                Picture uncheckpicture = new Picture(dc, InlineLayout.Inline(new SautinSoft.Document.Drawing.Size(13, 13)), uncheckPicPath);

                if (jobRadio == "0")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_6 = "";
                    txtjobRadio_7 = "";
                }
                else if (jobRadio == "1")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_6 = "";
                    txtjobRadio_7 = "";
                }
                else if (jobRadio == "2")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_6 = "";
                    txtjobRadio_7 = "";
                }
                else if (jobRadio == "3")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_7 = "";
                }
                else if (jobRadio == "4")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_6 = "";
                    txtjobRadio_7 = "";

                }
                else if (jobRadio == "5")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_6 = "";
                    txtjobRadio_7 = "";
                }
                else if (jobRadio == "6")
                {
                    regex = new Regex(@"z1z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z2z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z3z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z4z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z5z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z6z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z7z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    txtjobRadio_1 = "";
                    txtjobRadio_2 = "";
                    txtjobRadio_3 = "";
                    txtjobRadio_4 = "";
                    txtjobRadio_5 = "";
                    txtjobRadio_6 = "";
                }

                regex = new Regex(@"txtjobRadio__________________________________________________1", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_1);
                regex = new Regex(@"txtjobRadio_2", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_2);
                regex = new Regex(@"txtjobRadio_3", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_3);
                regex = new Regex(@"txtjobRadio_4", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_4);
                regex = new Regex(@"txtjobRadio_5", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_5);
                regex = new Regex(@"txtjobRadio_6", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_6);
                regex = new Regex(@"txtjobRadio_7", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txtjobRadio_7);
                #endregion

                #region purTypeRadio
                if (purTypeRadio == "0")
                {
                    regex = new Regex(@"z8z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z9z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                }
                else {
                    regex = new Regex(@"z8z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z9z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                }
                #endregion

                #region permitRadio
                if (permitRadio == "0")
                {
                    regex = new Regex(@"z10z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z11z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                }
                else if (permitRadio == "1")
                {
                    regex = new Regex(@"z10z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z11z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                }
                else
                {
                    regex = new Regex(@"z10z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z11z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                }
                #endregion

                #region AmericRadio
                if (AmericRadio == "0")
                {
                    regex = new Regex(@"z12z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z13z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                }
                else
                {
                    regex = new Regex(@"z12z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z13z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                }
                #endregion

                #region FATCARadio
                if (FATCARadio == "0")
                {
                    regex = new Regex(@"z14z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                    regex = new Regex(@"z15z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                }
                else
                {
                    regex = new Regex(@"z14z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(uncheckpicture.Content);
                    regex = new Regex(@"z15z", RegexOptions.IgnoreCase);
                    foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                        item.Replace(checkpicture.Content);
                }
                #endregion

                #region txt25~27
                string date = txt25 + "/" + txt26 + "/" + txt27;
                regex = new Regex(@"Rreplace25R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(date);
                #endregion

                #region txt28~32  txt28_1~txt32_1
                regex = new Regex(@"Rreplace27________________________________________________1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt28);
                regex = new Regex(@"Rreplace27_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt29);
                regex = new Regex(@"Rreplace27______________________________3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt30);
                regex = new Regex(@"Rreplace27______4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt31);
                regex = new Regex(@"Rreplace27_______________5R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt32);
                regex = new Regex(@"Rreplace28_________________________________________________1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt28_1);
                regex = new Regex(@"Rreplace28________________2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt29_1);
                regex = new Regex(@"Rreplace28____________________________3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt30_1);
                regex = new Regex(@"Rreplace28_______4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt31_1);
                regex = new Regex(@"Rreplace28________________5R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt32_1);
                #endregion


                #region txt33~35 
                regex = new Regex(@"Rreplace29R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt33);
                regex = new Regex(@"Rreplace30R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt34);
                regex = new Regex(@"Rreplace31R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt35);
                #endregion

                #region txt36~42  txt38_1~txt42_1  
                regex = new Regex(@"Rreplace32_____________________________________________________R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt36);
                regex = new Regex(@"Rreplace33__________________R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt37);
                regex = new Regex(@"Rreplace36____________________________________________________1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt38);
                regex = new Regex(@"Rreplace36_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt39);
                regex = new Regex(@"Rreplace36____________________________3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt40);
                regex = new Regex(@"Rreplace36_4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt41);
                regex = new Regex(@"Rreplace36_5R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt42);
                regex = new Regex(@"Rreplace37_____________________________________________________1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt38_1);
                regex = new Regex(@"Rreplace37_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt39_1);
                regex = new Regex(@"Rreplace37____________________________3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt40_1);
                regex = new Regex(@"Rreplace37_4R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt41_1);
                regex = new Regex(@"Rreplace37_5R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt42_1);
                #endregion

                #region txt43
                string txt43_1 = txt43;
                string txt43_2 = "";
                string txt43_3 = "";
                if (txt43.Length > 110)
                {
                    txt43_1 = txt43.Substring(0, 55);
                    txt43_2 = txt43.Substring(55, 55);
                    txt43_3 = txt43.Substring(110);
                }
                else if (txt43.Length > 55)
                {
                    txt43_1 = txt43.Substring(0, 55);
                    txt43_2 = txt43.Substring(55);
                }
                regex = new Regex(@"Rreplace38_1R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt43_1);
                regex = new Regex(@"Rreplace38_2R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt43_2);
                regex = new Regex(@"Rreplace38_3R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt43_3);

                regex = new Regex(@"z16z", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(checkpicture.Content);
                #endregion

                #region txt44~67  
                regex = new Regex(@"Rreplace39R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt44);
                regex = new Regex(@"Rreplace40R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt45);
                regex = new Regex(@"Rreplace41R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt46); 
                regex = new Regex(@"Rreplace42R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt47);
                regex = new Regex(@"Rreplace43R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt48);
                regex = new Regex(@"Rreplace44R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt49);
                regex = new Regex(@"Rreplace45R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt50);
                regex = new Regex(@"Rreplace46R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt51);
                regex = new Regex(@"Rreplace47R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt52);
                regex = new Regex(@"Rreplace48R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt53);
                regex = new Regex(@"Rreplace49R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt54);
                regex = new Regex(@"Rreplace50R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt55);
                regex = new Regex(@"Rreplace51R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt56);
                regex = new Regex(@"Rreplace52R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt57 + "/" + txt58 + "/" + txt59);
                regex = new Regex(@"Rreplace53R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt60);
                regex = new Regex(@"Rreplace54R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt61);
                regex = new Regex(@"Rreplace55R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt62);
                regex = new Regex(@"Rreplace56R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt63);
                regex = new Regex(@"Rreplace57R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt64);
                regex = new Regex(@"Rreplace58R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt65);
                regex = new Regex(@"Rreplace59R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt66);
                regex = new Regex(@"Rreplace60R", RegexOptions.IgnoreCase);
                foreach (ContentRange item in dc.Content.Find(regex).Reverse())
                    item.Replace(txt67);
                #endregion
                //不儲存到實體目錄 直接存在fileStream後下載
                string pdfG = Guid.NewGuid().ToString();
                MemoryStream fileStream1 = new MemoryStream();
                dc.Save(fileStream1, SaveOptions.PdfDefault);
                fileStream1.Position = 0;
                TempData[pdfG] = fileStream1.ToArray();

                string wordG = Guid.NewGuid().ToString();
                MemoryStream fileStream2 = new MemoryStream();
                dc.Save(fileStream2, SaveOptions.DocxDefault);
                fileStream2.Position = 0;
                TempData[wordG] = fileStream2.ToArray();

                //dc.Save(Server.MapPath("~/Content/files/ConCredential/Subscription Agreement" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".pdf"), SaveOptions.PdfDefault);
                //dc.Save(Server.MapPath("~/Content/files/ConCredential/Subscription Agreement" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".doc"), SaveOptions.DocxDefault);

                //return File(fileStream.ToArray(), "application/pdf", "Subscription Agreement" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".pdf");
                return Json(new {  PDFGuid = pdfG, WordGuid = wordG, errorMsg = "" }, "text/html");

                //return Json(handle);

            }
            catch (Exception ex)
            {
                return Json(new { PDFGuid = "", WordGuid = "", errorMsg = ex.ToString() }, "text/html");
            }
        }
        public  ActionResult DownloadPDF(string fileGuid)
        {
            string fileName = "Subscription Agreement" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".pdf";
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                var mime = MimeMapping.GetMimeMapping(fileName);

                //重新塞入tempdate
                TempData[fileGuid] = TempData[fileGuid];

                return File(data, mime, fileName);
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }
        public ActionResult DownloadWord(string fileGuid)
        {
            string fileName = "Subscription Agreement" + convertTime.UStoTW(DateTime.Now).ToString("yyyyMMdd") + ".doc";
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                var mime = MimeMapping.GetMimeMapping(fileName);

                //重新塞入tempdate
                TempData[fileGuid] = TempData[fileGuid];

                return File(data, mime, fileName);
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }
        //開啟檔案
        public ActionResult Open(string FileName)
        {
            //string FileName = @"譯文_Aquarius Protection Fund SPC - Safety Property SP - Subscription Agreement(2019.06.24).pdf";
            var path = Server.MapPath("/Content/files/Instrument/" + FileName);

            if (System.IO.File.Exists(path))
            {
                var mime = MimeMapping.GetMimeMapping(FileName);
                return File(path, mime, FileName);
            }
            else
            {
                return Json("找不到檔案", JsonRequestBehavior.AllowGet);
            }
        }

    }
}