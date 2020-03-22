using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class BackEndController : Controller
    {
        [Route("NewNote")]
        public ActionResult NewNote()
        {
            return View();
        }
        [Route("CliNoteRecord")]
        public ActionResult CliNoteRecord()
        {
            return View();
        }
        [Route("ConNoteRecord")]
        public ActionResult ConNoteRecord()
        {
            return View();
        }
        [Route("ConInfo")]
        public ActionResult ConInfo()
        {
            return View();
        }
        [Route("CliInfo")]
        public ActionResult CliInfo()
        {
            return View();
        }
        [Route("ConLackDoc")]
        public ActionResult ConLackDoc()
        {
            return View();
        }
        [Route("CliLackDoc")]
        public ActionResult CliLackDoc()
        {
            return View();
        }
        [Route("ConNewSignUp")]
        public ActionResult ConNewSignUp()
        {
            return View();
        }
        [Route("CliNewOpen")]
        public ActionResult CliNewOpen()
        {
            return View();
        }
        [Route("ConOrgTree")]
        public ActionResult ConOrgTree()
        {
            return View();
        }
        [Route("CliNewDeposite")]
        public ActionResult CliNewDeposite()
        {
            return View();
        }
        [Route("CliNewWithdrawal")]
        public ActionResult CliNewWithdrawal()
        {
            return View();
        }
        [Route("CliDepositeStatus")]
        public ActionResult CliDepositeStatus()
        {
            return View();
        }
        [Route("CliPayedRecord")]
        public ActionResult CliPayedRecord()
        {
            return View();
        }
        [Route("ConPayedCommit")]
        public ActionResult ConPayedCommit()
        {
            return View();
        }
        [Route("ConPerform")]
        public ActionResult ConPerform()
        {
            return View();
        }
        [Route("ConPayedReport")]
        public ActionResult ConPayedReport()
        {
            return View();
        }
        [Route("IndividualBonusReport")]
        public ActionResult IndividualBonusReport()
        {
            return View();
        }
        [Route("QuarterlyBonusReport")]
        public ActionResult QuarterlyBonusReport()
        {
            return View();
        }
        [Route("CliManage")]
        public ActionResult CliManage()
        {
            return View();
        }
        [Route("RoleManage")]
        public ActionResult RoleManage()
        {
            return View();
        }
        [Route("AccountManage")]
        public ActionResult AccountManage()
        {
            return View();
        }
        [Route("CliPWManage")]
        public ActionResult CliPWManage()
        {
            return View();
        }
        [Route("ConPWManage")]
        public ActionResult ConPWManage()
        {
            return View();
        }
        [Route("MailVerifi")]
        public ActionResult MailVerifi()
        {
            return View();
        }
        [Route("Announcement")]
        public ActionResult Announcement()
        {
            return View();
        }
        [Route("Purchase")]
        public ActionResult Purchase()
        {
            return View();
        }
        [Route("ConTransfer")]
        public ActionResult ConTransfer()
        {
            return View();
        }
        [Route("CliTransfer")]
        public ActionResult CliTransfer()
        {
            return View();
        }
        [Route("DepositeType")]
        public ActionResult DepositeType()
        {
            return View();
        }
        [Route("excelImport")]
        public ActionResult excelImport()
        {
            return View();
        }

    }
}
