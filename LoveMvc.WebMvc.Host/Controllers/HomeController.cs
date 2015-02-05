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
            LoveMvc.WebMvc.Program.Host(ControllerContext, (name, m) =>
            {
                var result = PartialView(name, m);
                result.ExecuteResult(ControllerContext);

                // TESTING
                //Response.End();
            });
            return null;
        }

        public ActionResult Test()
        {
            return View();
        }
        
        public ActionResult Test2()
        {
            var model = new LoveMvc.WebMvc.TestDocs.TodosViewModel();
            return View(model);
        }
    }
}