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
            //var httpContext = FakeHttpContext();

            //// http://stackoverflow.com/questions/9831463/add-keys-and-values-to-routedata-when-using-mvccontrib-to-unit-test-mvc-3-contro
            //RouteData routeData = new RouteData();
            //routeData.Values.Add("controller", "CONTROLLER");
            ////ControllerContext controllerContext = new ControllerContext { RouteData = routeData };
            //ControllerContext controllerContext = new ControllerContext(httpContext, routeData, new Mock<ControllerBase>().Object);

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
