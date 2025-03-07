using CobanlarMarket.Hubs;
using CobanlarMarket.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Model.V2.Transaction;
using Iyzipay.Request;
using Iyzipay.Request.V2;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using BCrypt.Net;
using System.Collections.Specialized;
using System.IdentityModel;
namespace CobanlarMarket.Controllers
{
    public class HomeController : Controller
    {
        private CobanlarMarketEntities db = new CobanlarMarketEntities();
        private EmailService _emailService = new EmailService();


        private void SendCampaignExpirationNotification(campaigns item)
        {
            db.notification.Add(new notification
            {
                is_read = false,
                status = true,
                title = "Kampanyanın Süresi Doldu!",
                text = item.campaign_title + " başlıklı kampanyanın süresi dolmuştur.",
                user_id = db.users.FirstOrDefault(x => x.role == true).id,
                order_id = 0,
                product_id = 0,
                type = "campaign",
                campaign_id = item.id
            });
            db.SaveChanges();

            var notificationProjection = db.notification
                .Where(x => x.status == true)
                .OrderByDescending(o => o.id)
                .Select(c => new
                {
                    c.id,
                    c.user_id,
                    c.order_id,
                    c.text,
                    c.title,
                    c.is_read,
                    c.status,
                    c.type,
                    c.campaign_id,
                    c.product_id,

                    User = new
                    {
                        c.users.first_name,
                        c.users.last_name,
                        c.users.id,
                        c.users.avatar
                    }
                }).ToList();

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.All.receiveNotification("Kampanyanın Süresi Doldu!", null, null, null, notificationProjection);
        }
        private void SendProductStockNotification(products item, string message)
        {
            db.notification.Add(new notification
            {
                is_read = false,
                status = true,
                title = "Ürün Stok Bildirimi!",
                text = message,
                user_id = db.users.FirstOrDefault(x => x.role == true).id,
                order_id = 0,

                type = "product",
                campaign_id = 0,
                product_id = item.id,
            });
            db.SaveChanges();

            var notificationProjection = db.notification
                .Where(x => x.status == true)
                .OrderByDescending(o => o.id)
                .Select(c => new
                {
                    c.id,
                    c.user_id,
                    c.order_id,
                    c.text,
                    c.title,
                    c.is_read,
                    c.status,
                    c.type,
                    c.product_id,
                    c.campaign_id,

                    User = new
                    {
                        c.users.first_name,
                        c.users.last_name,
                        c.users.id,
                        c.campaign_id,
                        c.users.avatar
                    }
                }).ToList();

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            hubContext.Clients.All.receiveNotification("Ürün Stok Bildirimi!", null, null, null, notificationProjection);
        }

        private Dictionary<string, SeoModel> LoadAllSeoSettings()
        {
            string seoFilePath = Server.MapPath("~/Content/seoSettings.json");
            if (System.IO.File.Exists(seoFilePath))
            {
                var jsonData = System.IO.File.ReadAllText(seoFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, SeoModel>>(jsonData);
            }
            return new Dictionary<string, SeoModel>();
        }

        private void LoadSeoSettingsToViewBag(string page, int productId = 0, int campaignId = 0)
        {
            var seoData = LoadAllSeoSettings();
            seoData.TryGetValue(page, out var seoSettings);

            // SEO verilerini ViewBag'e atıyoruz
            ViewBag.Title = seoSettings?.Title ?? "Çobanlar Market";
            ViewBag.Description = seoSettings?.Description ?? "Taze, kaliteli ürünlerin adresi.";
            ViewBag.Keywords = seoSettings?.Keywords ?? "market, taze, kalite,";
            ViewBag.NoIndex = seoSettings?.NoIndex ?? false;
            ViewBag.NoFollow = seoSettings?.NoFollow ?? false;

            if (seoSettings == null)
            {

                if (productId != 0)
                {
                    var product = db.products.FirstOrDefault(x => x.id == productId);
                    if (product != null && product.status != false)
                    {

                        ViewBag.Title = $"{product.name} - {ViewBag.Title}";
                        ViewBag.Description = $"{product.description}.";

                    }
                }
                if (campaignId != 0)
                {
                    var campaign = db.campaigns.FirstOrDefault(x => x.id == campaignId);
                    if (campaign != null && campaign.is_active == true)
                    {

                        ViewBag.Title = $"{campaign.campaign_title} - {ViewBag.Title}";
                        ViewBag.Description = $"{campaign.campaign_title} indirimler sizi bekliyor..";

                    }
                }

            }


        }

        public ActionResult Index()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);


            var expiredCampaigns = db.campaigns
                .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
                .ToList();

            expiredCampaigns.ForEach(c => c.is_active = false);


            foreach (var item in expiredCampaigns)
            {
                item.is_active = false;

                SendCampaignExpirationNotification(item);

            }



            db.SaveChanges();


            //var user = db.users.FirstOrDefault(x => x.id == 1088);
            //foreach (var item in user.order_details.ToList() )
            //{

            //    db.order_item.RemoveRange(item.order_item);
            //    db.payment_details.RemoveRange(item.payment_details);
            //     db.SaveChanges();



            //}
            //if (db.cart.FirstOrDefault(x => x.user_id == user.id) != null)
            //{
            //    db.cart.Remove(db.cart.FirstOrDefault(x => x.user_id == user.id));
            //}
            //db.order_details.RemoveRange(user.order_details);
            //db.notification.RemoveRange(user.notification);
            //db.users.Remove(user);

            //db.SaveChanges();

            //var dp = db.products.Where(x => x.status == false);
            //var od = db.order_details.Where(x => x.order_item.Count() == 0).ToList() ;
            //var pd = db.payment_details.Where(x => x.order_details.order_item.Count() == 0).ToList();

            //db.order_details.RemoveRange(od);
            //db.payment_details.RemoveRange(pd);
            //db.SaveChanges();

            //foreach (var item in db.products.Where(x => x.status == false))
            //{
            //    db.products_skus.RemoveRange(item.products_skus);
            //    db.order_item.RemoveRange(item.order_item);
            //    db.product_images.RemoveRange(item.product_images);
            //}
            //db.products.RemoveRange(dp);
            //db.SaveChanges();

            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();

            return View(model);

        }



