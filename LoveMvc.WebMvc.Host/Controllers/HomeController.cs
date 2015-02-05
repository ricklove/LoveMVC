using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoveMvc.WebMvc.Host.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            foreach (var doc in LoveMvc.TestDocs.WebMvc._TestDocs.GetDocs())
            {
                LoveMvc.WebMvc.Program.Host(ControllerContext, doc);
            }

            return null;
        }
    }
}