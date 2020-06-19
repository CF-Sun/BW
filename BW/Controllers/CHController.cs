using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BW.Controllers
{
    [Route("[controller]")]
    public class CHController : Controller
    {
        [Route("Index")]
        public ActionResult Index()
        {
            return View();
        }
        [Route("ConLogin")]
        public ActionResult ConLogin()
        {
            return View();
        }
        [Route("Word2PDF")]
        public ActionResult Word2PDF()
        {
            return View();
        }
        [Route("PurchasePDF")]
        public ActionResult PurchasePDF()
        {
            return View();
        }
        [Route("ConVerifi")]
        public ActionResult ConVerifi()
        {
            return View();
        }
        [Route("Purchase")]
        public ActionResult Purchase()
        {
            return View();
        }
        [Route("PurchaseStep")]
        public ActionResult PurchaseStep()
        {
            return View();
        }
        [Route("PurchaseCustom")]
        public ActionResult PurchaseCustom()
        {
            return View();
        }
        [Route("Contact")]
        public ActionResult Contact()
        {
            return View();
        }
        [Route("ConRegi")]
        public ActionResult ConRegi()
        {
            return View();
        }
        [Route("pur_download")]
        public ActionResult pur_download()
        {
            return View();
        }
        [Route("Maintain")]
        public ActionResult Maintain()
        {
            return View();
        }
        
    }
}
