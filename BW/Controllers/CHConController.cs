using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]

    public class CHConController : Controller
    {
        [Route("ConInfo")]
        public ActionResult ConInfo()
        {
            return View();
        }
        [Route("ConVerifi")]
        public ActionResult ConVerifi()
        {
            return View();
        }
        [Route("CliInfo")]
        public ActionResult CliInfo()
        {
            return View();
        }
        [Route("CliManage")]
        public ActionResult CliManage()
        {
            return View();
        }
        [Route("CliDepositeStatus")]
        public ActionResult CliDepositeStatus()
        {
            return View();
        }
        [Route("ConOrgTree")]
        public ActionResult ConOrgTree()
        {
            return View();
        }
        [Route("ConHieraRecord")]
        public ActionResult ConHieraRecord()
        {
            return View();
        }
        [Route("Announcement")]
        public ActionResult Announcement()
        {
            return View();
        }
        [Route("AnnouncementDetail")]
        public ActionResult AnnouncementDetail()
        {
            return View();
        }
        [Route("PerformIndividual")]
        public ActionResult PerformIndividual()
        {
            return View();
        }
        [Route("PerformOrgan")]
        public ActionResult PerformOrgan()
        {
            return View();
        }
        [Route("CliWithdrawalRegi")]
        public ActionResult CliWithdrawalRegi()
        {
            return View();
        }
        [Route("CliDepositeRegi")]
        public ActionResult CliDepositeRegi()
        {
            return View();
        }
        [Route("ConNoteRecord")]
        public ActionResult ConNoteRecord()
        {
            return View();
        }
        [Route("ConPayedReport")]
        public ActionResult ConPayedReport()
        {
            return View();
        }
        [Route("ConNoteAdd")]
        public ActionResult ConNoteAdd()
        {
            return View();
        }
        [Route("CliRegi")]
        public ActionResult CliRegi()
        {
            return View();
        }
        [Route("CliRegiAdd")]
        public ActionResult CliRegiAdd()
        {
            return View();
        }
        [Route("CliRegiDetail")]
        public ActionResult CliRegiDetail()
        {
            return View();
        }
    }
}
