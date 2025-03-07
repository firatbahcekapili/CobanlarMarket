using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CobanlarMarket.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Error()
        {

            Exception exception = Server.GetLastError();
            return View("Error");
        }


        public ViewResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }

        public ActionResult ManagementError()
        {
            Exception exception = Server.GetLastError();

            return View();
        }


    }
}