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

namespace CobanlarMarket.Controllers
{
    public class HomeController : Controller
    {
        private CobanlarMarketEntities db = new CobanlarMarketEntities();

        public object Viewbag { get; private set; }

        public ActionResult Index()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Include(x => x.campaign_products).ToList();
            model.carts = db.cart.ToList();

            return View(model);

        }
        
        public ActionResult Shop()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);
        }

        public ActionResult Contact()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);

        }

        public ActionResult Cart()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(p => p.products_skus).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.Include(p => p.cart_item).ToList(); ;

            ViewBag.Discount = 0;
            return View(model);
        }

        public ActionResult Campaing()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.campaigns = db.campaigns.Include(p => p.campaign_products).ToList();
            model.carts = db.cart.ToList();

            return View(model);


        }
        public ActionResult CampaingDetails(int id)
        {
            // İlgili kampanyadaki ürünlerin ID'lerini alıyoruz.
            var campaignProductIds = db.campaigns
                                       .Where(x => x.id == id)
                                       .SelectMany(x => x.campaign_products)
                                       .Select(cp => cp.product_id)
                                       .ToList();

            AllViewModel model = new AllViewModel();

            // Sadece campaignProductIds'e ait ürünler
            var products = db.products
                             .Where(p => campaignProductIds.Contains(p.id))
                             .ToList();

            model.products = products;

            // Bu ürünlere ait kategoriler ve alt kategoriler



            var subCategoryIds = products.Select(p => p.category_id).Distinct().ToList();
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
            // Diğer model verileri
            model.products_skus = db.products_skus.ToList();
            model.campaigns = db.campaigns.Where(x => x.id == id).ToList();
            model.users = db.users.ToList();
            model.carts = db.cart.ToList();

            return View(model);
        }



        public ActionResult Blog()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);


        }
        public ActionResult Auth()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);


        }



        public PartialViewResult GetProductsPartial(int CategoryId, int? CategoryType, string PriceRange, int PageNumber)
        {
            IOrderedQueryable<products> products = null;
            int pageSize = 12;
            if (CategoryId != -1)
            {

                if (CategoryType == 1)
                {
                    products = db.products.Where(x => x.category_id == CategoryId).OrderBy(x => x.id); // Adjust the query as per your requirement

                }
                else if (CategoryType == 0)
                {
                    products = db.products.Where(x => x.sub_categories.parent_id == CategoryId).OrderBy(x => x.id);
                }

            }
            else
            {
                products = db.products.OrderBy(x => x.id); // Adjust the query as per your requirement

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

            var pagedProducts = products.Skip((PageNumber - 1) * pageSize).Take(pageSize).ToList();
            var totalProducts = products.Count();
            ViewBag.totalProcuct = totalProducts;
            ViewBag.pageSize = pageSize;
            ViewBag.categoryId = CategoryId;


            AllViewModel model = new AllViewModel();
            model.products = pagedProducts;
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();



            return PartialView("_ProductsPartial", model);
        }



        public PartialViewResult GetCampaignProductsPartial(int CampaignId, int CategoryId, int? CategoryType, string PriceRange, int PageNumber)
        {
            IOrderedQueryable<products> products = null;


            var campaignProductIds = db.campaigns
                                .Where(x => x.id == CampaignId)
                                .SelectMany(x => x.campaign_products)
                                .Select(cp => cp.product_id)
                                .ToList();




            int pageSize = 12;
            if (CategoryId != -1)
            {

                if (CategoryType == 1)
                {
                    products = db.products.Where(x => x.category_id == CategoryId && campaignProductIds.Contains(x.id)).OrderBy(x => x.id); // Adjust the query as per your requirement

                }
                else if (CategoryType == 0)
                {
                    products = db.products.Where(x => x.sub_categories.parent_id == CategoryId && campaignProductIds.Contains(x.id)).OrderBy(x => x.id);
                }

            }
            else
            {


                products = db.products
                                 .Where(p => campaignProductIds.Contains(p.id))
                                 .OrderBy(p => p.id);


            }
            // Fiyat aralığına göre ürünleri filtreleme
            if (!string.IsNullOrEmpty(PriceRange))
            {
                var prices = PriceRange.Split('-');
                if (prices.Length == 2)
                {
                    if (decimal.TryParse(prices[0], out decimal minPrice) && decimal.TryParse(prices[1], out decimal maxPrice))
                    {
                        products = products.Where(x => x.products_skus.FirstOrDefault().price >= minPrice && x.products_skus.FirstOrDefault().price <= maxPrice && campaignProductIds.Contains(x.id)).OrderBy(x => x.id);
                    }
                }
            }

            var pagedProducts = products.Skip((PageNumber - 1) * pageSize).Take(pageSize).ToList();
            var totalProducts = products.Count();
            ViewBag.totalProcuct = totalProducts;
            ViewBag.pageSize = pageSize;
            ViewBag.categoryId = CategoryId;


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


            products product = db.products.Find(Id);
            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).Include(x => x.product_images).Where(x => x.id == Id).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.carts = db.cart.ToList();

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
                    var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;
                    var existingItem = db.cart.FirstOrDefault(x => x.user_id == user.id).cart_item
                                        .FirstOrDefault(x => x.product_id == Id);

                    if (existingItem != null)
                    {
                        existingItem.quantity += Adet;
                    }
                    else
                    {
                        db.cart.FirstOrDefault(x => x.user_id == user.id).cart_item.Add(item);
                    }
                    db.SaveChanges();

                    var cartItemsPartial = PartialView("_CartPartial", db.cart_item.Include(c => c.products).Where(x => x.cart_id == cartId).ToList());
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



        public PartialViewResult removeItemInCart(int Id)
        {
            var user = Session["User"] as users;
            if (user != null)
            {
                db.cart_item.Remove(db.cart_item.FirstOrDefault(x => x.id == Id));
                db.SaveChanges();
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;

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
                }
                else
                {
                    db.cart_item.Remove(db.cart_item.FirstOrDefault(x => x.id == Id));
                }
                db.SaveChanges();
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;

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
                var cartId = db.cart.FirstOrDefault(x => x.user_id == user.id).id;

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

                if (db.users.Where(x => x.username == username && x.password == password).Count() != 0)
                {
                    var user = db.users.FirstOrDefault(x => x.username == username && x.password == password);
                    Session["user"] = db.users.FirstOrDefault(x => x.username == username && x.password == password);

                    if (user.role == true)
                    {

                        FormsAuthentication.SetAuthCookie(user.role.ToString(), false);
                        return Json(new { redirectUrl = Url.Action("Dashboard", "Management") });
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(user.role.ToString(), false);
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
            return View(model);


        }


        public ActionResult LogOut()
        {
            Session["User"] = null;
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");

        }


        public PartialViewResult Search(string Word)
        {
            var results = db.products.Where(x => x.name.Contains(Word)).Take(10).ToList();


            return PartialView("_SearchPartial", results);
        }



        public async Task<JsonResult> Order()
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

            var od = new order_details
            {
                user_id = user.id,
                created_at = DateTime.Now
            };

            db.order_details.Add(od);
            await db.SaveChangesAsync();

            var cartItems = await db.cart_item
                                    .Where(x => x.cart_id == cart.id)
                                    .Include(x => x.products)
                                    .ToListAsync();

            decimal? total = 0;
            var orderItems = new List<order_item>();

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
                    created_at = DateTime.Now
                });
                db.products_skus.FirstOrDefault(x => x.product_id == item.product_id).quantity -= item.quantity;
                db.SaveChanges();
            }

            db.order_item.AddRange(orderItems);
            od.total = total;

            var pd = new payment_details
            {
                created_at = DateTime.Now,
                order_id = od.id,
                amount = total,
                provider = "Mastercard",
                status = "Ödendi"
            };

            db.payment_details.Add(pd);
            await db.SaveChangesAsync();

            od.payment_id = pd.id;
            await db.SaveChangesAsync();

            db.cart_item.RemoveRange(cartItems);
            await db.SaveChangesAsync();

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
                    var coupon = db.coupons.FirstOrDefault(x => x.Code == CouponCode);
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

                            // Convert nullable integers to non-nullable, removing any nulls
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
                                                    .Select(p => p.category_id)
                                                    .Where(sc => sc.HasValue)
                                                    .Select(sc => sc.Value)
                                                    .ToList();


                            // Check if any product, category, or subcategory in the cart is eligible for the coupon
                            bool isProductEligible = cartProductIds.Any(cartProductId => couponProductIds.Contains(cartProductId));
                            bool isCategoryEligible = cartCategoryIds.Any(cartCategoryId => couponCategoryIds.Contains(cartCategoryId));
                            bool isSubCategoryEligible = cartSubCategoryIds.Any(cartSubCategoryId => couponSubCategoryIds.Contains(cartSubCategoryId));

                            // If any of the checks are true, the coupon is eligible
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
                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = indirimTutarı,coupon=coupon.Code });
                                    }
                                    else
                                    {
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

                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = indirimTutarı, coupon = coupon.Code });

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

                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString() , discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code });
                                    }
                                    else
                                    {
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

                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code });

                                }

                            }
                            else if (isSubCategoryEligible)
                            {
                                var categoryIds = cartSubCategoryIds.Where(cartSubCategoryId => couponSubCategoryIds.Contains(cartSubCategoryId));

                                decimal? total = 0;
                                decimal? indirimTutarı = 0;
                                foreach (var item in userCart.cart_item.Where(x => x.products.category_id.HasValue && categoryIds.Contains(x.products.category_id.Value)))

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

                                        return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount = Math.Round((decimal)indirimTutarı, 2).ToString("F2").Replace(".", ","), coupon = coupon.Code });
                                    }
                                    else
                                    {
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

                                    return Json(new { success = true, message = "Kupon geçerli! Tl:" + indirimTutarı.ToString(), discount =Math.Round((decimal)indirimTutarı,2).ToString("F2").Replace(".",","), coupon = coupon.Code });

                                }

                            }
                            else
                            {
                                return Json(new { success = false, message = "Sepetinizdeki ürünler bu kupon için uygun değil." });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Kullanıcı sepeti bulunamadı." });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Geçersiz kupon kodu." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Kullanıcı oturumu geçersiz." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }



    }
}