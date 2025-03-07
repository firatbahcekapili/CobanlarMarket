using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CobanlarMarket
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error()
        {
            Exception exception = Server.GetLastError();
            Response.Clear();

            var httpException = exception as HttpException;
            string action;

            // Hata kodlarını kontrol et
            if (httpException != null)
            {
                // Yönetim kontrolcüsünde hata oluşmuşsa
                if (Request.Url.AbsolutePath.Contains("/Management"))
                {
                    // Yönetim hatası için yönlendirme
                    action = "ManagementError"; // Yönetim hata sayfası
                }
                else
                {
                    // Diğer hatalar için yönlendirme
                    switch (httpException.GetHttpCode())
                    {
                        case 404:
                            action = "NotFound";
                            break;
                        case 500:
                            action = "Error";
                            break;
                        default:
                            action = "Error";
                            break;
                    }
                }
            }
            else
            {
                // Belirli bir hata yoksa
                action = "Error";
            }

            // Hata sayfasına yönlendirme
            Response.Redirect($"~/Error/{action}");
        }



    }
}