        public ActionResult Shop()
        {

            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();

            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);
        }

        public ActionResult Contact()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);

            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);

        }
        [HttpGet]
        public ActionResult MyAccount(int id)
        {

            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);

            var user = Session["User"] as users;
            if (user != null)
            {
                if (user.id != id)
                {
                    return HttpNotFound();
                }
            }



            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.Where(x => x.id == user.id).Include(x => x.addresses).ToList();

            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_details = db.order_details.Where(x => x.user_id == id).Include(x => x.order_item).Include(x => x.payment_details).ToList();
            model.order_item = db.order_item.Where(x => x.order_details.user_id == id).ToList();
            model.payment_details = db.payment_details.Where(x => x.order_details.user_id == id).ToList();
            return View(model);

        }
        private string EncodeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // Dosya adını küçük harfe çevir ve boşlukları "-" ile değiştir
            fileName = fileName.Replace(" ", "-");

            // Özel karakterleri kaldır ve yalnızca harf, rakam ve "-" karakterlerini bırak
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"[^A-Za-z0-9-]", "");

            return fileName;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditUser(HttpPostedFileBase Img, string Id, string Name, string Surname, string Username, string Password, string Email, string Tel, string Birthdate)
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            int userid = int.Parse(Id);
            var user = db.users.Find(userid);
            var currentUser = Session["User"] as users;
            if (currentUser == null)
            {
                LogOut();
                RedirectToAction("Auth");
            }


            if (user != null)
            {


                if (currentUser.id != user.id)
                {
                    return Json(new { success = false, message = "Bunu yapmaya yetkiniz bulunmamaktadır." }, JsonRequestBehavior.AllowGet);

                }


                if (user.username != Username)
                {
                    if (db.users.Any(x => x.username == Username))
                    {
                        return Json(new { success = false, message = "Bu kullanıcı adına sahip bir hesap zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

                    }
                }

                if (user.email != Email)
                {
                    if (db.users.Any(x => x.email == Email))
                    {
                        return Json(new { success = false, message = "Bu Email adresine sahip bir kullanıcı zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

                    }
                }
                if (user.phone_number != Tel)
                {
                    if (db.users.Any(x => x.phone_number == Tel))
                    {
                        return Json(new { success = false, message = "Bu telefon numarasına sahip bir kullanıcı zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

                    }
                }
                if (Img != null)
                {
                    string imgpath = null;




                    var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                    var extension = Path.GetExtension(Img.FileName);

                    // Dosya adını encode et
                    var fileName = EncodeFileName(originalFileName) + extension;
                    var folderName = EncodeFileName(Username);
                    var path = Path.Combine(Server.MapPath("~/Content/UserImg/" + folderName + "/"));

                    imgpath = "/Content/UserImg/" + folderName + "/" + fileName;

                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        foreach (FileInfo file in directoryInfo.GetFiles())
                        {
                            file.Delete();
                        }
                        Img.SaveAs(Path.Combine(path, fileName));
                    }
                    else
                    {
                        Directory.CreateDirectory(path);
                        Img.SaveAs(Path.Combine(path, fileName));
                    }
                    user.avatar = imgpath;
                }

                user.first_name = Name;
                user.last_name = Surname;
                user.username = Username;

                user.email = Email;
                user.phone_number = Tel;
                if (!Password.IsNullOrWhiteSpace())
                {
                    if (Password.Length < 8)
                    {
                        return Json(new { success = false, message = "Şifreniz en az 8 karakterden oluşmalıdır." }, JsonRequestBehavior.AllowGet);

                    }

                    user.password = HashPassword(Password);


                }
                else
                {
                    user.password = user.password;
                }



                if (!TryValidateModel(user))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();


                    return Json(new { success = false, message = errors });
                }



                if (DateTime.TryParse(Birthdate, out DateTime birthdate))
                {
                    user.birth_of_date = birthdate;
                }
                else
                {
                    ModelState.AddModelError("Birthdate", "Geçersiz tarih formatı");
                    return Json(new { success = false, message = "Geçersiz tarih formatı" }, JsonRequestBehavior.AllowGet);
                }

                db.SaveChanges();

                Session["User"] = user;
                return Json(new
                {
                    success = true,
                    user = new
                    {
                        Id = user.id,
                        first_name = user.first_name,
                        last_name = user.last_name,
                        username = user.username,
                        email = user.email,
                        phone_number = user.phone_number,
                        birth_of_date = user.birth_of_date,
                        avatar = user.avatar
                    },
                    message = "Düzenleme işlemi başarılı."
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEmail(String Email, String OldMail)
        {



            if (db.users.Any(x => x.email == Email))
            {
                return Json(new { success = false, message = "Bu Email adresine sahip bir kullanıcı zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

            }

            var sessionUser = Session["User"] as users;
            if (sessionUser == null)
            {
                LogOut();
                return Json(new { success = false, message = "Oturum bulunamadı." }, JsonRequestBehavior.AllowGet);
            }
            var currentuser = db.users.FirstOrDefault(x => x.id == sessionUser.id);

            if (currentuser.email != OldMail)
            {
                return Json(new { success = false, message = "Bunu yapmaya yetkiniz yok." }, JsonRequestBehavior.AllowGet);

            }


            if (currentuser.ban_expiry_date.HasValue && currentuser.ban_expiry_date > DateTime.Now)
            {
                return Json(new { success = false, message = "Çok fazla denemeden dolayı bir süre yasaklandınız." }, JsonRequestBehavior.AllowGet);

            }

            if (currentuser.ban_expiry_date < DateTime.Now)
            {
                currentuser.ban_expiry_date = null;
                currentuser.attempt_count = 0;
                db.SaveChanges();
            }
            if (!Email.Replace(" ", "").Equals(""))
            {

                bool isvalidemail = true;
                try
                {
                    MailAddress m = new MailAddress(Email);


                }
                catch (FormatException)
                {
                    isvalidemail = false;
                }
                if (!isvalidemail)
                {
                    return Json(new { success = false, message = "Geçersiz Email adresi" }, JsonRequestBehavior.AllowGet);

                }

                Random random = new Random();

                var code = random.Next(100000, 1000000);

                string htmlBody = @"
                    <html>
                    <head>
                        <meta charset='utf-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Çobanlar Market Kayıt Onay Kodunuz</title>
                        <style>
                            body {
                                font-family: 'Arial', sans-serif;
                                background-color: #f9f9f9;
                                margin: 0;
                                padding: 20px;
                            }
                            .container {
                                max-width: 600px;
                                margin: auto;
                                background: #ffffff;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                            }
                            h1 {
                                color: #333;
                                font-size: 24px;
                                margin-bottom: 10px;
                            }
                            h3 {
                                color: #4CAF50;
                                font-size: 20px;
                                margin-top: 20px;
                            }
                            p {
                                font-size: 16px;
                                line-height: 1.5;
                                margin: 10px 0;
                            }
                            .footer {
                                margin-top: 20px;
                                font-size: 12px;
                                color: #777;
                                border-top: 1px solid #e0e0e0;
                                padding-top: 10px;
                            }
                            .code {
                                display: inline-block;
                                font-size: 24px;
                                color: #fff;
                                background-color: #4CAF50;
                                padding: 10px 15px;
                                border-radius: 5px;
                                margin-top: 15px;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Merhaba!</h1>
                            <p>Email adres değişikliği için onay kodunuz:</p>
                            <h3 class='code'>" + code + @"</h3>
                        </div>
                        <div class='footer'>
                            <p>Bu bir otomatik e-posta mesajıdır. Lütfen yanıt vermeyin.</p>
                        </div>
                    </body>
                    </html>";

                _emailService.SendEmail(Email, "Çobanlar Market Email Adres Değişikliği Onay Kodunuz", htmlBody);

                Session["ChangeEmail"] = Email;


                var user = db.users.FirstOrDefault(x => x.email == OldMail);
                if (user != null)
                {
                    user.email_change_token = code.ToString();
                    user.email_change_token_expiry = DateTime.Now.AddMinutes(2);
                    db.SaveChanges();
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }



        }
        [HttpPost]
        public JsonResult ConfirmChangeMailCode(String Code)
        {
            var sessionUser = Session["User"] as users;

            if (sessionUser == null)
            {
                Session["ChangeEmail"] = null;
                LogOut();
                return Json(new { success = false, message = "Oturum bulunamadı." }, JsonRequestBehavior.AllowGet);
            }
            var currentuser = db.users.FirstOrDefault(x => x.id == sessionUser.id);

            var code = currentuser.email_change_token;

            int attemptCount = currentuser.attempt_count ?? 0;

            attemptCount++;
            currentuser.attempt_count = attemptCount;
            db.SaveChanges();


            if (attemptCount > 3)
            {
                currentuser.email_change_token = null;
                currentuser.email_change_token_expiry = null;
                currentuser.ban_expiry_date = DateTime.Now.AddHours(1);
                db.SaveChanges();
                return Json(new { success = false, message = "Çok fazla hatalı giriş yaptınız. 1 saat sonra tekrar deneyebilirsiniz.", redirectUrl = Url.Action("MyAccount", "Home", new { id = currentuser.id }) }, JsonRequestBehavior.AllowGet);
            }

            if (code == null || currentuser.email_change_token_expiry < DateTime.Now)
            {
                currentuser.email_change_token = null;
                currentuser.email_change_token_expiry = null;
                db.SaveChanges();

                return Json(new { success = false, message = "Onay kodunun süresi doldu. Tekrar kod alabilirsiniz." }, JsonRequestBehavior.AllowGet);
            }

            if (code == Code)
            {



                users user = db.users.FirstOrDefault(x => x.email == sessionUser.email);

                try
                {
                    user.email = Session["ChangeEmail"] as string;

                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = e.InnerException?.InnerException?.Message }, JsonRequestBehavior.AllowGet);
                }

                var newmail = Session["ChangeEmail"] as string;

                Session["User"] = db.users.FirstOrDefault(x => x.email == newmail);
                user = db.users.FirstOrDefault(x => x.email == newmail);
                user.attempt_count = 0;
                user.ban_expiry_date = null;
                user.email_change_token_expiry = null;
                user.email_change_token = null;

                db.SaveChanges();

                return Json(new { success = true, message = "Mail adresiniz güncellenmiştir.", redirectUrl = Url.Action("MyAccount", "Home", new { id = user.id }) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Hatalı kod girdiniz!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public void ResetChangeMailConfirmCode()
        {
            Session["ChangeMailConfirmationCode"] = null;
        }








       
        public ActionResult Cart()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);

            var expiredCoupons = db.coupons
            .Where(c => c.EndDate <= DateTime.Now && c.IsActive == true)
            .ToList();

            expiredCoupons.ForEach(c => c.IsActive = false);

            db.SaveChanges();





            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(p => p.products_skus).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.Include(p => p.cart_item).ToList();
            model.addresses = db.addresses.ToList();
            model.coupons = db.coupons.Where(x => x.IsActive == true).ToList();
            model.company_details = db.company_details.ToList();
            ViewBag.CompanyDetails = db.company_details.FirstOrDefault();


            if (Session["User"] != null)
            {
                var user = Session["User"] as users;
                var cart = db.cart.FirstOrDefault(x => x.user_id == user.id);

                var removedproducts = cart.cart_item.Where(x => x.products.status == false);
                db.cart_item.RemoveRange(removedproducts);
                db.SaveChanges();

                if (db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id != null)
                {
                    var couponResult = UseCoupon(db.coupons.FirstOrDefault(c => c.Id == db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id).Code.ToString());

                    // JSON sonucunu dinamik bir objeye dönüştür
                    dynamic jsonResult = couponResult.Data;
                    if (!jsonResult.success)
                    {
                        cart.coupon_id = null;
                        cart.discount_value = null;
                        db.SaveChanges();
                    }
                }
            }




            ViewBag.Discount = 0;
            return View(model);
        }

        public ActionResult Campaing()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);

            var expiredCampaigns = db.campaigns
                .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
                .ToList();
            foreach (var item in expiredCampaigns)
            {
                item.is_active = false;

                SendCampaignExpirationNotification(item);

            }
            db.SaveChanges();

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(p => p.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);


        }
        public ActionResult CampaingDetails(int id)
        {



            var expiredCampaigns = db.campaigns
             .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
             .ToList();

            foreach (var item in expiredCampaigns)
            {
                item.is_active = false;

                SendCampaignExpirationNotification(item);

            }
            db.SaveChanges();

            if (!db.campaigns.Any(x => x.id == id && x.is_active == true))
            {
                return HttpNotFound();
            }
            // İlgili kampanyadaki ürünlerin ID'leri
            var campaignProductIds = db.campaigns
                                       .Where(x => x.id == id)
                                       .SelectMany(x => x.campaign_products)
                                       .Select(cp => cp.product_id)
                                       .ToList();

            AllViewModel model = new AllViewModel();

            // Sadece campaignProductIds'e ait ürünler
            var products = db.products
                             .Where(p => campaignProductIds.Contains(p.id) && p.status == true)
                             .ToList();

            model.products = products;

            // Bu ürünlere ait kategoriler ve alt kategoriler



            var subCategoryIds = products.Select(p => p.subcategory_id).Distinct().ToList();
            // Kategorileri filtrele


            model.sub_categories = db.sub_categories
                                .Where(sc => subCategoryIds.Contains(sc.id))
                                .ToList();
            var parentCategoryIds = db.sub_categories
        .Where(sc => subCategoryIds.Contains(sc.id))
        .Select(sc => sc.parent_id)
        .Distinct()
        .ToList();


            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}/{id}";
            var camp = db.campaigns.FirstOrDefault(x => x.id == id);
            LoadSeoSettingsToViewBag(page, 0, camp.id);
            // Belirtilen parentId değerlerine sahip kategorileri seç
            model.categories = db.categories
                .Where(c => parentCategoryIds.Contains(c.id))
                .ToList();

            model.products_skus = db.products_skus.ToList();
            model.campaigns = db.campaigns.Where(x => x.id == id).ToList();
            model.users = db.users.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);
        }



        public ActionResult Blog()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);


        }
        public ActionResult Auth()
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);


        }

        [HttpPost]
        public PartialViewResult GetProductsPartial(int? CampaignId, int CategoryId, int? CategoryType, string PriceRange, int PageNumber, string OrderType)
        {
            IOrderedQueryable<products> products = null;
            int pageSize = CampaignId.HasValue ? 12 : 27; // Kampanyada 12 ürün göster, diğer durumda 27 ürün göster.

            // Kampanya varsa, kampanya ürünlerini alın
            if (CampaignId.HasValue)
            {

                var campaignProductIds = db.campaigns
                                           .Where(x => x.id == CampaignId)
                                           .SelectMany(x => x.campaign_products)
                                           .Select(cp => cp.product_id)
                                           .ToList();

                if (CategoryId != -1)
                {
                    if (CategoryType == 1)
                    {
                        products = db.products.Where(x => x.subcategory_id == CategoryId && campaignProductIds.Contains(x.id) && x.status == true).OrderBy(x => x.id);
                    }
                    else if (CategoryType == 0)
                    {
                        products = db.products.Where(x => x.sub_categories.parent_id == CategoryId && campaignProductIds.Contains(x.id) && x.status == true).OrderBy(x => x.id);
                    }
                    else if (CategoryType == 2)
                    {
                        products = db.products.Where(x => x.sub_subcategory_id == CategoryId.ToString() && campaignProductIds.Contains(x.id) && x.status == true).OrderBy(x => x.id);

                    }
                }
                else
                {
                    products = db.products.Where(p => campaignProductIds.Contains(p.id) && p.status == true).OrderBy(p => p.id);
                }
            }
            else
            {
                // Kampanya yoksa, genel ürün sorgusu yap
                if (CategoryId != -1)
                {
                    if (CategoryType == 1)
                    {
                        products = db.products.Where(x => x.subcategory_id == CategoryId && x.status == true).OrderBy(x => x.id);
                    }
                    else if (CategoryType == 0)
                    {
                        products = db.products.Where(x => x.sub_categories.parent_id == CategoryId && x.status == true).OrderBy(x => x.id);
                    }
                    else if (CategoryType == 2)
                    {
                        products = db.products.Where(x => x.sub_subcategory_id == CategoryId.ToString() && x.status == true).OrderBy(x => x.id);

                    }
                }
                else
                {
                    products = db.products.Where(x => x.status == true).OrderBy(x => x.id);
                }
            }

            // Fiyat aralığına göre ürünleri filtreleme
            if (!string.IsNullOrEmpty(PriceRange))
            {
                var prices = PriceRange.Split('-');
                if (prices.Length == 2)
                {
                    if (decimal.TryParse(prices[0], out decimal minPrice) && decimal.TryParse(prices[1], out decimal maxPrice))
                    {
                        products = products.Where(x => x.products_skus.FirstOrDefault().price >= minPrice && x.products_skus.FirstOrDefault().price <= maxPrice).OrderBy(x => x.id);
                    }
                }
            }

            // Fiyata veya isme göre sıralama
            switch (OrderType)
            {
                case "price_asc":
                    products = products.OrderBy(x => x.products_skus.FirstOrDefault().price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(x => x.products_skus.FirstOrDefault().price);
                    break;
                case "A-Z":
                    products = products.OrderBy(x => x.name);
                    break;
                case "Z-A":
                    products = products.OrderByDescending(x => x.name);
                    break;
                default:
                    products = products.OrderBy(x => x.id);
                    break;
            }

            var pagedProducts = products.Skip((PageNumber - 1) * pageSize).Take(pageSize).ToList();
            var totalProducts = products.Count();
            ViewBag.totalProcuct = totalProducts;
            ViewBag.pageSize = pageSize;
            ViewBag.categoryId = CategoryId;
            ViewBag.pageNumber = PageNumber;
            ViewBag.orderType = OrderType;

            AllViewModel model = new AllViewModel();
            model.products = pagedProducts;
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();

            return PartialView("_ProductsPartial", model);
        }


        public ActionResult Product(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadGateway);
            }


            products product = db.products.FirstOrDefault(x => x.status == true && x.id == Id);

            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}/{Id}";
            LoadSeoSettingsToViewBag(page, product.id, 0);
            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).Include(x => x.product_images).Where(x => x.id == Id).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();

            model.product_attributes = db.product_attributes.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult AddCart(int Id, int Adet)
        {
            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {


                    if (db.products.Any(x => x.id == Id))
                    {
                        var prod = db.products.FirstOrDefault(x => x.id == Id);
                        if (prod.products_skus.FirstOrDefault().quantity <= 0)
                        {
                            SendProductStockNotification(prod, prod.name + " isimli ürünün stoğu tükenmiştir.");

                            return Json(new { success = false, message = "Stoklarımız tükenmiştir." });


                        }

                        if (prod.products_skus.FirstOrDefault().quantity < Adet)
                        {
                            SendProductStockNotification(prod, prod.name + " isimli ürünün stoğu azalmıştır.");

                            return Json(new { success = false, message = "Stoklarımızda yeteri kadar ürün kalmamıştır." });
                        }
                    }

                    cart_item item = new cart_item();
                    item.product_id = Id;
                    item.quantity = Adet;
                    item.created_at = DateTime.Now;
                    var cart = db.cart.FirstOrDefault(x => x.user_id == user.id);
                    var existingItem = db.cart.FirstOrDefault(x => x.user_id == user.id).cart_item
                                        .FirstOrDefault(x => x.product_id == Id);

                    if (existingItem != null)
                    {
                        existingItem.quantity += Adet;
                    }
                    else
                    {
                        cart.cart_item.Add(item);
                    }
                    db.SaveChanges();


                    var cartitems = db.cart_item.Where(x => x.cart_id == cart.id).Include(x => x.products.products_skus).ToList();

                    var carttotal = cartitems.Sum(y => y.products.products_skus.FirstOrDefault().price * y.quantity);
                    db.cart.FirstOrDefault(z => z.user_id == user.id).total = carttotal;
                    var minAmount = db.company_details.FirstOrDefault().min_amonunt_for_free_shipping.GetValueOrDefault(300); //Default 300

                    if (carttotal >= minAmount)
                    {
                        db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = true;
                    }
                    else
                    {
                        db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = false;
                    }
                    db.SaveChanges();



                    Session["CartsForPartial"] = db.cart.ToList();
                    Session["CouponsForPartial"] = db.coupons.ToList();
                    ViewBag.CompanyDetails = db.company_details.FirstOrDefault();
                    var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == cart.id).ToList());
                    string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);

                    return Json(new { success = true, message = db.products.Find(Id).name.ToString() + " Ürünü Başarıyla Sepetinize Eklendi", cartItemsHtml = cartItemsHtml });
                }
                else
                {
                    return Json(new { success = false, message = "Sepete Ekleyebilmek İçin Giriş Yapmalısınız." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        private string RenderPartialViewToString(ControllerContext context, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = context.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(context, viewName);
                ViewContext viewContext = new ViewContext(context, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public PartialViewResult removeItemInCart(int Id)
        {
            var user = Session["User"] as users;
            if (user != null)
            {
                db.cart_item.Remove(db.cart_item.FirstOrDefault(x => x.id == Id));
                db.SaveChanges();
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;

                var cartitems = db.cart_item.Where(x => x.cart_id == db.cart.FirstOrDefault(c => c.user_id == user.id).id).ToList();

                var carttotal = cartitems.Sum(y => y.products.products_skus.FirstOrDefault().price * y.quantity);
                db.cart.FirstOrDefault(z => z.user_id == user.id).total = carttotal;
                var minAmount = db.company_details.FirstOrDefault().min_amonunt_for_free_shipping.GetValueOrDefault(300); //Default 300

                if (carttotal >= minAmount)
                {
                    db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = true;
                }
                else
                {
                    db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = false;
                }
                db.SaveChanges();
                if (db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id != null)
                {
                    var cart = db.cart.FirstOrDefault(x => x.user_id == user.id);
                    var couponResult = UseCoupon(db.coupons.FirstOrDefault(c => c.Id == db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id).Code.ToString());

                    // JSON sonucunu dinamik bir objeye dönüştür
                    dynamic jsonResult = couponResult.Data;
                    if (!jsonResult.success)
                    {
                        cart.coupon_id = null;
                        cart.discount_value = null;
                        db.SaveChanges();
                    }




                }

                Session["CartsForPartial"] = db.cart.ToList();
                Session["CouponsForPartial"] = db.coupons.ToList();
                ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                return PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == cartId).ToList());
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult minusItemInCart(int Id)
        {
            var user = Session["User"] as users;
            if (user != null)
            {
                if (db.cart_item.FirstOrDefault(x => x.id == Id).quantity > 1)
                {

                    db.cart_item.FirstOrDefault(x => x.id == Id).quantity--;
                    db.SaveChanges();

                    var cartitems = db.cart_item.Where(x => x.cart_id == db.cart.FirstOrDefault(c => c.user_id == user.id).id).ToList();

                    var carttotal = cartitems.Sum(y => y.products.products_skus.FirstOrDefault().price * y.quantity);
                    db.cart.FirstOrDefault(z => z.user_id == user.id).total = carttotal;

                    var minAmount = db.company_details.FirstOrDefault().min_amonunt_for_free_shipping.GetValueOrDefault(300); //Default 300

                    if (carttotal >= minAmount)
                    {
                        db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = true;
                    }
                    else
                    {
                        db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = false;
                    }

                }
                else
                {
                    db.cart_item.Remove(db.cart_item.FirstOrDefault(x => x.id == Id));
                    db.SaveChanges();

                    db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = false;

                }
                db.SaveChanges();


                if (db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id != null)
                {
                    var cart = db.cart.FirstOrDefault(x => x.user_id == user.id);
                    var couponResult = UseCoupon(db.coupons.FirstOrDefault(c => c.Id == db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id).Code.ToString());

                    // JSON sonucunu dinamik bir objeye dönüştür
                    dynamic jsonResult = couponResult.Data;
                    if (!jsonResult.success)
                    {
                        cart.coupon_id = null;
                        cart.discount_value = null;
                        db.SaveChanges();
                    }




                }
                Session["CartsForPartial"] = db.cart.ToList();
                Session["CouponsForPartial"] = db.coupons.ToList();
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;
                ViewBag.CompanyDetails = db.company_details.FirstOrDefault();


                return PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == cartId).ToList());
            }
            return null;

        }

        [HttpPost]
        public PartialViewResult increaseItemInCart(int Id)
        {

            var user = Session["User"] as users;
            if (user != null)
            {


                db.cart_item.FirstOrDefault(x => x.id == Id).quantity++;

                db.SaveChanges();



                var cartitems = db.cart_item.Where(x => x.cart_id == db.cart.FirstOrDefault(c => c.user_id == user.id).id).ToList();

                var carttotal = cartitems.Sum(y => y.products.products_skus.FirstOrDefault().price * y.quantity);
                db.cart.FirstOrDefault(z => z.user_id == user.id).total = carttotal;
                var minAmount = db.company_details.FirstOrDefault().min_amonunt_for_free_shipping.GetValueOrDefault(300); //Default 300

                if (carttotal >= minAmount)
                {
                    db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = true;
                }
                else
                {
                    db.cart.FirstOrDefault(z => z.user_id == user.id).isCargoFree = false;
                }
                db.SaveChanges();

                if (db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id != null)
                {
                    var cart = db.cart.FirstOrDefault(x => x.user_id == user.id);
                    var couponResult = UseCoupon(db.coupons.FirstOrDefault(c => c.Id == db.cart.FirstOrDefault(x => x.user_id == user.id).coupon_id).Code.ToString());

                    // JSON sonucunu dinamik bir objeye dönüştür
                    dynamic jsonResult = couponResult.Data;
                    if (!jsonResult.success)
                    {
                        cart.coupon_id = null;
                        cart.discount_value = null;
                        db.SaveChanges();
                    }
                }
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;
                Session["CartsForPartial"] = db.cart.ToList();
                Session["CouponsForPartial"] = db.coupons.ToList();
                ViewBag.CompanyDetails = db.company_details.FirstOrDefault();


                return PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == cartId).ToList());

            }
            return null;

        }

        //public JsonResult GetProducts(int CategoryId, String PriceRange)
        //{
        //    try
        //    {
        //        if (db.products.Any(x => x.category_id == CategoryId))
        //        {
        //            product_attributes attribute = db.product_attributes.FirstOrDefault(x => x.id == Id);
        //            if (attribute != null)
        //            {
        //                if (attribute.type == "color")
        //                {
        //                    foreach (var item in db.products_skus.Where(x => x.color_attribute_id == Id))
        //                    {
        //                        item.color_attribute_id = -1;
        //                    }
        //                }
        //                else if (attribute.type == "size")
        //                {
        //                    foreach (var item in db.products_skus.Where(x => x.size_attribute_id == Id))
        //                    {
        //                        item.size_attribute_id = -1;
        //                    }
        //                }

        //                db.SaveChanges(); // Ürün SKU'ları güncelleme değişikliklerini kaydet
        //                db.product_attributes.Remove(attribute);
        //                db.SaveChanges(); // Attribute'u kaldırma değişikliklerini kaydet
        //            }
        //        }

        //        var list = db.product_attributes
        //         .ToList()  // Retrieve data from database before formatting
        //         .Select(x => new
        //         {
        //             x.id,
        //             x.type,
        //             x.value,
        //             created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"
        //         })
        //         .ToList();
        //        return Json(list, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Hata durumunda JSON hata mesajı döndür
        //        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }

        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Auth(String username, String password)
        {
            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {


                if (db.users.Where(x => x.username == username || x.email == username).Count() != 0)
                {
                    var user = db.users.FirstOrDefault(x => x.username == username || x.email == username);

                    if (!VerifyPassword(password, user.password))
                    {
                        return Json(new { success = false, message = "Hatalı bilgi girdiniz.", redirectUrl = Url.Action("Auth", "Home") });

                    }
                    Session["User"] = db.users.FirstOrDefault(x => x.username == username || x.email == username);

                    if (user.role == true)
                    {

                        FormsAuthentication.SetAuthCookie("Yönetici", false);
                        return Json(new { success = true, redirectUrl = Url.Action("Dashboard", "Management") });
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie("Müşteri", false);
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

                    }
                }
                else
                {
                    return Json(new { success = false, message = "Hatalı bilgi girdiniz.", redirectUrl = Url.Action("Auth", "Home") });
                }

            }
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);


        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(String Name, String Lastname, String username, String password, String Email)
        {


            var inputs = new[]
           {
                    Name,
                    Lastname,
                    username,
                    password,
                    Email
                };

            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return Json(new { success = false, message = "Tüm alanlar zorunludur." }, JsonRequestBehavior.AllowGet);
                }
            }
            if (db.users.Any(x => x.username == username))
            {
                return Json(new { success = false, message = "Bu kullanıcı adına sahip bir hesap zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

            }

            if (db.users.Any(x => x.email == Email))
            {
                return Json(new { success = false, message = "Bu Email adresine sahip bir kullanıcı zaten bulunmaktadır." }, JsonRequestBehavior.AllowGet);

            }

            if (!username.Replace(" ", "").Equals("") && !password.Replace(" ", "").Equals("") && !Email.Replace(" ", "").Equals("") && !Name.Replace(" ", "").Equals("") && !Lastname.Replace(" ", "").Equals(""))
            {

                bool isvalidemail = true;
                try
                {
                    MailAddress m = new MailAddress(Email);


                }
                catch (FormatException)
                {
                    isvalidemail = false;
                }
                if (!isvalidemail)
                {
                    return Json(new { success = false, message = "Geçersiz Email adresi" }, JsonRequestBehavior.AllowGet);

                }

                Random random = new Random();

                var code = random.Next(1000, 10000);

                string htmlBody = @"
                    <html>
                    <head>
                        <meta charset='utf-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Çobanlar Market Kayıt Onay Kodunuz</title>
                        <style>
                            body {
                                font-family: 'Arial', sans-serif;
                                background-color: #f9f9f9;
                                margin: 0;
                                padding: 20px;
                            }
                            .container {
                                max-width: 600px;
                                margin: auto;
                                background: #ffffff;
                                padding: 20px;
                                border-radius: 8px;
                                box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                            }
                            h1 {
                                color: #333;
                                font-size: 24px;
                                margin-bottom: 10px;
                            }
                            h3 {
                                color: #4CAF50;
                                font-size: 20px;
                                margin-top: 20px;
                            }
                            p {
                                font-size: 16px;
                                line-height: 1.5;
                                margin: 10px 0;
                            }
                            .footer {
                                margin-top: 20px;
                                font-size: 12px;
                                color: #777;
                                border-top: 1px solid #e0e0e0;
                                padding-top: 10px;
                            }
                            .code {
                                display: inline-block;
                                font-size: 24px;
                                color: #fff;
                                background-color: #4CAF50;
                                padding: 10px 15px;
                                border-radius: 5px;
                                margin-top: 15px;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <h1>Merhaba " + Name + @"!</h1>
                            <p>Kayıt olmak için onay kodunuz:</p>
                            <h3 class='code'>" + code + @"</h3>
                        </div>
                        <div class='footer'>
                            <p>Bu bir otomatik e-posta mesajıdır. Lütfen yanıt vermeyin.</p>
                        </div>
                    </body>
                    </html>";

                _emailService.SendEmail(Email, "Çobanlar Market Kayıt Onay Kodunuz", htmlBody);
                Session["ConfirmationCode"] = code.ToString();
                Session["NewName"] = Name.ToString();
                Session["NewLastname"] = Lastname.ToString();
                Session["NewUsername"] = username.ToString();
                Session["NewEmail"] = Email.ToString();
                Session["NewPassword"] = password.ToString();

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            }



        }
        [HttpPost]
        public JsonResult ConfirmCode(String Code)
        {
            var code = Session["ConfirmationCode"] as string;

            if (code == null)
            {
                return Json(new { success = false, message = "Onay kodunun süresi doldu. Tekrar kod alabilirsiniz." }, JsonRequestBehavior.AllowGet);

            }

            if (code == Code)
            {


                users user = new users();
                user.first_name = Session["NewName"] as string;
                user.last_name = Session["NewLastname"] as string;
                user.username = Session["NewUsername"] as string;
                user.email = Session["NewEmail"] as string;

                user.password = HashPassword(Session["NewPassword"] as string);
                user.role = false;
                user.status = true;
                user.created_at = DateTime.Now;


                try
                {
                    db.users.Add(user);
                    db.SaveChanges();

                    cart cart = new cart();
                    cart.user_id = user.id;
                    cart.created_at = DateTime.Now;
                    cart.isCargoFree = false;
                    db.cart.Add(cart);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = e.InnerException.InnerException.Message }, JsonRequestBehavior.AllowGet);

                }



                Auth(user.username, user.password);
                return Json(new { success = true, message = "Başarıyla kaydoldunuz", redirectUrl = Url.Action("Index", "Home") }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Hatalı kod girdiniz!" }, JsonRequestBehavior.AllowGet);

            }
        }

        private void ResetConfirmCode()
        {
            Session["ConfirmationCode"] = null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ContactMail(String Name, String Email, String Subject, String Message)
        {


            var inputs = new[]
           {
                    Name,
                    Email,
                    Subject,
                    Message
                };

            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return Json(new { success = false, message = "Tüm alanlar zorunludur." }, JsonRequestBehavior.AllowGet);
                }
            }

            bool isvalidemail = true;
            try
            {
                MailAddress m = new MailAddress(Email);


            }
            catch (FormatException)
            {
                isvalidemail = false;
            }
            if (!isvalidemail)
            {
                return Json(new { success = false, message = "Geçersiz Email adresi" }, JsonRequestBehavior.AllowGet);

            }

            string htmlBody = @"
                <html>
                <head>
                    <meta charset='utf-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                    <title>" + Subject + @"</title>
                    <style>
                        body {
                            font-family: 'Arial', sans-serif;
                            background-color: #f9f9f9;
                            margin: 0;
                            padding: 20px;
                        }
                        .container {
                            max-width: 600px;
                            margin: auto;
                            background: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        }
                        h1 {
                            color: #333;
                            font-size: 24px;
                            margin-bottom: 10px;
                            border-bottom: 2px solid #4CAF50;
                            padding-bottom: 10px;
                        }
                        h3 {
                            color: #4CAF50;
                            font-size: 18px;
                            margin-top: 20px;
                        }
                        p {
                            font-size: 16px;
                            line-height: 1.5;
                            margin: 10px 0;
                        }
                        .footer {
                            margin-top: 20px;
                            font-size: 12px;
                            color: #777;
                            border-top: 1px solid #e0e0e0;
                            padding-top: 10px;
                        }
                        .button {
                            display: inline-block;
                            padding: 10px 15px;
                            font-size: 16px;
                            color: #fff;
                            background-color: #4CAF50;
                            text-decoration: none;
                            border-radius: 5px;
                            margin-top: 20px;
                        }
                        .button:hover {
                            background-color: #45a049;
                        }
                    </style>
                </head>
                <body>
                   
                    <div class='container'>
                        <h1>" + Subject + @"</h1>
                        <p>" + Message + @"</p>
                        <h3>Gönderen: " + Name + @"</h3>
                        <h3>Gönderen Email: " + Email + @"</h3>
                    </div>
                    <div class='footer'>
                        <p>Bu iletişim formundan gönderilmiş bir mesajdır.</p>
                    </div>
                </body>
                </html>";


            _emailService.ContactMail(Subject, htmlBody);
            return Json(new { success = true, message = "Mesajınız iletilmiştir. Teşekkür ederiz." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        [HttpPost]
        // Şifre Doğrulama
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        public ActionResult LogOut()
        {
            Session["User"] = null;
            Session.Abandon();
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");

        }


        public PartialViewResult Search(string Word)
        {
            var results = db.products.Where(x => x.name.Contains(Word) && x.status == true).Take(10).ToList();


            return PartialView("_SearchPartial", results);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Order(String AddressId)
        {
            var user = Session["User"] as users;
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            if (user.phone_number == null)
            {
                return Json(new { success = false, message = "    <div class=\"inline-elements\" style=\"display:flex;align-items:center;\">\r\n                            <span>Hesabınızda telefon numaranız ekli değildir. Lütfen ekleyiniz.</span>\r\n                            <a href=\"/Home/MyAccount/" + user.id + "\" class=\"success-btn\" style=\"background-color:#ffffff;\">Hesabıma Git</a>\r\n                        </div>" }, JsonRequestBehavior.AllowGet);
            }
            var cart = await db.cart.FirstOrDefaultAsync(x => x.user_id == user.id);
            if (cart == null)
            {
                return Json(new { success = false, message = "Sepet bulunamadı" }, JsonRequestBehavior.AllowGet);
            }

            int id = int.Parse(AddressId);


            var address = db.addresses.Find(id);
            if (address == null)
            {
                return Json(new { success = false, message = "Adres bulunamadı." });
            }

            if (address.user_id != user.id)
            {
                return Json(new { success = false, message = "Adres geçersiz." });

            }

            var cartItems = await db.cart_item
                                    .Where(x => x.cart_id == cart.id)
                                    .Include(x => x.products)
                                    .ToListAsync();



            foreach (var item in cartItems)
            {



                if (db.products.Any(x => x.id == item.product_id))
                {
                    var p = db.products.FirstOrDefault(x => x.id == item.product_id);
                    if (p.products_skus.FirstOrDefault().quantity <= 0)
                    {
                        SendProductStockNotification(p, p.name + " isimli ürünün stoğu tükenmiştir.");


                        return Json(new { success = false, message = "Stoklarımız tükenmiştir.(" + p.name + ")" });


                    }

                    if (p.products_skus.FirstOrDefault().quantity < item.quantity)
                    {

                        SendProductStockNotification(p, p.name + " isimli ürünün stoğu azalmıştır.");

                        return Json(new { success = false, message = "Stoklarımızda yeteri kadar ürün kalmamıştır.(" + item.products.name + ")" });
                    }
                }


            }





            decimal? total = 0;


            foreach (var item in cartItems)
            {
                var sku = await db.products_skus.FirstOrDefaultAsync(x => x.product_id == item.product_id);
                if (sku == null)
                {
                    return Json(new { success = false, message = "Ürün SKU'su bulunamadı" }, JsonRequestBehavior.AllowGet);
                }

                total += sku.price * item.quantity;


                //db.products_skus.FirstOrDefault(x => x.product_id == item.product_id).quantity -= item.quantity;
                db.SaveChanges();
            }

            string paidPrice = "";

            if (cart.coupon_id != null)
            {
                if (cart.coupons == null || !cart.coupons.IsActive)
                {
                    cart.coupon_id = null;
                    cart.discount_value = null;
                    db.SaveChanges();
                }
            }
            var minAmount = db.company_details.FirstOrDefault().min_amonunt_for_free_shipping.GetValueOrDefault(300); //Default 300
            if (total >= minAmount)
            {
                cart.isCargoFree = true;
            }
            else
            {
                cart.isCargoFree = false;
            }

            var discountValue = cart.discount_value;
            var shippingPrice = db.company_details.FirstOrDefault().shipping_cost.GetValueOrDefault(30);//Default kargo ücreti 30₺
            if (discountValue == null)
            {
                discountValue = "0";
            }
            else
            {
                discountValue = cart.discount_value.Replace(".", ",");
            }
            if (cart.isCargoFree == true)
            {
                paidPrice = (total - decimal.Parse(discountValue)).ToString();

            }
            else
            {
                paidPrice = (total + shippingPrice - decimal.Parse(discountValue)).ToString();

            }



            Payment(total.ToString(), paidPrice, cart.id.ToString(), user, cartItems, AddressId.ToString());


            //var pd = new payment_details
            //{
            //    created_at = DateTime.Now,
            //    order_id = od.id,
            //    amount = total,
            //    provider = "Mastercard",
            //    status = "Ödendi"
            //};

            //db.payment_details.Add(pd);
            //await db.SaveChangesAsync();

            //od.payment_id = pd.id;
            //await db.SaveChangesAsync();

            //db.cart_item.RemoveRange(cartItems);
            //await db.SaveChangesAsync();

            return Json(new { success = true, message = "Ödeme işlemine geçiliyor." }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]

        public JsonResult AddWishlist(int Id)
        {
            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {
                    if (db.products.Any(x => x.id == Id))
                    {


                        if (!db.wishlist.Any(x => x.product_id == Id && x.user_id == user.id))
                        {
                            wishlist wishlist = new wishlist();
                            wishlist.user_id = user.id;
                            wishlist.product_id = db.products.FirstOrDefault(x => x.id == Id).id;
                            wishlist.created_at = DateTime.Now;
                            db.wishlist.Add(wishlist);
                            db.SaveChanges();

                            var list = db.wishlist.Where(x => x.user_id == user.id).Select(wl => new
                            {

                                wl.user_id,
                                wl.product_id,
                                wl.created_at,
                                wl.id
                            }
                        );

                            return Json(new { success = true, message = db.products.Find(Id).name.ToString() + " Ürünü Başarıyla İstek Listenize Eklendi", list = list });

                        }
                        else
                        {
                            return Json(new { success = false, message = "Bu Ürün İstek Listenizde Zaten Bulunmaktadır." });

                        }




                    }
                    else
                    {
                        return Json(new { success = false, message = "Böyle Bir Ürün Bulunamadı." });

                    }

                }
                else
                {
                    return Json(new { success = false, message = "İstek Listsine Ürün Ekleyebilmek İçin Giriş Yapmalısınız." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult RemoveinWishlist(int Id)
        {
            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {
                    if (db.products.Any(x => x.id == Id))
                    {
                        wishlist wishlist = db.wishlist.FirstOrDefault(x => x.product_id == Id && x.user_id == user.id);

                        if (wishlist != null)
                        {
                            db.wishlist.Remove(wishlist);
                            db.SaveChanges();

                        }
                        else
                        {
                            return Json(new { success = false, message = "İstek Listesinde Böyle Bir Ürün Bulunamadı." });

                        }

                        db.SaveChanges();

                        var list = db.wishlist.Where(x => x.user_id == user.id);

                        return Json(new { success = true, message = db.products.FirstOrDefault(x => x.id == Id).name.ToString() + " Ürünü Başarıyla İstek Listenizden Silindi", list = list });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Böyle Bir Ürün Bulunamadı." });

                    }

                }
                else
                {
                    return Json(new { success = false, message = "İstek Listsine ürün Ekleyebilmek İçin Giriş Yapmalısınız." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }




        [HttpGet]
        public JsonResult GetSubCategories(int categoryId)
        {
            var subCategories = db.sub_categories
                                  .Where(sc => sc.parent_id == categoryId)
                                  .Select(sc => new
                                  {
                                      sc.id,
                                      sc.name
                                  })
                                  .ToList();

            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubSubCategories(int subcategoryId)
        {
            var subsubCategories = db.sub_subcategories
                                  .Where(sc => sc.parent_sub_category_id == subcategoryId)
                                  .Select(sc => new
                                  {
                                      sc.id,
                                      sc.name
                                  })
                                  .ToList();

            return Json(subsubCategories, JsonRequestBehavior.AllowGet);
        }





        [HttpGet]
        public JsonResult GetCategories()
        {

            var Categories = db.categories
                                  .Select(c => new
                                  {
                                      c.id,
                                      c.name,
                                      c.description
                                  })
                                  .ToList();

            return Json(Categories, JsonRequestBehavior.AllowGet);
        }




        public ActionResult deneme()
        {



            return View();
        }
        [HttpPost]
        public JsonResult UseCoupon(string CouponCode)
        {
            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {
                    var coupon = db.coupons.FirstOrDefault(x => x.Code == CouponCode && x.Status == true && x.IsActive == true);
                    if (coupon != null)
                    {
                        var userCart = db.cart.FirstOrDefault(x => x.user_id == user.id);
                        if (userCart != null)
                        {
                            var couponProductIds = db.coupon_products
                                                    .Where(x => x.coupon_id == coupon.Id)
                                                    .Select(x => x.product_id)
                                                    .ToList();

                            var couponCategoryIds = db.coupon_categories
                                                    .Where(x => x.coupon_id == coupon.Id)
                                                    .Select(x => x.category_id)
                                                    .ToList();

                            var couponSubCategoryIds = db.coupon_categories
                                                    .Where(x => x.coupon_id == coupon.Id)
                                                    .Select(x => x.subcategory_id)
                                                    .ToList();

                            var cartProductIds = userCart.cart_item
                                                    .Select(x => x.product_id)
                                                    .Where(x => x.HasValue)
                                                    .Select(x => x.Value)
                                                    .ToList();

                            var cartCategoryIds = db.products
                                        .Where(p => cartProductIds.Contains(p.id))
                                        .Select(p => p.sub_categories.parent_id)
                                        .Where(c => c.HasValue)
                                        .Select(c => c.Value)
                                        .ToList();

                            var cartSubCategoryIds = db.products
                                                    .Where(p => cartProductIds.Contains(p.id))
                                                    .Select(p => p.subcategory_id)
                                                    .Where(sc => sc.HasValue)
                                                    .Select(sc => sc.Value)
                                                    .ToList();


                            bool isProductEligible = cartProductIds.Any(cartProductId => couponProductIds.Contains(cartProductId));
                            bool isCategoryEligible = cartCategoryIds.Any(cartCategoryId => couponCategoryIds.Contains(cartCategoryId));
                            bool isSubCategoryEligible = cartSubCategoryIds.Any(cartSubCategoryId => couponSubCategoryIds.Contains(cartSubCategoryId));

                            bool isEligible = isProductEligible || isCategoryEligible || isSubCategoryEligible;

                            if (isProductEligible)
                            {

                                var productIds = cartProductIds.Where(cartProductId => couponProductIds.Contains(cartProductId));

                                decimal? total = 0;
                                decimal? indirimTutarı = 0;
                                foreach (var item in userCart.cart_item.Where(x => productIds.Contains(x.product_id.Value)))
                                {
                                    var product = db.products.Include(x => x.products_skus).FirstOrDefault(x => x.id == item.product_id);

                                    total += product.products_skus.FirstOrDefault().price * item.quantity;


                                }
                                if (coupon.MinimumSpend != null)
                                {

                                    if (total >= coupon.MinimumSpend)
                                    {

                                        if (coupon.MaxDiscountAmount == null)
                                        {
                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }


                                        }
                                        else
                                        {

                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }

                                                if (indirimTutarı > total)
                                                {
                                                    indirimTutarı = total;
                                                }
                                            }
                                        }
                                        ViewBag.Discount = indirimTutarı;
                                        Session["ActiveCoupon"] = coupon;
                                        userCart.coupon_id = coupon.Id;
                                        userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                        db.SaveChanges();

                                        Session["CartsForPartial"] = db.cart.ToList();
                                        Session["CouponsForPartial"] = db.coupons.ToList();
                                        ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                        var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                        string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);

                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = indirimTutarı, coupon = coupon.Code, cartItemsHtml = cartItemsHtml });
                                    }
                                    else
                                    {
                                        Session["ActiveCoupon"] = null;
                                        return Json(new { success = false, message = "Kupon için gereken minimum tutar karşılanmıyor!" });

                                    }

                                }
                                else
                                {
                                    if (coupon.MaxDiscountAmount == null)
                                    {
                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                        }

                                        if (indirimTutarı > total)
                                        {
                                            indirimTutarı = total;
                                        }

                                    }
                                    else
                                    {

                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }
                                        }
                                    }
                                    ViewBag.Discount = indirimTutarı;
                                    Session["ActiveCoupon"] = coupon;
                                    userCart.coupon_id = coupon.Id;
                                    userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                    db.SaveChanges();
                                    Session["CartsForPartial"] = db.cart.ToList();
                                    Session["CouponsForPartial"] = db.coupons.ToList();
                                    ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                    var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                    string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);
                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = indirimTutarı, coupon = coupon.Code, cartItemsHtml = cartItemsHtml });

                                }
                            }
                            else if (isCategoryEligible)
                            {
                                var categoryIds = cartCategoryIds.Where(cartCategoryId => couponCategoryIds.Contains(cartCategoryId));

                                decimal? total = 0;
                                decimal? indirimTutarı = 0;
                                foreach (var item in userCart.cart_item
                                  .Where(x => x.products != null &&
                                              x.products.sub_categories != null &&
                                              x.products.sub_categories.parent_id.HasValue &&
                                              categoryIds.Contains(x.products.sub_categories.parent_id.Value)))
                                {
                                    var product = db.products.Include(x => x.products_skus).FirstOrDefault(x => x.id == item.product_id);

                                    total += product.products_skus.FirstOrDefault().price * item.quantity;


                                }
                                if (coupon.MinimumSpend != null)
                                {

                                    if (total >= coupon.MinimumSpend)
                                    {

                                        if (coupon.MaxDiscountAmount == null)
                                        {
                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }


                                        }
                                        else
                                        {

                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }

                                                if (indirimTutarı > total)
                                                {
                                                    indirimTutarı = total;
                                                }
                                            }
                                        }
                                        ViewBag.Discount = indirimTutarı;
                                        Session["ActiveCoupon"] = coupon;
                                        userCart.coupon_id = coupon.Id;
                                        userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                        db.SaveChanges();
                                        Session["CartsForPartial"] = db.cart.ToList();
                                        Session["CouponsForPartial"] = db.coupons.ToList();
                                        ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                        var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                        string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);

                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code, cartItemsHtml = cartItemsHtml });
                                    }
                                    else
                                    {
                                        Session["ActiveCoupon"] = null;

                                        return Json(new { success = false, message = "Kupon için gereken minimum tutar karşılanmıyor!" });

                                    }

                                }
                                else
                                {
                                    if (coupon.MaxDiscountAmount == null)
                                    {
                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                        }

                                        if (indirimTutarı > total)
                                        {
                                            indirimTutarı = total;
                                        }

                                    }
                                    else
                                    {

                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }
                                        }
                                    }
                                    ViewBag.Discount = indirimTutarı;
                                    Session["ActiveCoupon"] = coupon;
                                    userCart.coupon_id = coupon.Id;
                                    userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                    db.SaveChanges();
                                    Session["CartsForPartial"] = db.cart.ToList();
                                    Session["CouponsForPartial"] = db.coupons.ToList();
                                    ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                    var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                    string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);

                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code, cartItemsHtml = cartItemsHtml });

                                }

                            }
                            else if (isSubCategoryEligible)
                            {
                                var categoryIds = cartSubCategoryIds.Where(cartSubCategoryId => couponSubCategoryIds.Contains(cartSubCategoryId));

                                decimal? total = 0;
                                decimal? indirimTutarı = 0;
                                foreach (var item in userCart.cart_item.Where(x => x.products.subcategory_id.HasValue && categoryIds.Contains(x.products.subcategory_id.Value)))

                                {
                                    var product = db.products.Include(x => x.products_skus).FirstOrDefault(x => x.id == item.product_id);

                                    total += product.products_skus.FirstOrDefault().price * item.quantity;


                                }
                                if (coupon.MinimumSpend != null)
                                {

                                    if (total >= coupon.MinimumSpend)
                                    {

                                        if (coupon.MaxDiscountAmount == null)
                                        {
                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }


                                        }
                                        else
                                        {

                                            if (coupon.DiscountType == "Yüzde")
                                            {
                                                indirimTutarı = total * (coupon.DiscountValue / 100);
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }
                                            }
                                            else
                                            {
                                                indirimTutarı = coupon.DiscountValue;
                                                if (indirimTutarı > coupon.MaxDiscountAmount)
                                                {
                                                    indirimTutarı = coupon.MaxDiscountAmount;
                                                }

                                                if (indirimTutarı > total)
                                                {
                                                    indirimTutarı = total;
                                                }
                                            }
                                        }
                                        ViewBag.Discount = indirimTutarı;
                                        Session["ActiveCoupon"] = coupon;
                                        userCart.coupon_id = coupon.Id;
                                        userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                        db.SaveChanges();
                                        Session["CartsForPartial"] = db.cart.ToList();
                                        Session["CouponsForPartial"] = db.coupons.ToList();
                                        ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                        var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                        string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);
                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code, cartItemsHtml = cartItemsHtml });
                                    }
                                    else
                                    {
                                        Session["ActiveCoupon"] = null;

                                        return Json(new { success = false, message = "Kupon için gereken minimum tutar karşılanmıyor!" });

                                    }

                                }
                                else
                                {
                                    if (coupon.MaxDiscountAmount == null)
                                    {
                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                        }

                                        if (indirimTutarı > total)
                                        {
                                            indirimTutarı = total;
                                        }

                                    }
                                    else
                                    {

                                        if (coupon.DiscountType == "Yüzde")
                                        {
                                            indirimTutarı = total * (coupon.DiscountValue / 100);
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }
                                        }
                                        else
                                        {
                                            indirimTutarı = coupon.DiscountValue;
                                            if (indirimTutarı > coupon.MaxDiscountAmount)
                                            {
                                                indirimTutarı = coupon.MaxDiscountAmount;
                                            }

                                            if (indirimTutarı > total)
                                            {
                                                indirimTutarı = total;
                                            }
                                        }
                                    }
                                    ViewBag.Discount = indirimTutarı;
                                    Session["ActiveCoupon"] = coupon;
                                    userCart.coupon_id = coupon.Id;
                                    userCart.discount_value = indirimTutarı.ToString().Replace(",", ".");
                                    db.SaveChanges();
                                    Session["CartsForPartial"] = db.cart.ToList();
                                    Session["CouponsForPartial"] = db.coupons.ToList();
                                    ViewBag.CompanyDetails = db.company_details.FirstOrDefault();

                                    var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == userCart.id).ToList());
                                    string cartItemsHtml = RenderPartialViewToString(this.ControllerContext, "_CartPartial", cartItemsPartial.Model);
                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code, cartItemsHtml = cartItemsHtml });

                                }

                            }
                            else
                            {
                                Session["ActiveCoupon"] = null;

                                return Json(new { success = false, message = "Sepetinizdeki ürünler bu kupon için uygun değil." });
                            }
                        }
                        else
                        {
                            Session["ActiveCoupon"] = null;

                            return Json(new { success = false, message = "Kullanıcı sepeti bulunamadı." });
                        }
                    }
                    else
                    {
                        Session["ActiveCoupon"] = null;

                        return Json(new { success = false, message = "Geçersiz kupon kodu." });
                    }
                }
                else
                {
                    Session["ActiveCoupon"] = null;

                    return Json(new { success = false, message = "Kullanıcı oturumu geçersiz." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
        [HttpGet]
        public ActionResult Payment()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();



            ViewBag.CompanyDetails = db.company_details.FirstOrDefault();




            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Payment(String Price, String PaidPrice, String BasketId, users User, List<cart_item> cart_items, String AddressId)
        {


            int id = int.Parse(AddressId);


            var address = db.addresses.Find(id);
            if (address == null)
            {
                return Json(new { success = false, message = "Adres bulunamadı." });
            }
            Session["AdressId"] = id.ToString();

            Options options = new Options();
            options.ApiKey = "sandbox-lfDKd5dEcP9SvjEbRdOaMGX5LOYVcYgO"; //Iyzico Tarafından Sağlanan Api Key
            options.SecretKey = "G4GKghvkujw7YYchDECfiW6MzhfTLhsq"; //Iyzico Tarafından Sağlanan Secret Key
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";

            request.Currency = Currency.TRY.ToString();
            request.BasketId = BasketId;
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Url.Content("~");
            //request.CallbackUrl = baseUrl + "/Home/Sonuc?UserId=" + User.id;
            request.CallbackUrl = "https://cobanlarmarket.com/Home/Sonuc?UserId=" + User.id + "&AddressId=" + AddressId;

            //List<int> enabledInstallments = new List<int>();
            //enabledInstallments.Add(2);
            //enabledInstallments.Add(3);
            //enabledInstallments.Add(6);
            //enabledInstallments.Add(9);
            //request.EnabledInstallments = enabledInstallments;


            string hostName = Dns.GetHostName();
            Console.WriteLine(hostName);

            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine("My IP Address is :" + myIP);

            Buyer buyer = new Buyer();
            buyer.Id = User.id.ToString();
            buyer.Name = User.first_name.ToString();
            buyer.Surname = User.last_name.ToString();
            buyer.GsmNumber = User.phone_number.IsNullOrWhiteSpace() ? address.phone_number : User.phone_number;
            buyer.Email = User.email.ToString();
            buyer.IdentityNumber = "12345678911";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            DateTime createdAtDateTime = Convert.ToDateTime(User.created_at);
            string formattedDate = createdAtDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            buyer.RegistrationDate = formattedDate;
            buyer.RegistrationAddress = address.address + " " + address.district + "/" + address.city;
            buyer.Ip = myIP;
            buyer.City = address.city;
            buyer.Country = address.country;
            buyer.ZipCode = address.postal_code;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = User.first_name.ToString() + " " + User.last_name.ToString();
            shippingAddress.City = address.city;
            shippingAddress.Country = address.country;
            shippingAddress.Description = address.address;
            shippingAddress.ZipCode = address.postal_code;
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = User.first_name.ToString() + " " + User.last_name.ToString();
            billingAddress.City = address.city;
            billingAddress.Country = address.country;
            billingAddress.Description = address.address;
            billingAddress.ZipCode = address.postal_code;
            request.BillingAddress = billingAddress;


            List<BasketItem> basketItems = new List<BasketItem>();



            decimal? toplam = 0;
            foreach (var item in cart_items)
            {
                var product = db.products.FirstOrDefault(x => x.id == item.product_id);
                BasketItem basketItem = new BasketItem();
                basketItem.Id = item.id.ToString();
                basketItem.Name = db.products.FirstOrDefault(x => x.id == item.product_id).name;


                if (db.products.FirstOrDefault(x => x.id == item.product_id).subcategory_id != null)
                {

                    basketItem.Category2 = product.sub_categories.name.ToString();
                    basketItem.Category1 = db.categories.FirstOrDefault(x => x.id == db.sub_categories.FirstOrDefault(y => y.id == product.subcategory_id).parent_id).name.ToString();

                }
                else
                {
                    basketItem.Category1 = "null";
                    basketItem.Category2 = "null";
                }
                basketItem.ItemType = BasketItemType.PHYSICAL.ToString();
                basketItem.Price = (db.products_skus.FirstOrDefault(x => x.product_id == product.id).price * item.quantity).ToString().Replace(",", ".");
                basketItems.Add(basketItem);

                toplam += decimal.Parse(basketItem.Price.Replace(".", ","));
            }

            request.Price = Price.ToString().Replace(",", ".");
            request.PaidPrice = PaidPrice.ToString().Replace(",", ".");
            request.BasketItems = basketItems;
            CheckoutFormInitialize checkoutFormInitialize = await CheckoutFormInitialize.Create(request, options);

            TempData["Iyzico"] = checkoutFormInitialize.CheckoutFormContent;

            return Json(new { success = true, message = "Ödeme işlemine geçiliyor." });

        }



        [HttpPost]
        public async Task<ActionResult> Sonuc(RetrieveCheckoutFormRequest model, string UserId, string AddressId)
        {


            // Gelen POST isteğinde Token'i al
            string token = model.Token;

            if (string.IsNullOrEmpty(token))
            {
                // Eğer Token boş geliyorsa, hata döndür
                ViewBag.ErrorMessage = "Ödeme işlemi başarısız oldu.";
                return View();
            }

            // İyzico API'ye bu Token ile ödeme sonucunu sorgula
            Options options = new Options();
            options.ApiKey = "sandbox-lfDKd5dEcP9SvjEbRdOaMGX5LOYVcYgO";
            options.SecretKey = "G4GKghvkujw7YYchDECfiW6MzhfTLhsq";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
            request.Token = token;

            // Ödeme sonucunu iyzico'dan alıyoruz
            CheckoutForm checkoutForm = await CheckoutForm.Retrieve(request, options);

            order_details order = new order_details();
            if (checkoutForm.PaymentStatus == "SUCCESS")
            {
                var uidd = int.Parse(UserId);

                var userr = db.users.FirstOrDefault(x => x.id == uidd);

                var cartt = await db.cart.FirstOrDefaultAsync(x => x.user_id == userr.id);

                var cartItemss = await db.cart_item
                                     .Where(x => x.cart_id == cartt.id)
                                     .Include(x => x.products)
                                     .ToListAsync();
                foreach (var item in cartItemss)
                {
                    // Ürün kontrolü
                    var product = db.products.Include(x => x.products_skus)
                                             .FirstOrDefault(x => x.id == item.product_id);

                    if (product != null)
                    {
                        var sku = product.products_skus.FirstOrDefault();

                        // SKU bulunamadığında veya stok 0 veya daha az ise veya istenilen miktarda stok yoksa
                        if (sku == null || sku.quantity <= 0 || sku.quantity < item.quantity)
                        {
                            // Cancel işlemi başlat
                            string hostName = Dns.GetHostName();
                            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                            var cancelRequest = new CreateCancelRequest
                            {
                                Locale = Locale.TR.ToString(),
                                ConversationId = checkoutForm.ConversationId,
                                PaymentId = checkoutForm.PaymentId,
                                Ip = myIP
                            };

                            var cancelResponse = await Iyzipay.Model.Cancel.Create(cancelRequest, options);

                            if (cancelResponse.Status.ToString() == "success")
                            {

                                SendProductStockNotification(product, product.name + " isimli ürünün stoğu yeterli olmadığı için " + userr.username + " kullanıcı adlı kullanıcının siparişi iptal edilmiştir.");


                                return Json(new { success = false, message = "Yeterli stok bulunmadığından ödeme iptal edildi. (" + product.name + ")" });
                            }
                            else
                            {
                                // Cancel başarısızsa Refund işlemi başlat
                                var refundRequest = new CreateRefundRequest
                                {
                                    Locale = Locale.TR.ToString(),
                                    ConversationId = checkoutForm.ConversationId,
                                    PaymentTransactionId = checkoutForm.PaymentId,
                                    Price = checkoutForm.PaidPrice,
                                    Currency = Currency.TRY.ToString(),
                                    Ip = myIP
                                };

                                var refundResponse = await Iyzipay.Model.Refund.Create(refundRequest, options);

                                if (refundResponse.Status.ToString() == "success")
                                {
                                    SendProductStockNotification(product, product.name + " isimli ürünün stoğu yeterli olmadığı için " + userr.username + " kullanıcı adlı kullanıcının siparişi iade edilmiştir.");

                                    return Json(new { success = false, message = "Yeterli stok bulunmadığından ödeme iade edildi. (" + product.name + ")" });
                                }
                                else
                                {
                                    return Json(new { success = false, message = "İptal ve iade işlemleri başarısız oldu: " + refundResponse.ErrorMessage });
                                }
                            }
                        }
                    }
                }











                // Ödeme başarılı ise kullanıcıya bilgi gösterin
                ViewBag.PaymentStatus = "SUCCESS";
                ViewBag.PaidPrice = checkoutForm.PaidPrice;
                ViewBag.PaymentId = checkoutForm.PaymentId;
                ViewBag.BasketId = checkoutForm.BasketId;
                ViewBag.Installment = checkoutForm.Installment;

                var uid = int.Parse(UserId);

                var user = db.users.FirstOrDefault(x => x.id == uid);

                if (user != null)
                {
                    var cart = await db.cart.FirstOrDefaultAsync(x => x.user_id == user.id);


                    var address_id = int.Parse(AddressId);
                    var od = new order_details
                    {
                        user_id = user.id,
                        address_id = address_id,
                        created_at = DateTime.Now,
                        coupon_id = cart.coupon_id != null ? cart.coupon_id : null,
                        is_delivered = false
                    };
                    Session["AdressId"] = null;
                    db.order_details.Add(od);
                    await db.SaveChangesAsync();

                    var orderItems = new List<order_item>();
                    var cartItems = await db.cart_item
                                      .Where(x => x.cart_id == cart.id)
                                      .Include(x => x.products)
                                      .ToListAsync();

                    decimal? total = 0;



                    foreach (var item in cartItems)
                    {
                        var sku = await db.products_skus.FirstOrDefaultAsync(x => x.product_id == item.product_id);
                        if (sku == null)
                        {
                            return Json(new { success = false, message = "Ürün SKU'su bulunamadı" }, JsonRequestBehavior.AllowGet);
                        }

                        total += sku.price * item.quantity;

                        orderItems.Add(new order_item
                        {
                            order_id = od.id,
                            product_id = item.product_id,
                            quantity = item.quantity,
                            ordered_price = sku.price,
                            payment_transaction_id = checkoutForm.PaymentItems.Where(x => x.ItemId == item.id.ToString()).FirstOrDefault().PaymentTransactionId,
                            status = "success",
                            created_at = DateTime.Now
                        });
                        db.products_skus.FirstOrDefault(x => x.product_id == item.product_id).quantity -= item.quantity;
                        db.SaveChanges();
                    }


                    db.order_item.AddRange(orderItems);
                    od.total = total;
                    var address = db.addresses.FirstOrDefault(x => x.id == address_id);
                    od.shipping_title = address.title;
                    od.shipping_address = address.address;
                    od.shipping_city = address.city;
                    od.shipping_country = address.country;
                    od.shipping_district = address.district;
                    od.shipping_quarter = address.quarter;
                    od.shipping_name = address.name;
                    od.shipping_surname = address.surname;
                    od.shipping_phone_number = address.phone_number;
                    od.shipping_postal_code = address.postal_code;

                    var pd = new payment_details
                    {
                        created_at = DateTime.Now,
                        order_id = od.id,
                        amount = decimal.Parse(checkoutForm.Price.Replace(".", ",")),
                        provider = checkoutForm.CardAssociation,
                        status = checkoutForm.Status,
                        installment = checkoutForm.Installment,
                        cardFamily = checkoutForm.CardFamily,
                        cardType = checkoutForm.CardType,
                        paidPrice = decimal.Parse(checkoutForm.PaidPrice.Replace(".", ",")),
                        paymentId = checkoutForm.PaymentId,
                        cargoPrice = cart.isCargoFree == true ? 0 : db.company_details.FirstOrDefault().shipping_cost.GetValueOrDefault(30),//Default kargo ücreti 30₺
                        couponId = cart.coupon_id != null ? cart.coupon_id : null,
                        couponDiscountValue = cart.discount_value != null ? decimal.Parse(cart.discount_value.Replace(".", ",")) : 0



                    };





                    db.payment_details.Add(pd);
                    await db.SaveChangesAsync();

                    od.payment_id = pd.id;
                    await db.SaveChangesAsync();

                    db.cart_item.RemoveRange(cartItems);
                    await db.SaveChangesAsync();

                    cart.coupon_id = null;
                    cart.discount_value = null;
                    cart.isCargoFree = false;
                    cart.total = 0;
                    db.SaveChanges();

                    var orderItemsProjection = orderItems.Select(c => new
                    {
                        c.id,
                        c.product_id,
                        c.products_sku_id,
                        c.order_id,
                        c.payment_transaction_id,
                        c.quantity,
                        orderDetails = new
                        {
                            c.order_details.id,
                            c.order_details.total,
                            created_at = c.order_details.created_at.HasValue
                     ? c.order_details.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss")
                     : "N/A",
                            User = new
                            {
                                c.order_details.users.first_name,
                                c.order_details.users.email,
                                c.order_details.users.username

                            },
                            payment_details = new
                            {
                                c.status
                            }

                        },
                        products = new
                        {
                            c.products.cover,
                            c.products.name,
                            c.products.products_skus.FirstOrDefault().price
                        }
                    }).ToList();

                    List<payment_details> paydet = new List<payment_details>();
                    List<order_details> ordet = new List<order_details>();
                    paydet.Add(pd);
                    ordet.Add(od);

                    var paydetProjection = paydet.Select(p => new
                    {


                        pd.id,
                        pd.amount,
                        pd.paidPrice,
                        pd.order_id,
                        pd.status,
                        pd.created_at,
                        User = new
                        {
                            pd.order_details.users.first_name,
                            pd.order_details.users.email
                        }
                    }).ToList();



                    var ordetProjection = ordet.Select(o => new
                    {
                        pd.id,
                        pd.amount,
                        pd.order_id,
                        pd.status,
                        pd.created_at,
                        User = new
                        {
                            pd.order_details.users.first_name,
                            pd.order_details.users.email
                        }
                    }).ToList();
                    db.notification.Add(new notification
                    {
                        is_read = false,
                        status = true,
                        title = "Yeni Sipariş",
                        text = user.first_name + " " + user.last_name + " tarafından " + Math.Round((decimal)pd.paidPrice, 2) + " &#8378;'lik sipariş geldi!",
                        user_id = user.id,
                        order_id = od.id,
                        type = "order",
                        campaign_id = 0,
                        product_id = 0
                    });

                    db.SaveChanges();
                    var notificationProjection = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
                    {
                        c.id,
                        c.user_id,
                        c.order_id,
                        c.text,
                        c.title,
                        c.is_read,
                        c.status,
                        c.type,
                        c.campaign_id,
                        c.product_id,
                        User = new
                        {
                            c.users.first_name,
                            c.users.last_name,
                            c.users.id,
                            c.users.avatar
                        }

                    });

                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                    hubContext.Clients.All.receiveNotification("Yeni bir sipariş geldi!", orderItemsProjection, paydetProjection, ordetProjection, notificationProjection);



                    order = od;
                }



                //_emailService.SendInfoEmail(user.email, "Siparişini aldık", htmlBody);

                _emailService.SendInfoEmail(user.email, "Siparişini aldık", order.id);


                Session["User"] = db.users.FirstOrDefault(x => x.id == order.user_id);

                return RedirectToAction("SiparisDetay", new { orderId = order.id });

            }

            // Ödeme başarısızsa hata mesajı gösterin
            ViewBag.PaymentStatus = "FAILED";


            return View("Sonuc");
        }

        public ActionResult SiparisDetay(int orderId)
        {
            string page = $"/{Request.RequestContext.RouteData.Values["controller"]}/{Request.RequestContext.RouteData.Values["action"]}";
            LoadSeoSettingsToViewBag(page);

            var user = Session["User"] as users;
            if (user == null)
            {
                return RedirectToAction("Auth");
            }

            var order = db.order_details.FirstOrDefault(x => x.id == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }
            if (order.user_id != user.id)
            {
                return HttpNotFound();
            }


            AllViewModel model = new AllViewModel();
            model.order_details = db.order_details
                .Where(x => x.id == orderId)
                .Include(x => x.order_item)
                .ToList();
            model.order_item = db.order_item
                .Where(x => x.order_id == orderId)
                .Include(x => x.products)
                .Include(x => x.products_skus)
                .ToList();
            model.payment_details = db.payment_details.Where(x => x.order_id == orderId).Include(x => x.coupons).ToList();
            model.users = db.users.Where(x => x.id == order.user_id).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View("Sonuc", model);
        }

        [HttpPost]
        public async Task<ActionResult> Webhook()
        {
            // Webhook bildirimlerini alın
            var jsonString = await new StreamReader(Request.InputStream).ReadToEndAsync();

            if (!string.IsNullOrEmpty(jsonString))
            {
                // Gelen JSON verisini deserialize ederek işleyin
                dynamic jsonWebhook = JsonConvert.DeserializeObject(jsonString);

                string iyziEventType = jsonWebhook.iyziEventType;
                string paymentId = jsonWebhook.iyziPaymentId;
                string token = jsonWebhook.token;
                string status = jsonWebhook.status;
                string referenceCode = jsonWebhook.iyziReferenceCode;
                string conversationId = jsonWebhook.paymentConversationId;





                // İyzico API ayarları
                Options options = new Options();
                options.ApiKey = "sandbox-lfDKd5dEcP9SvjEbRdOaMGX5LOYVcYgO";
                options.SecretKey = "G4GKghvkujw7YYchDECfiW6MzhfTLhsq";
                options.BaseUrl = "https://sandbox-api.iyzipay.com";


                // Transaction raporu almak için istek oluştur
                RetrieveTransactionReportRequest request = new RetrieveTransactionReportRequest
                {
                    TransactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // Güncel tarih
                    Page = 1
                };

                // API'den Transaction raporunu al
                TransactionReport transactionReport = TransactionReport.Retrieve(request, options);

                if (transactionReport.Transactions != null)
                {
                    // `paymentId` ile ilgili işlemi bul
                    var transaction = transactionReport.Transactions.FirstOrDefault(t => t.PaymentId.ToString() == paymentId);

                    if (transaction != null)
                    {
                        // İşlem türünü kontrol et
                        if (transaction.TransactionType == "CANCEL")
                        {
                            Console.WriteLine($"İptal işlemi tespit edildi: {paymentId}");



                            var p = db.payment_details.FirstOrDefault(x => x.paymentId == paymentId);
                            p.status = "İade";


                            foreach (var item in db.order_item.Where(x => x.order_id == p.order_id))
                            {
                                item.status = "iade";
                                item.products.products_skus.FirstOrDefault().quantity += item.quantity;
                            }

                            db.SaveChanges();


                        }
                        else if (transaction.TransactionType == "REFUND")
                        {
                            Console.WriteLine($"İade işlemi tespit edildi: {paymentId}");
                            var p = db.payment_details.FirstOrDefault(x => x.paymentId == paymentId);
                            p.status = "İade";


                            foreach (var item in db.order_item.Where(x => x.order_id == p.order_id))
                            {
                                item.status = "iade";
                                item.products.products_skus.FirstOrDefault().quantity += item.quantity;

                            }

                            db.SaveChanges();
                        }
                        else
                        {
                            Console.WriteLine($"Başka bir işlem türü: {transaction.TransactionType}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Belirtilen paymentId ile işlem bulunamadı.");
                    }
                }
                else
                {
                    Console.WriteLine("İşlem raporu alınamadı: " + transactionReport.ErrorMessage);
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddAdress(string Name, string Surname, string Phone, string Title, string Address, string il, string ilce, string mahalle)
        {

            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {




                    var iller = LoadData();

                    // Gönderilen 'il' bilgisini bul
                    var selectedIl = iller.FirstOrDefault(i => i.Name == il);

                    if (selectedIl == null)
                    {
                        return Json(new { success = false, message = "Geçersiz il seçimi." });
                    }

                    // Gönderilen 'ilçe' bilgisini bul
                    var selectedIlce = selectedIl.Towns.FirstOrDefault(t => t.Name == ilce);

                    if (selectedIlce == null)
                    {
                        return Json(new { success = false, message = "Seçilen ile ait geçersiz ilçe." });
                    }

                    // Gönderilen 'mahalle' bilgisini bul
                    var selectedMahalle = selectedIlce.Districts
                        .Where(d => d.Name != "Köyler") // "Köyler" hariç
                        .SelectMany(d => d.Quarters)
                        .FirstOrDefault(q => q.Name == mahalle);

                    if (selectedMahalle == null)
                    {
                        return Json(new { success = false, message = "Seçilen ilçe için geçersiz mahalle." });
                    }


                    var address = new addresses();
                    address.user_id = user.id;
                    address.name = Name;
                    address.surname = Surname;
                    address.title = Title;
                    address.phone_number = Phone;
                    address.address = Address;
                    address.country = "Türkiye";
                    address.city = il;
                    address.district = ilce;
                    address.quarter = mahalle;
                    address.postal_code = "00000";
                    address.created_at = DateTime.Now;

                    db.addresses.Add(address);
                    db.SaveChanges();

                    var list = db.addresses.Where(x => x.user_id == user.id).Select(x => new
                    {
                        x.id,
                        x.title,
                        x.name,
                        x.surname,
                        x.phone_number,
                        x.country,
                        x.city,
                        x.district,
                        x.quarter,
                        x.address
                    }).ToList();

                    return Json(new { success = true, message = "Adres Başarıyla Eklendi", adressList = list }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı Bulunamadı" }, JsonRequestBehavior.AllowGet);
                }



            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Adres Eklenirken Bir Hata Oluştu" }, JsonRequestBehavior.AllowGet);

                throw;
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditAdress(int Id, string Name, string Surname, string Phone, string Title, string Address, string il, string ilce, string mahalle)
        {

            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {

                    if (!db.addresses.Any(x => x.id == Id))
                    {
                        return Json(new { success = false, message = "Adres Bulunamadı" }, JsonRequestBehavior.AllowGet);
                    }




                    var iller = LoadData();

                    // Gönderilen 'il' bilgisini bul
                    var selectedIl = iller.FirstOrDefault(i => i.Name == il);

                    if (selectedIl == null)
                    {
                        return Json(new { success = false, message = "Geçersiz il seçimi." });
                    }

                    // Gönderilen 'ilçe' bilgisini bul
                    var selectedIlce = selectedIl.Towns.FirstOrDefault(t => t.Name == ilce);

                    if (selectedIlce == null)
                    {
                        return Json(new { success = false, message = "Seçilen ile ait geçersiz ilçe." });
                    }

                    // Gönderilen 'mahalle' bilgisini bul
                    var selectedMahalle = selectedIlce.Districts
                        .Where(d => d.Name != "Köyler") // "Köyler" hariç
                        .SelectMany(d => d.Quarters)
                        .FirstOrDefault(q => q.Name == mahalle);

                    if (selectedMahalle == null)
                    {
                        return Json(new { success = false, message = "Seçilen ilçe için geçersiz mahalle." });
                    }





                    var address = db.addresses.Find(Id);
                    address.name = Name;
                    address.surname = Surname;
                    address.title = Title;
                    address.phone_number = Phone;
                    address.address = Address;
                    address.country = "Türkiye";
                    address.city = il;
                    address.district = ilce;
                    address.quarter = mahalle;
                    address.postal_code = "00000";


                    db.SaveChanges();

                    var list = db.addresses.Where(x => x.user_id == user.id).Select(x => new
                    {
                        x.id,
                        x.title,
                        x.name,
                        x.surname,
                        x.phone_number,
                        x.country,
                        x.city,
                        x.district,
                        x.quarter,
                        x.address
                    }).ToList();

                    return Json(new { success = true, message = "Adres Başarıyla Düzenlendi", adressList = list }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı Bulunamadı" }, JsonRequestBehavior.AllowGet);
                }



            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Adres Düzenlenirken Bir Hata Oluştu" }, JsonRequestBehavior.AllowGet);

                throw;
            }



        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveAddress(int Id)
        {
            var address = db.addresses.Find(Id);
            var user = Session["User"] as users;

            if (address == null)
            {

                return Json(new { success = false, message = "Adres Bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            else if (user != null)
            {
                db.addresses.Remove(address);
                db.SaveChanges();

                var list = db.addresses.Where(x => x.user_id == user.id).Select(x => new
                {
                    x.id,
                    x.title,
                    x.name,
                    x.surname,
                    x.phone_number,
                    x.country,
                    x.city,
                    x.district,
                    x.quarter,
                    x.address
                }).ToList();
                return Json(new { success = true, message = "Adres Kaldırıldı", adressList = list }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Adres kaldırılırken bir hata oluştu." });
            }


        }
        [HttpPost]
        public JsonResult GetAddress(int Id)
        {
            var address = db.addresses.Find(Id);
            var user = Session["User"] as users;

            if (address == null)
            {

                return Json(new { success = false, message = "Adres Bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            else if (user != null)
            {


                var list = db.addresses.Where(x => x.id == Id).Select(x => new
                {
                    x.id,
                    x.title,
                    x.name,
                    x.surname,
                    x.phone_number,
                    x.country,
                    x.city,
                    x.district,
                    x.quarter,
                    x.address
                }).ToList();
                return Json(new { success = true, message = "Adres Seçildi", adressList = list }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }


        }

        public ActionResult GetShippingCost()
        {

            var shippingCost = db.company_details.FirstOrDefault().shipping_cost.GetValueOrDefault(30);//Default kargo ücreti 30₺
            return Content(shippingCost.ToString());
        }








        private List<City> LoadData()
        {
            var filePath = Server.MapPath("~/wwwroot/LocationData.json");
            var json = System.IO.File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<City>>(json);
        }

        [HttpGet]
        public JsonResult GetIller()
        {
            var iller = LoadData();
            return Json(iller, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetIlceler(string il)
        {
            // Artık LoadData List<City> döndürüyor.
            var iller = LoadData();
            var ilceler = iller.FirstOrDefault(i => i.Name == il)?.Towns
                .Select(t => new { t.Name }) // Towns içinde ilçe isimlerini alıyoruz.
                .ToList();

            return Json(ilceler, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetMahalleler(string il, string ilce)
        {
            // Artık LoadData List<City> döndürüyor.
            var iller = LoadData();
            var mahalleler = iller.FirstOrDefault(i => i.Name == il)
                ?.Towns.FirstOrDefault(t => t.Name == ilce)
                ?.Districts.Where(d => d.Name != "Köyler")
                .SelectMany(d => d.Quarters)
                .Select(q => new { q.Name }) // Mahalle isimlerini alıyoruz.
                .ToList();

            return Json(mahalleler, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GizlilikPolitikasi()
        {



            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();
            return View(model);
        }

        public ActionResult TeslimatIadeSartlari()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();
            return View(model);
        }


        public ActionResult MesafeliSatisSozlesmesi()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();
            return View(model);
        }


        public ActionResult KVKKPolitikasi()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();
            return View(model);
        }



        public ActionResult Hakkimizda()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Where(x => x.is_active == true).Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();
            model.order_item = db.order_item.ToList();
            return View(model);
        }


        [HttpPost]
        public JsonResult eBulten(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailRegex))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }

            if (db.newsletter.Any(x => x.email == Email))
            {
                var n = db.newsletter.FirstOrDefault(x => x.email == Email);
                if (n.status == true)
                {
                    return Json(new { success = false, message = "Bu mail adresi zaten E-Bültenimize abonedir." });

                }
                else
                {
                    db.newsletter.Remove(n);
                    db.SaveChanges();
                    return Json(new { success = false, message = "Yeniden mail adresini onaylamanız gerekiyor.Mail adresinizi girdikten sonra size gönderdiğimiz mailden onay vermeniz gerekmektedir." });

                }

            }

            // Benzersiz bir onay tokeni oluşturma
            string token = Guid.NewGuid().ToString();

            // Onay linki oluşturma
            var confirmationLink = Url.Action("ConfirmEmail", "Home", new { Email, token }, Request.Url.Scheme);

            // Onay maili gönderme
            string subject = "E-Bülten Abonelik Onayı";
            string body = $@"
<html>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
        <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border: 1px solid #cccccc; background-color: #ffffff;'>
            <tr>
                <td align='center' bgcolor='red' style='padding: 40px 0; color: #ffffff; font-size: 24px; font-weight: bold;'>
                    Çobanlar Market E-Bülten
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; color: #333333; font-size: 16px;'>
                    <p>Merhaba,</p>
                    <p>E-bültenimize kaydolmak için e-posta adresinizi onaylamanız gerekiyor.</p>
                    <p style='text-align: center; margin: 20px 0;'>
                        <a href='{confirmationLink}' style='text-decoration: none; background-color: red; color: #ffffff; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>Aboneliği Onayla</a>
                    </p>
                    <p>Bu bağlantı 24 saat boyunca geçerlidir. Bu süreden sonra aboneliğinizi onaylamak için yeniden başvurmanız gerekecektir.</p>
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; font-size: 12px; color: #666666; text-align: center;'>
                    <p>Bu e-posta, Çobanlar Market e-Bülten abonelik sistemi tarafından gönderilmiştir.</p>
                    <p style='margin: 0;'>Çobanlar Market, Merkez Mahallesi Atatürk Caddesi No:17 TRABZON / MAÇKA</p>
                    <p style='margin: 0;'>© 2024 Çobanlar Market Tüm Hakları Saklıdır.</p>
                </td>
            </tr>
        </table>
    </body>
</html>";

            // E-posta gönderme 
            _emailService.SendEmail(Email, subject, body);

            newsletter newsletter = new newsletter
            {
                email = Email,
                token = token,
                token_expiration_date = DateTime.Now.AddHours(24),
                status = false
            };


            db.newsletter.Add(newsletter);
            db.SaveChanges();

            return Json(new { success = true, message = "Onay maili gönderildi. Lütfen e-posta adresinizi kontrol edin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ConfirmEmail(string email, string token)
        {
            // Token doğrulama ve e-posta kaydını tamamlama
            if (ValidateToken(email, token))
            {

                var n = db.newsletter.FirstOrDefault(x => x.email == email);
                n.status = true;
                db.SaveChanges();

                return Json(new { success = true, message = "E-bültene başarıyla kaydoldunuz!" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Token geçersiz veya süresi dolmuş." }, JsonRequestBehavior.AllowGet);
        }


        private bool ValidateToken(string email, string token)
        {

            if (db.newsletter.Any(x => x.email == email))
            {

                var n = db.newsletter.FirstOrDefault(x => x.email == email);

                if (n.token_expiration_date < DateTime.Now)
                {
                    db.newsletter.Remove(n);
                    db.SaveChanges();
                    return false;
                }
                if (n.token == token && n.token_expiration_date >= DateTime.Now)
                {

                    return true;

                }
                else
                {
                    return false;
                }
            }

            return false;

        }


        [HttpGet]
        public JsonResult confirmUnsubscribe(string email, string token)
        {



            if (ValidateToken(email, token))
            {

                var n = db.newsletter.FirstOrDefault(x => x.email == email);
                db.newsletter.Remove(n);
                db.SaveChanges();
                return Json(new { success = true, message = "E-Bülten aboneliğinden çıktınız." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Böyle bir mail adresi bulunmamaktadır." }, JsonRequestBehavior.AllowGet);




        }



        [HttpPost]
        public JsonResult Unsubscribe(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailRegex))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }

            if (!db.newsletter.Any(x => x.email == Email))
            {
                return Json(new { success = false, message = "Böyle bir mail adresi bulunmamaktadır." }, JsonRequestBehavior.AllowGet);

            }

            // Benzersiz bir onay tokeni oluşturma
            string token = Guid.NewGuid().ToString();

            // Onay linki oluşturma
            var confirmationLink = Url.Action("confirmUnsubscribe", "Home", new { Email, token }, Request.Url.Scheme);

            // Onay maili gönderme
            string subject = "E-Bülten Abonelikten Çıkma Onayı";
            string body = $@"
<html>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
        <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border: 1px solid #cccccc; background-color: #ffffff;'>
            <tr>
                <td align='center' bgcolor='red' style='padding: 40px 0; color: #ffffff; font-size: 24px; font-weight: bold;'>
                    Çobanlar Market E-Bülten
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; color: #333333; font-size: 16px;'>
                    <p>Merhaba,</p>
                    <p>E-bülten aboneliğinden çıkmak için maili onaylamanız gerekiyor.</p>
                    <p style='text-align: center; margin: 20px 0;'>
                        <a href='{confirmationLink}' style='text-decoration: none; background-color: red; color: #ffffff; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>Abonelikten Çık</a>
                    </p>
                    <p>Bu bağlantı 24 saat boyunca geçerlidir. Bu süreden sonra abonelikten çıkma işlemini onaylamak için yeniden başvurmanız gerekecektir.</p>
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; font-size: 12px; color: #666666; text-align: center;'>
                    <p>Bu e-posta, Çobanlar Market e-Bülten abonelik sistemi tarafından gönderilmiştir.</p>
                    <p style='margin: 0;'>Çobanlar Market, Merkez Mahallesi Atatürk Caddesi No:17 TRABZON / MAÇKA</p>
                    <p style='margin: 0;'>© 2024 Çobanlar Market Tüm Hakları Saklıdır.</p>
                </td>
            </tr>
        </table>
    </body>
</html>";

            // E-posta gönderme 
            _emailService.SendEmail(Email, subject, body);

            var n = db.newsletter.FirstOrDefault(x => x.email == Email);
            n.token = token;
            n.token_expiration_date = DateTime.Now.AddHours(10);
            db.SaveChanges();

            return Json(new { success = true, message = "Onay maili gönderildi. Lütfen e-posta adresinizi kontrol edin." }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResetPassword(string password, string confirmPassword)
        {


            var u = Session["ResetPasswordUserId"] as users;
            if (u == null)
            {
                return Json(new { success = false, message = "Sıfırlama linki geçersiz. Tekrar sıfırlama maili alabilirsiniz.", redirectUrl = Url.Action("Auth", "Home") }, JsonRequestBehavior.AllowGet);

            }
            users user = db.users.FirstOrDefault(x => x.id == u.id);

            if (!ValidateForgotToken(user.email, user.token))
            {
                return Json(new { success = false, message = "Sıfırlama linki geçersiz. Tekrar sıfırlama maili alabilirsiniz.", redirectUrl = Url.Action("Auth", "Home") }, JsonRequestBehavior.AllowGet);

            }


            if (password.IsNullOrWhiteSpace() || confirmPassword.IsNullOrWhiteSpace())
            {
                return Json(new { success = false, message = "Şifre ve şifre tekrar boş geçilemez." }, JsonRequestBehavior.AllowGet);

            }

            if (password.Replace(" ", "").Length < 8)
            {
                return Json(new { success = false, message = "Şifreniz en az 8 karakterden oluşmalıdır." }, JsonRequestBehavior.AllowGet);

            }

            if (password.Replace(" ", "") != confirmPassword.Replace(" ", ""))
            {
                return Json(new { success = false, message = "Şifre ve şifre tekrar uyuşmuyor." }, JsonRequestBehavior.AllowGet);
            }


            user.password = HashPassword(password.Replace(" ", ""));
            user.token = null;
            user.token_expiration_date = null;
            db.SaveChanges();
            Session["ResetPasswordUserId"] = null;
            return Json(new { success = true, message = "Şifreniz başarıyla güncellenmiştir.", redirectUrl = Url.Action("Auth", "Home") }, JsonRequestBehavior.AllowGet);

        }


        private bool ValidateForgotToken(string email, string token)
        {

            if (db.users.Any(x => x.email == email))
            {

                var u = db.users.FirstOrDefault(x => x.email == email);
                if (u.token_expiration_date < DateTime.Now)
                {
                    u.token = null;
                    u.token_expiration_date = null;
                    db.SaveChanges();
                    return false;
                }
                if (u.token == token && u.token_expiration_date >= DateTime.Now)
                {

                    return true;

                }
                else
                {
                    return false;
                }

            }

            return false;

        }

        [HttpGet]
        public ActionResult ConfirmForgotPassword(string email, string token)
        {
            if (ValidateForgotToken(email, token))
            {
                Session["ResetPasswordToken"] = token;
                Session["ResetPasswordEmail"] = email;

                return RedirectToAction("SifreSifirla");
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult SifreSifirla()
        {

            var email = Session["ResetPasswordEmail"] as string;
            var token = Session["ResetPasswordToken"] as string;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return HttpNotFound();

            var user = db.users.FirstOrDefault(x => x.email == email);

            if (user == null)
                return HttpNotFound();

            Session["ResetPasswordUserId"] = user;

            if (ValidateForgotToken(email, token))
            {



                AllViewModel model = new AllViewModel
                {
                    company_details = db.company_details.ToList()
                };
                return View("ConfirmForgotPassword", model);

            }

            return HttpNotFound();
        }


        [HttpPost]
        public JsonResult ForgotPassword(string Email)
        {
            if (string.IsNullOrEmpty(Email))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }
            var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailRegex))
            {
                return Json(new { success = false, message = "Lütfen geçerli bir e-posta adresi giriniz." });
            }

            if (!db.users.Any(x => x.email == Email))
            {
                return Json(new { success = false, message = "Böyle bir mail adresi bulunmamaktadır." }, JsonRequestBehavior.AllowGet);

            }
            var u = db.users.FirstOrDefault(x => x.email == Email);
            string token = null;
            if (ValidateForgotToken(u.email, u.token))
            {

                // Benzersiz bir onay tokeni oluşturma
                token = u.token;

                // Onay linki oluşturma
                var cconfirmationLink = Url.Action("ConfirmForgotPassword", "Home", new { Email, token }, Request.Url.Scheme);

                // Onay maili gönderme
                string ssubject = "Şifre Sıfırlama";
                string bbody = $@"
                        <html>
                            <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
                                <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border: 1px solid #cccccc; background-color: #ffffff;'>
                                    <tr>
                                        <td align='center' bgcolor='red' style='padding: 40px 0; color: #ffffff; font-size: 24px; font-weight: bold;'>
                                            Çobanlar Market Şifre Sıfırlama
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 20px; color: #333333; font-size: 16px;'>
                                            <p>Merhaba,</p>
                                            <p>Şifrenizi aşağıdaki linkten sıfırlayabilirsiniz.</p>
                                            <p style='text-align: center; margin: 20px 0;'>
                                                <a href='{cconfirmationLink}' style='text-decoration: none; background-color: red; color: #ffffff; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>Şifremi Sıfırla</a>
                                            </p>
                                            <p>Bu bağlantı 1 saat boyunca geçerlidir. Bu süreden sonra şifrenizi sıfırlamak için yeniden başvurmanız gerekecektir.</p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 20px; font-size: 12px; color: #666666; text-align: center;'>
                                            <p>Bu e-posta, Çobanlar Market şifre sıfırlama sistemi tarafından gönderilmiştir.</p>
                                            <p style='margin: 0;'>Çobanlar Market, Merkez Mahallesi Atatürk Caddesi No:17 TRABZON / MAÇKA</p>
                                            <p style='margin: 0;'>© 2024 Çobanlar Market Tüm Hakları Saklıdır.</p>
                                        </td>
                                    </tr>
                                </table>
                            </body>
                        </html>";

                // E-posta gönderme 
                _emailService.SendEmail(Email, ssubject, bbody);




                return Json(new { success = false, message = "Zaten size hala geçerli olan bir sıfırlama linki göndermişiz. Aynı maili size tekrar gönderiyoruz." }, JsonRequestBehavior.AllowGet);

            }

            // Benzersiz bir onay tokeni oluşturma
            token = Guid.NewGuid().ToString();

            // Onay linki oluşturma
            var confirmationLink = Url.Action("ConfirmForgotPassword", "Home", new { Email, token }, Request.Url.Scheme);

            // Onay maili gönderme
            string subject = "Şifre Sıfırlama";
            string body = $@"
<html>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;'>
        <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border: 1px solid #cccccc; background-color: #ffffff;'>
            <tr>
                <td align='center' bgcolor='red' style='padding: 40px 0; color: #ffffff; font-size: 24px; font-weight: bold;'>
                    Çobanlar Market Şifre Sıfırlama
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; color: #333333; font-size: 16px;'>
                    <p>Merhaba,</p>
                    <p>Şifrenizi aşağıdaki linkten sıfırlayabilirsiniz.</p>
                    <p style='text-align: center; margin: 20px 0;'>
                        <a href='{confirmationLink}' style='text-decoration: none; background-color: red; color: #ffffff; padding: 10px 20px; border-radius: 5px; font-weight: bold;'>Şifremi Sıfırla</a>
                    </p>
                    <p>Bu bağlantı 1 saat boyunca geçerlidir. Bu süreden sonra şifrenizi sıfırlamak için yeniden başvurmanız gerekecektir.</p>
                </td>
            </tr>
            <tr>
                <td style='padding: 20px; font-size: 12px; color: #666666; text-align: center;'>
                    <p>Bu e-posta, Çobanlar Market şifre sıfırlama sistemi tarafından gönderilmiştir.</p>
                    <p style='margin: 0;'>Çobanlar Market, Merkez Mahallesi Atatürk Caddesi No:17 TRABZON / MAÇKA</p>
                    <p style='margin: 0;'>© 2024 Çobanlar Market Tüm Hakları Saklıdır.</p>
                </td>
            </tr>
        </table>
    </body>
</html>";

            // E-posta gönderme 
            _emailService.SendEmail(Email, subject, body);

            u.token = token;
            u.token_expiration_date = DateTime.Now.AddHours(1);
            db.SaveChanges();

            return Json(new { success = true, message = "Sıfırlama maili gönderildi. Lütfen e-posta adresinizi kontrol edin." }, JsonRequestBehavior.AllowGet);
        }






    }
}