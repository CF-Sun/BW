using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]

    public class CHCliController : Controller
    {
        [Route("CliVerifi")]
        public ActionResult CliVerifi()
        {
            return View();
        }
        [Route("CliAccount")]
        public ActionResult CliAccount()
        {
            return View();
        }
        [Route("CliDepositeStatus")]
        public ActionResult CliDepositeStatus()
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
        [Route("CliInfo")]
        public ActionResult CliInfo()
        {
            return View();
        }
        [Route("CliNoteRecord")]
        public ActionResult CliNoteRecord()
        {
            return View();
        }
        [Route("CliNoteAdd")]
        public ActionResult CliNoteAdd()
        {
            return View();
        }
        [Route("CliDepositeRegi")]
        public ActionResult CliDepositeRegi()
        {
            return View();
        }
        [Route("CliWithdrawalRegi")]
        public ActionResult CliWithdrawalRegi()
        {
            return View();
        }
    }
}
