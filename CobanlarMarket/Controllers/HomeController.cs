using CobanlarMarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using WebGrease.Css.Extensions;
using System.Web.Services.Protocols;
using System.Web.Security;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web.UI.WebControls.WebParts;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using Iyzipay.Model.V2.Subscription;
using Iyzipay.Model;
using Iyzipay.Request;
using Iyzipay;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Iyzipay.Model.V2.Transaction;
using Iyzipay.Request.V2;
using CobanlarMarket.Hubs;
using Microsoft.AspNet.SignalR;
using System.Net.Mail;

namespace CobanlarMarket.Controllers
{
    public class HomeController : Controller
    {
        private CobanlarMarketEntities db = new CobanlarMarketEntities();
        private EmailService _emailService = new EmailService();

        public object Viewbag { get; private set; }
        public ActionResult Index()
        {

            var expiredCampaigns = db.campaigns
                .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
                .ToList();

            expiredCampaigns.ForEach(c => c.is_active = false);

            db.SaveChanges();

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
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);

        }

        public ActionResult Cart()
        {

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
            }




            ViewBag.Discount = 0;
            return View(model);
        }

        public ActionResult Campaing()
        {

            var expiredCampaigns = db.campaigns
                .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
                .ToList();

            expiredCampaigns.ForEach(c => c.is_active = false);

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

            expiredCampaigns.ForEach(c => c.is_active = false);

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
                             .Where(p => campaignProductIds.Contains(p.id) && p.status==true)
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
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.company_details = db.company_details.ToList();

            return View(model);


        }


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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            products product = db.products.FirstOrDefault(x => x.status == true && x.id == Id);
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


        public JsonResult AddCart(int Id, int Adet)
        {
            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {
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
        public ActionResult Auth(String username, String password)
        {
            if (!username.Equals("") && !password.Equals(""))
            {

                if (db.users.Where(x => x.username == username || x.email==username && x.password == password).Count() != 0)
                {
                    var user = db.users.FirstOrDefault(x => x.username == username  || x.email==username && x.password == password);
                    Session["User"] = db.users.FirstOrDefault(x => x.username == username || x.email == username && x.password == password);

                    if (user.role == true)
                    {

                        FormsAuthentication.SetAuthCookie("Yönetici", false);
                        return Json(new { redirectUrl = Url.Action("Dashboard", "Management") });
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie("Müşteri", false);
                        return Json(new { redirectUrl = Url.Action("Index", "Home") });

                    }
                }
                else
                {
                    return Json(new { redirectUrl = Url.Action("Auth", "Home") });
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
                        <title>Çobanlar Market Kayıt Onay Kodunuz</title>
                    </head>
                    <body>
                        <h1>Merhaba " + Name + @"!</h1>
                        <p>Kayıt olmak için onay kodunuz:<h3><strong>" + code + @"</strong></h3></p>
                        
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
                user.password = Session["NewPassword"] as string;
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


        public void ResetConfirmCode()
        {
            Session["ConfirmationCode"] = null;
        }

        public ActionResult LogOut()
        {
            Session["User"] = null;
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");

        }


        public PartialViewResult Search(string Word)
        {
            var results = db.products.Where(x => x.name.Contains(Word) && x.status == true).Take(10).ToList();


            return PartialView("_SearchPartial", results);
        }



        public async Task<JsonResult> Order(String AddressId)
        {
            var user = Session["User"] as users;
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);
            }

            var cart = await db.cart.FirstOrDefaultAsync(x => x.user_id == user.id);
            if (cart == null)
            {
                return Json(new { success = false, message = "Sepet bulunamadı" }, JsonRequestBehavior.AllowGet);
            }


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


                db.products_skus.FirstOrDefault(x => x.product_id == item.product_id).quantity -= item.quantity;
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

            return Json(new { success = true, message = "Ödeme başarılı" }, JsonRequestBehavior.AllowGet);
        }



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
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Payment(String Price, String PaidPrice, String BasketId, users User, List<cart_item> cart_items, String AddressId)
        {
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
            request.CallbackUrl = "https://4421-46-1-5-250.ngrok-free.app/Home/Sonuc?UserId=" + User.id + "&AddressId=" + AddressId;

            List<int> enabledInstallments = new List<int>();
            enabledInstallments.Add(2);
            enabledInstallments.Add(3);
            enabledInstallments.Add(6);
            enabledInstallments.Add(9);
            request.EnabledInstallments = enabledInstallments;

            int id = int.Parse(AddressId);

            Session["AdressId"] = id.ToString();

            var address = db.addresses.Find(id);
            string hostName = Dns.GetHostName();
            Console.WriteLine(hostName);

            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            Console.WriteLine("My IP Address is :" + myIP);

            Buyer buyer = new Buyer();
            buyer.Id = User.id.ToString();
            buyer.Name = User.first_name.ToString();
            buyer.Surname = User.last_name.ToString();
            buyer.GsmNumber = User.phone_number.ToString();
            buyer.Email = User.email.ToString();
            buyer.IdentityNumber = "12345678911";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            DateTime createdAtDateTime = Convert.ToDateTime(User.created_at);
            string formattedDate = createdAtDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            buyer.RegistrationDate = formattedDate;
            buyer.RegistrationAddress = "Trabzon";
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

            return View();
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
                        order_id = od.id
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


                Session["User"] = db.users.FirstOrDefault(x => x.id == order.user_id);

                return RedirectToAction("SiparisDetay", new { orderId = order.id });

            }

            // Ödeme başarısızsa hata mesajı gösterin
            ViewBag.PaymentStatus = "FAILED";


            return View("Sonuc");
        }

        public ActionResult SiparisDetay(int orderId)
        {

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



                Options options = new Options();
                options.ApiKey = "sandbox-lfDKd5dEcP9SvjEbRdOaMGX5LOYVcYgO";
                options.SecretKey = "G4GKghvkujw7YYchDECfiW6MzhfTLhsq";
                options.BaseUrl = "https://sandbox-api.iyzipay.com";

                // Ödeme bilgilerini sorgulama talebi oluşturun
                RetrievePaymentRequest request = new RetrievePaymentRequest();
                request.PaymentId = paymentId; // webhook'dan gelen paymentId'yi kullanın
                request.ConversationId = "123456789"; // Optional, kendi referansınızı koyabilirsiniz

                // Ödeme detaylarını İyzico API'sinden sorgulayın
                TransactionDetail paymentDetail = new TransactionDetail();

                // Transaction raporu almak için istek oluştur
                RetrieveTransactionReportRequest requestt = new RetrieveTransactionReportRequest()
                {
                    ConversationId = conversationId, // Payment işleminde kullandığın ConversationId
                    TransactionDate = "2024-09-11 23:59:59",
                    Page = 1
                };

                // Transaction raporunu getir
                TransactionReport transactionReport = TransactionReport.Retrieve(requestt, options);

                // Sonuçları incele ve işle
                if (transactionReport.Transactions.FirstOrDefault(x => x.PaymentId.ToString() == paymentId).TransactionType == "CANCEL")
                {

                    //İADE İPTAL
                    Console.WriteLine("İade yapıldı" + paymentId);
                }
                else
                {
                    // Hata varsa hata mesajını döndür
                    ViewBag.Message = "İşlem raporu alınırken bir hata oluştu: " + transactionReport.ErrorMessage;
                }



            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }




        public JsonResult AddAdress(string Name, string Surname, string Phone, string Title, string Address)
        {

            try
            {
                var user = Session["User"] as users;
                if (user != null)
                {
                    var address = new addresses();
                    address.user_id = user.id;
                    address.name = Name;
                    address.surname = Surname;
                    address.title = Title;
                    address.phone_number = Phone;
                    address.address = Address;
                    address.country = "Türkiye";
                    address.city = "Trabzon";
                    address.postal_code = "61750";
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

        public JsonResult EditAdress(int Id, string Name, string Surname, string Phone, string Title, string Address)
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

                    var address = db.addresses.Find(Id);
                    address.name = Name;
                    address.surname = Surname;
                    address.title = Title;
                    address.phone_number = Phone;
                    address.address = Address;
                    address.country = "Türkiye";
                    address.city = "Trabzon";
                    address.postal_code = "61750";


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
                return Json(new { success = false, message = "Adres Eklenirken Bir Hata Oluştu" }, JsonRequestBehavior.AllowGet);

                throw;
            }



        }

        [HttpPost]
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

    }
}