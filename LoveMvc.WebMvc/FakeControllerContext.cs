using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LoveMvc.WebMvc
{
    public class FakeControllerContext
    {
        public static ControllerContext CreateControllerContext()
        {
            var httpRequest = new HttpRequest("FILENAME", "http://localhost/", "QUERYSTRING");
            var httpResponse = new HttpResponse(new StringWriter());

            var routeData = new RouteData();
            routeData.Values.Add("controller", "CONTROLLER");

            var httpContext = new HttpContext(httpRequest, httpResponse);
            var httpContextWrapper = new HttpContextWrapper(httpContext);
            var controller = new SimpleController();

            var controllerContext = new ControllerContext(httpContextWrapper, routeData, controller);
            return controllerContext;
        }

        private class SimpleController : Controller
        {

        }
    }
}
