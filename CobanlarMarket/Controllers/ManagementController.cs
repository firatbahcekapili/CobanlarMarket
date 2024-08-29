using CobanlarMarket.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Windows.Forms;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

namespace CobanlarMarket.Controllers
{
    public class ManagementController : Controller
    {

        private CobanlarMarketEntities db = new CobanlarMarketEntities();
        // GET: Management
        public ActionResult Dashboard()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }

        public ActionResult AddProduct()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }
        [HttpPost]
        public ActionResult AddProduct(IEnumerable<HttpPostedFileBase> Imgs, string Name, string SubCategory, string Description, string Size, string Color, string Sku, int Quantity, string Price, string OldPrice)
        {

            var path = Server.MapPath("~/Content/ProductImg/" + Name.Replace(" ", "") + "/");
            List<String> imgPaths = null;
            if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
            {
                Directory.CreateDirectory(path); // Klasörü oluştur
            }

            if (Imgs != null)
            {
                foreach (var img in Imgs)
                {
                    if (img != null && img.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(img.FileName);
                        var filePath = Path.Combine(path, fileName);
                        img.SaveAs(filePath);
                    }
                }
                imgPaths = Imgs.Select(img => "/Content/ProductImg/" + Name.Replace(" ", "") + "/" + Path.GetFileName(img.FileName)).ToList();

            }







            // Resmi bir klasöre kaydetme

            products product = new products() { name = Name, category_id = int.Parse(SubCategory), cover = imgPaths == null ? null : imgPaths[0], description = Description, created_at = DateTime.Now, status = true };
            db.products.Add(product);
            db.SaveChanges();
            decimal? price = null;
            decimal? old_price = null;


            if (decimal.TryParse(OldPrice.Replace(".", ","), out decimal parsedOldPrice))
            {
                old_price = parsedOldPrice;
            }


            if (decimal.TryParse(Price.Replace(".", ","), out decimal parsedPrice))
            {
                price = parsedPrice;
            }


            products_skus products_skus = new products_skus() { product_id = product.id, size_attribute_id = int.Parse(Size), color_attribute_id = -1, sku = Sku, quantity = Quantity, price = price, old_price = old_price, created_at = DateTime.Now };

            db.products_skus.Add(products_skus);
            db.SaveChanges();


            if (imgPaths != null)
            {
                foreach (var img in imgPaths)
                {
                    db.product_images.Add(new product_images { product_id = product.id, image_path = img });
                    db.SaveChanges();
                }
            }


            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return RedirectToAction("ProductList", model);


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

        [HttpGet]
        public JsonResult GetAttributes()
        {
            var colors = db.product_attributes

                                  .Select(c => new
                                  {
                                      c.type,
                                      c.value,
                                      c.id
                                  })
                                  .ToList();

            return Json(colors, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult RemoveProduct(int Id)
        {
            products product = db.products.Find(Id);
            product.status = false;


            db.SaveChanges();
            var list = db.products
                    .Where(x => x.status == true)
                    .ToList()  // Retrieve data from database before formatting
                    .Select(x => new
                    {
                        x.id,
                        x.name,
                        x.description,
                        x.summary,
                        x.cover,
                        x.category_id,
                        created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                        product_skus_count = x.products_skus.Count(),
                        product_sku_quantity = x.products_skus.FirstOrDefault().quantity

                    })
                    .ToList();
            return Json(list, JsonRequestBehavior.AllowGet);


        }

        public ActionResult EditProduct(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            products product = db.products.Find(Id);

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult EditProduct(IEnumerable<HttpPostedFileBase> Imgs, int Id, string Name, int? SubCategory, string Description, int? Size, int? Color, string Sku, int Quantity, string Price, string OldPrice)
        {
            try
            {
                if (db.products.Any(x => x.id == Id))
                {
                    var product = db.products.Find(Id);

                    if (product != null)
                    {
                        product.name = Name;
                        product.description = Description;
                        product.category_id = SubCategory;
                        product.products_skus.FirstOrDefault().size_attribute_id = Size ?? -1;
                        product.products_skus.FirstOrDefault().color_attribute_id = Color ?? -1;
                        product.products_skus.FirstOrDefault().sku = Sku;
                        product.products_skus.FirstOrDefault().quantity = Quantity;
                        decimal? price = null;
                        decimal? old_price = null;


                        if (decimal.TryParse(OldPrice.Replace(".", ","), out decimal parsedOldPrice))
                        {
                            old_price = parsedOldPrice;
                        }


                        if (decimal.TryParse(Price.Replace(".", ","), out decimal parsedPrice))
                        {
                            price = parsedPrice;
                        }
                        product.products_skus.FirstOrDefault().price = price;
                        product.products_skus.FirstOrDefault().old_price = old_price;

                        var path = Server.MapPath("~/Content/ProductImg/" + Name.Replace(" ", "") + "/");

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var existingFiles = db.product_images
                            .Where(p => p.product_id == product.id)
                            .Select(p => p.image_path)
                            .ToList();

                        var savedFiles = new List<string>();
                        bool hasError = false;
                        var errorMessage = "";

                        if (Imgs != null)
                        {
                            foreach (var img in Imgs)
                            {
                                if (img != null && img.ContentLength > 0)
                                {
                                    var fileName = Path.GetFileName(img.FileName);
                                    var filePath = Path.Combine(path, fileName);

                                    if (!System.IO.File.Exists(filePath))
                                    {
                                        img.SaveAs(filePath);
                                        var imgPath = "/Content/ProductImg/" + Name.Replace(" ", "") + "/" + fileName;

                                        if (!db.product_images.Any(p => p.image_path == imgPath))
                                        {
                                            db.product_images.Add(new product_images { product_id = product.id, image_path = imgPath });
                                            db.SaveChanges();
                                        }

                                        savedFiles.Add(imgPath);
                                    }
                                    else
                                    {
                                        hasError = true;
                                        errorMessage = $"'{fileName}' dosyası zaten mevcut.";
                                    }
                                }
                            }
                            product.cover = product.product_images.FirstOrDefault().image_path;
                        }

                        db.SaveChanges();
                        var list = db.product_images
                          .Where(x => x.product_id == Id)
                          .ToList()  // Retrieve data from database before formatting
                          .Select(x => new
                          {
                              x.id,
                              x.product_id,
                              x.image_path


                          })
                          .ToList();

                        if (hasError)
                        {
                            return Json(new { success = false, message = errorMessage, files = list });
                        }
                        else
                        {
                            return Json(new { success = true, message = "Ürün başarıyla güncellendi!", files = list });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Ürün bulunamadı." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Geçersiz ürün ID'si." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }



        [HttpPost]
        public JsonResult RemoveImg(int Id)
        {
            var img = db.product_images.Find(Id);
            if (img == null)
            {
                // Dosya veritabanında bulunamadıysa 404 döndür
                return Json(new { success = false, message = "Resim Bulunamadı" }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                var path = Server.MapPath(img.image_path);
                // Dosyanın var olup olmadığını kontrol et
                if (System.IO.File.Exists(path))
                {
                    // Dosyayı sil
                    System.IO.File.Delete(path);
                    db.product_images.Remove(img);
                    db.SaveChanges();
                    var product = db.products.Find(img.product_id);
                    if (product.product_images.Any())
                    {
                        product.cover = product.product_images.FirstOrDefault().image_path;
                    }
                    else
                    {
                        product.cover = null;
                    }
                }
                else
                {
                    // Dosya bulunamadıysa 404 döndür
                    return Json(new { success = false, message = "Resim Bulunamadı" }, JsonRequestBehavior.AllowGet);
                }

                // Veritabanından dosya kaydını sil
                db.SaveChanges();

                // Başarı mesajı döndür
                return Json(new { success = true, message = "Resim başarıyla silindi" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Hata durumunda 500 Internal Server Error döndür
                return Json(new { success = false, message = "Resim Silinirken Bir Hata Oluştu" }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ProductList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }






        public ActionResult AddCategory()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        [HttpPost]
        public JsonResult AddCategory(HttpPostedFileBase Img, string Name, string Description)
        {

            string imgpath = null;
            if (!db.categories.Any(x => x.name == Name))
            {
                if (Img != null)
                {
                    var fileName = Path.GetFileName(Img.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/CategoriesImg/" + Name + "/"));

                    if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                    {
                        Directory.CreateDirectory(path); // Klasörü oluştur
                        ViewBag.Message = "Yeni klasör başarıyla oluşturuldu.";
                    }
                    else
                    {

                        ViewBag.Message = "Klasör zaten var.";
                    }




                    Img.SaveAs(Path.Combine(Server.MapPath("~/Content/CategoriesImg/" + Name + "/"), fileName));
                    imgpath = "/Content/CategoriesImg/" + Name + "/" + fileName;




                }
                if (Name != "" && Description != "")
                {
                    categories category = new categories()
                    {
                        name = Name,
                        cover = imgpath,
                        description = Description,
                        created_at = DateTime.Now,
                    };

                    db.categories.Add(category);
                    db.SaveChanges();
                }

                var listt = db.categories
                 .ToList()  // Retrieve data from database before formatting
                 .Select(x => new
                 {
                     x.id,
                     x.name,
                     x.description,

                 })
                 .ToList();
                return Json(new { success = true, message = "Kategori Başarıyla Eklendi.", list = listt });



            }

            var list = db.categories
               .ToList()  // Retrieve data from database before formatting
               .Select(x => new
               {
                   x.id,
                   x.name,
                   x.description,

               })
               .ToList();
            return Json(new { success = false, message = "Kategori Eklenirken Bir Hata Oluştu.", list = list });

        }

        public JsonResult RemoveCategory(int Id)
        {
            using (db)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Kategoriyi bul
                        var category = db.categories.Include(c => c.sub_categories).FirstOrDefault(c => c.id == Id);

                        if (category == null)
                        {
                            return Json(new { success = false, message = "Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
                        }

                        // Alt kategorileri güncelle
                        foreach (var subCategory in category.sub_categories.ToList())
                        {
                            subCategory.parent_id = null; // veya uygun başka bir kategoriye ayarlayın
                                                          // Ürünleri güncelle
                            foreach (var product in db.products.Where(x => x.category_id == subCategory.id))
                            {
                                product.category_id = null; // veya uygun başka bir kategoriye ayarlayın
                            }
                        }



                        // Kategoriyi sil
                        db.categories.Remove(category);

                        // Değişiklikleri kaydet
                        db.SaveChanges();

                        // Transaction'ı tamamla
                        transaction.Commit();


                        var listt = db.categories
                         .ToList()  // Retrieve data from database before formatting
                         .Select(x => new
                         {
                             x.id,
                             x.name,
                             x.description,
                             created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"
                         })
                         .ToList();
                        return Json(listt, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda rollback yap
                        transaction.Rollback();
                        // Hata mesajını kullanıcıya göster
                        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

        }


        public ActionResult EditCategory(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            categories category = db.categories.Find(Id);

            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }


        [HttpPost]
        public ActionResult EditCategory(HttpPostedFileBase Img, int Id, string Name, string Description)
        {

            if (db.categories.Any(x => x.id == Id))
            {

                if (Name != "" && Description != "")
                {

                    string imgPath = null;




                    categories category = db.categories.Find(Id);


                    category.name = Name;
                    category.description = Description;

                    var path = Server.MapPath("~/Content/CategoriesImg/" + Name.Replace(" ", "") + "/");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }




                    if (Img != null)
                    {

                        if (Img != null && Img.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(Img.FileName);
                            var filePath = Path.Combine(path, fileName);

                            if (!System.IO.File.Exists(filePath))
                            {

                                DirectoryInfo di = new DirectoryInfo(path);

                                try
                                {
                                    // Klasörün içindeki tüm dosyaları sil
                                    foreach (FileInfo file in di.GetFiles())
                                    {
                                        file.Delete();
                                    }

                                    // Klasörün içindeki tüm alt klasörleri sil
                                    foreach (DirectoryInfo dir in di.GetDirectories())
                                    {
                                        dir.Delete(true);
                                    }

                                    Console.WriteLine("Klasör başarıyla boşaltıldı.");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Hata: " + ex.Message);
                                }
                                Img.SaveAs(filePath);


                            }
                            imgPath = "/Content/CategoriesImg/" + Name.Replace(" ", "") + "/" + fileName;


                        }

                        category.cover = imgPath;

                    }




                    db.SaveChanges();
                    var list = db.categories.Where(x => x.id == Id)
                       .ToList()  // Retrieve data from database before formatting
                       .Select(x => new
                       {
                           x.id,
                           x.name,
                           x.cover,
                           x.description,
                           created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"
                       })
                       .ToList();
                    return Json(new { success = true, message = "Kategori Başarıyla Düzenlendi.", category = list });
                }
                else
                {
                    return Json(new { success = false, message = "İsim Ve Açıklama Boş Geçilemez." });

                }
            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kategori Mevcut Değil.." });

            }



        }


        [HttpPost]
        public JsonResult RemoveCategoryImg(int Id)
        {
            var category = db.categories.Find(Id);
            if (category == null)
            {
                // Dosya veritabanında bulunamadıysa 404 döndür
                return Json(new { success = false, message = "Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                category.cover = null;
                db.SaveChanges();
                return Json(new { success = true, message = "Resim Kaldırıldı" }, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult EditSubCategory(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            sub_categories subcategory = db.sub_categories.Find(Id);

            if (subcategory == null)
            {
                return HttpNotFound();
            }
            return View(subcategory);
        }


        [HttpPost]
        public ActionResult EditSubCategory(int Id, int ParentId, string Name, string Description)
        {

            if (db.sub_categories.Any(x => x.id == Id))
            {

                if (Name != null && Description != null)
                {
                    sub_categories subcategory = db.sub_categories.Find(Id);

                    subcategory.name = Name;
                    subcategory.description = Description;
                    subcategory.parent_id = ParentId;

                    db.SaveChanges();
                }
            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return Json(new { redirectUrl = Url.Action("CategoryList", "Management") });
        }






        public JsonResult RemoveSubCategory(int Id)
        {
            using (db)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Kategoriyi bul
                        var category = db.sub_categories.FirstOrDefault(c => c.id == Id);

                        if (category == null)
                        {
                            return Json(new { success = false, message = "Alt Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
                        }

                        // Alt kategorileri güncelle

                        foreach (var product in db.products.Where(x => x.category_id == Id))
                        {
                            product.category_id = null; // veya uygun başka bir kategoriye ayarlayın
                        }




                        // Kategoriyi sil
                        db.sub_categories.Remove(category);

                        // Değişiklikleri kaydet
                        db.SaveChanges();

                        // Transaction'ı tamamla
                        transaction.Commit();


                        var listt = db.sub_categories
                         .ToList()  // Retrieve data from database before formatting
                         .Select(x => new
                         {
                             x.id,
                             x.name,
                             x.description,
                             created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                             parentCategory = db.categories.FirstOrDefault(c => c.id == x.parent_id) == null ? "Kategori Yok" : db.categories.FirstOrDefault(c => c.id == x.parent_id).name
                         })
                         .ToList();
                        return Json(listt, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        // Hata durumunda rollback yap
                        transaction.Rollback();
                        // Hata mesajını kullanıcıya göster
                        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

        }

        [HttpPost]
        public void AddSubCategory(int ParentId, string Name, string Description)
        {

            if (db.categories.Any(x => x.id == ParentId))
            {

                if (Name != "" && Description != "")
                {
                    sub_categories subcategory = new sub_categories()
                    {
                        parent_id = ParentId,
                        name = Name,
                        description = Description,
                        created_at = DateTime.Now,
                    };

                    db.sub_categories.Add(subcategory);
                    db.SaveChanges();
                }

            }


        }


        public ActionResult CategoryList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.sub_categories = db.sub_categories.ToList();
            return View(model);
        }


        public ActionResult AddAttribute()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }
        [HttpPost]
        public ActionResult AddAttribute(string Value, string Type)
        {


            if (Value != null && Type != null)
            {
                product_attributes attributes = new product_attributes
                {
                    value = Value,
                    type = Type,
                    created_at = DateTime.Now,
                };

                db.product_attributes.Add(attributes);
                db.SaveChanges();
            }


            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }

        public JsonResult RemoveAttribute(int Id)
        {
            try
            {
                if (db.product_attributes.Any(x => x.id == Id))
                {
                    product_attributes attribute = db.product_attributes.FirstOrDefault(x => x.id == Id);
                    if (attribute != null)
                    {
                        if (attribute.type == "color")
                        {
                            foreach (var item in db.products_skus.Where(x => x.color_attribute_id == Id))
                            {
                                item.color_attribute_id = -1;
                            }
                        }
                        else if (attribute.type == "size")
                        {
                            foreach (var item in db.products_skus.Where(x => x.size_attribute_id == Id))
                            {
                                item.size_attribute_id = -1;
                            }
                        }

                        db.SaveChanges(); // Ürün SKU'ları güncelleme değişikliklerini kaydet
                        db.product_attributes.Remove(attribute);
                        db.SaveChanges(); // Attribute'u kaldırma değişikliklerini kaydet
                    }
                }

                var list = db.product_attributes
                 .ToList()  // Retrieve data from database before formatting
                 .Select(x => new
                 {
                     x.id,
                     x.type,
                     x.value,
                     created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"
                 })
                 .ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Hata durumunda JSON hata mesajı döndür
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }









        }




        public ActionResult EditAttribute(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            product_attributes attribute = db.product_attributes.Find(Id);

            if (attribute == null)
            {
                return HttpNotFound();
            }
            return View(attribute);
        }


        [HttpPost]
        public ActionResult EditAttribute(int Id, string Value, string Type)
        {

            if (db.product_attributes.Any(x => x.id == Id))
            {

                if (Value != null && Type != null)
                {
                    product_attributes attribute = db.product_attributes.Find(Id);

                    attribute.value = Value;
                    attribute.type = Type;

                    db.SaveChanges();
                }
            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return Json(new { redirectUrl = Url.Action("AttributeList", "Management") });
        }


        public ActionResult AttributeList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.product_attributes = db.product_attributes.ToList();
            return View(model);
        }
        public ActionResult OrderDetail()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }
        public ActionResult OrderList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            return View(model);
        }


        public ActionResult AddUser()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);
        }


        [HttpPost]
        public ActionResult AddUser(HttpPostedFileBase Img, string Name, string Surname, string Username, string Password, string Email, string Tel, string Birthdate)
        {

            string imgpath = null;
            if (!db.users.Any(x => x.username == Username))
            {
                if (Img != null)
                {
                    var fileName = Path.GetFileName(Img.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/UserImg/" + Username + "/"));

                    if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                    {
                        Directory.CreateDirectory(path); // Klasörü oluştur
                        ViewBag.Message = "Yeni klasör başarıyla oluşturuldu.";
                    }
                    else
                    {

                        ViewBag.Message = "Klasör zaten var.";
                    }




                    Img.SaveAs(Path.Combine(Server.MapPath("~/Content/UserImg/" + Username + "/"), fileName));
                    imgpath = "/Content/UserImg/" + Username + "/" + fileName;

                }

                // Resmi bir klasöre kaydetme

                users user = new users() { first_name = Name, last_name = Surname, username = Username, password = Password, email = Email, phone_number = Tel, birth_of_date = DateTime.Parse(Birthdate), avatar = imgpath, role = false, created_at = DateTime.Now, status = true };

                db.users.Add(user);
                db.SaveChanges();
            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return View(model);


        }




        [HttpPost]
        public JsonResult RemoveUser(int Id)
        {
            users user = db.users.Find(Id);
            user.status = false;


            db.SaveChanges();
            var list = db.users
                    .Where(x => x.status == true)
                    .ToList()  // Retrieve data from database before formatting
                    .Select(x => new
                    {
                        x.id,
                        x.first_name,
                        x.last_name,
                        x.phone_number,
                        x.email,
                        x.avatar,
                        created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"
                    })
                    .ToList();
            return Json(list, JsonRequestBehavior.AllowGet);


        }


        public ActionResult EditUser(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (db.users.Any(x => x.id == Id))
            {
                users user = db.users.Find(Id);

                return View(user);
            }
            else
            {
                return HttpNotFound();
            }


        }

        [HttpPost]
        public JsonResult EditUser(HttpPostedFileBase Img, string Id, string Name, string Surname, string Username, string Password, string Email, string Tel, string Birthdate)
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            int userid = int.Parse(Id);
            var user = db.users.Find(userid);

            if (user != null)
            {
                if (Img != null)
                {
                    string imgpath = null;
                    var fileName = Path.GetFileName(Img.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/UserImg/" + Username + "/"));
                    imgpath = "/Content/UserImg/" + Username + "/" + fileName;

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
                user.password = Password;
                user.phone_number = Tel;

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

                return Json(new { success = true, user = user }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UserList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }




        public ActionResult AddCoupon()
        {
            ViewBag.DiscountTypes = new List<SelectListItem>
        {
            new SelectListItem { Text = "Yüzde", Value = "Yüzde" },
            new SelectListItem { Text = "Sabit", Value = "Sabit" }
        };
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }

        [HttpPost]
        public JsonResult AddCoupon(string Code, string Type, string Value, string MinPrice, string MaxPrice, string StartDate, string EndDate, string Categories, string SubCategories, string Products)
        {




            if (!db.coupons.Any(x => x.Code == Code))
            {
                decimal value = 0;

                decimal? min_price = null;
                decimal? max_price = null;


                if (decimal.TryParse(MinPrice.Replace(".", ","), out decimal parsedMinPrice))
                {
                    min_price = parsedMinPrice;
                }


                if (decimal.TryParse(MaxPrice.Replace(".", ","), out decimal parsedMaxPrice))
                {
                    max_price = parsedMaxPrice;
                }
                if (decimal.TryParse(Value.Replace(".", ","), out decimal parsedValue))
                {
                    value = parsedValue;
                }

                coupons coupon = new coupons() { Code = Code, DiscountType = Type, DiscountValue = value, MinimumSpend = min_price, MaxDiscountAmount = max_price, StartDate = DateTime.Parse(StartDate), EndDate = DateTime.Parse(EndDate), CreatedAt = DateTime.Now, IsActive = true };
                db.coupons.Add(coupon);
                db.SaveChanges();
                if (Categories != "")
                {
                    string[] categoryIds = Categories.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] categories = Array.ConvertAll(categoryIds, int.Parse);

                    foreach (var item in categories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.category_id == item && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { category_id = item, coupon_id = coupon.Id };
                            db.coupon_categories.Add(cc);
                            db.SaveChanges();
                        }
                    }

                }

                if (SubCategories != "")
                {
                    string[] subCategoryIds = SubCategories.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] subcategories = Array.ConvertAll(subCategoryIds, int.Parse);

                    foreach (var item in subcategories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.subcategory_id == item && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { subcategory_id = item, coupon_id = coupon.Id };
                            db.coupon_categories.Add(cc);
                            db.SaveChanges();
                        }

                    }
                }
                if (Products != "")
                {
                    string[] productsIds = Products.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] products = Array.ConvertAll(productsIds, int.Parse);

                    foreach (var item in products)
                    {
                        if (!db.coupon_products.Any(cp => cp.product_id == item && cp.coupon_id == coupon.Id))
                        {
                            coupon_products cp = new coupon_products() { product_id = item, coupon_id = coupon.Id };
                            db.coupon_products.Add(cp);
                            db.SaveChanges();
                        }

                    }

                }



                db.SaveChanges();
                return Json(new { success = true, message = "Kupon Eklendi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kupon Kodu Bulunmaktadır" }, JsonRequestBehavior.AllowGet);

            }





        }
        public ActionResult EditCoupon(int id)
        {

            if (!db.coupons.Any(x=>x.Id==id))
            {
                return HttpNotFound();
            }
            
            var discountTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Yüzde", Value = "Yüzde" },
                new SelectListItem { Text = "Sabit", Value = "Sabit" }
            };
            var selectedDiscountType = db.coupons.FirstOrDefault(x=>x.Id==id)?.DiscountType;
            if (selectedDiscountType != "")
            {
                discountTypes.FirstOrDefault(x => x.Value == selectedDiscountType).Selected = true;
            }

            ViewBag.DiscountTypes = discountTypes;
            var coupon = db.coupons.Find(id);
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.Where(x => x.Id == id).Include(c => c.coupon_categories).Include(c => c.coupon_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();

            model.carts = db.cart.ToList();
            return View(model);

        }
        [HttpPost]
        public JsonResult EditCoupon(int Id, string Code, string Type, string Value, string MinPrice, string MaxPrice, string StartDate, string EndDate, string Categories, string SubCategories, string Products)
        {


            if (db.coupons.Any(x => x.Id == Id))
            {
                decimal value = 0;

                decimal? min_price = null;
                decimal? max_price = null;


                if (decimal.TryParse(MinPrice.Replace(".", ","), out decimal parsedMinPrice))
                {
                    min_price = parsedMinPrice;
                }


                if (decimal.TryParse(MaxPrice.Replace(".", ","), out decimal parsedMaxPrice))
                {
                    max_price = parsedMaxPrice;
                }
                if (decimal.TryParse(Value.Replace(".", ","), out decimal parsedValue))
                {
                    value = parsedValue;
                }

                var coupon = db.coupons.FirstOrDefault(x => x.Id == Id);
                coupon.Code = Code;
                coupon.DiscountType = Type;
                coupon.DiscountValue = value;
                coupon.MinimumSpend = min_price;
                coupon.MaxDiscountAmount = max_price;
                coupon.StartDate = DateTime.Parse(StartDate);
                coupon.EndDate = DateTime.Parse(EndDate);

                db.SaveChanges();
                if (db.coupon_categories.Any(x => x.coupon_id == coupon.Id && x.category_id != null))
                {
                    var categoriesToRemove = db.coupon_categories.Where(x => x.coupon_id == coupon.Id && x.category_id != null).ToList();
                    db.coupon_categories.RemoveRange(categoriesToRemove);
                    db.SaveChanges();

                }

                if (Categories != "")
                {
                    string[] categoryIds = Categories.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] categories = Array.ConvertAll(categoryIds, int.Parse);




                    foreach (var item in categories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.category_id == item && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { category_id = item, coupon_id = coupon.Id };
                            db.coupon_categories.Add(cc);
                            db.SaveChanges();
                        }
                    }

                }


                if (db.coupon_categories.Any(x => x.coupon_id == coupon.Id && x.subcategory_id != null))
                {
                    var subcategoriesToRemove = db.coupon_categories.Where(x => x.coupon_id == coupon.Id && x.subcategory_id != null).ToList();
                    db.coupon_categories.RemoveRange(subcategoriesToRemove);
                    db.SaveChanges();

                }
                if (SubCategories != "")
                {
                    string[] subCategoryIds = SubCategories.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] subcategories = Array.ConvertAll(subCategoryIds, int.Parse);



                    foreach (var item in subcategories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.subcategory_id == item && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { subcategory_id = item, coupon_id = coupon.Id };
                            db.coupon_categories.Add(cc);
                            db.SaveChanges();
                        }

                    }
                }
                if (db.coupon_products.Any(x => x.coupon_id == coupon.Id))
                {
                    var productsToRemove = db.coupon_products.Where(x => x.coupon_id == coupon.Id).ToList();
                    db.coupon_products.RemoveRange(productsToRemove);
                    db.SaveChanges();

                }
                if (Products != "")
                {
                    string[] productsIds = Products.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] products = Array.ConvertAll(productsIds, int.Parse);


                    foreach (var item in products)
                    {
                        if (!db.coupon_products.Any(cp => cp.product_id == item && cp.coupon_id == coupon.Id))
                        {
                            coupon_products cp = new coupon_products() { product_id = item, coupon_id = coupon.Id };
                            db.coupon_products.Add(cp);
                            db.SaveChanges();
                        }

                    }

                }






                db.SaveChanges();
                return Json(new { success = true, message = "Kupon Başarıyla Düzenlendi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kupon Kodu Bulunmamaktadır" }, JsonRequestBehavior.AllowGet);

            }





        }
        public ActionResult CouponList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.Include(c => c.coupon_categories).Include(c => c.coupon_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();

            model.carts = db.cart.ToList();
            return View(model);
        }



        [HttpPost]
        public JsonResult RemoveCoupon(int Id)
        {

            if (db.coupons.Any(x => x.Id == Id))
            {

                coupons coupon = db.coupons.Find(Id);

                db.coupon_categories.RemoveRange(coupon.coupon_categories.ToList());
                db.coupon_products.RemoveRange(coupon.coupon_products.ToList());
                db.coupons.Remove(coupon);

                db.SaveChanges();
                var list = db.coupons
                        .Where(x => x.IsActive == true)
                        .ToList()  // Retrieve data from database before formatting
                        .Select(x => new
                        {
                            x.Id,
                            x.Code,
                            x.DiscountType,
                            x.DiscountValue,
                            x.MinimumSpend,
                            x.MaxDiscountAmount,

                            EndDate = x.EndDate != null ? x.EndDate.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                            StartDate = x.StartDate != null ? x.StartDate.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                            CreatedAt = x.CreatedAt != null ? x.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A"



                        })
                        .ToList();
                return Json(new { success = true, message = "Kupon Başarıyla Silindi", coupons = list }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kupon Bulunamadı" }, JsonRequestBehavior.AllowGet);


            }




        }



        public ActionResult AddCampaign()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.campaigns = db.campaigns.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }
        [HttpPost]
        public JsonResult AddCampaign(HttpPostedFileBase Img, string Title, string StartDate, string EndDate, string Products)
        {

            try
            {
                var path = Server.MapPath("~/Content/CampaignCover/" + Title.Replace(" ", "-") + "/");
                String imgPath = null;
                if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                {
                    Directory.CreateDirectory(path); // Klasörü oluştur
                }

                if (Img != null)
                {

                    if (Img.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(Img.FileName);
                        var filePath = Path.Combine(path, fileName);
                        Img.SaveAs(filePath);
                    }

                    imgPath = "/Content/CampaignCover/" + Title.Replace(" ", "-") + "/" + Path.GetFileName(Img.FileName);

                }


                // Resmi bir klasöre kaydetme

                campaigns campaign = new campaigns() { campaign_cover = imgPath, campaign_title = Title, campaign_start_date = DateTime.Parse(StartDate), campaign_end_date = DateTime.Parse(EndDate) };
                db.campaigns.Add(campaign);
                db.SaveChanges();



                if (Products != "")
                {
                    string[] productsIds = Products.Split(',');

                    // String dizisini int dizisine dönüştür
                    int[] products = Array.ConvertAll(productsIds, int.Parse);

                    foreach (var item in products)
                    {
                        if (!db.campaign_products.Any(cp => cp.product_id == item && cp.product_id == campaign.id))
                        {
                            campaign_products cp = new campaign_products() { product_id = item, camapign_id = campaign.id };
                            db.campaign_products.Add(cp);
                            db.SaveChanges();
                        }

                    }

                }


                AllViewModel model = new AllViewModel();
                model.products = db.products.ToList();
                model.users = db.users.ToList();
                model.campaigns = db.campaigns.Include(x => x.campaign_products).ToList();
                model.categories = db.categories.ToList();
                model.carts = db.cart.ToList();

                return Json(new { success = true, message = "Kampanya Başarıyla Eklendi" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Kampanya Eklenirken Bir Hata Oluştu" }, JsonRequestBehavior.AllowGet);

                throw;
            }



        }

        public ActionResult CampaignList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.campaigns = db.campaigns.Include(c => c.campaign_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();

            model.carts = db.cart.ToList();
            return View(model);
        }



        [HttpPost]
        public JsonResult RemoveCampaign(int Id)
        {

            if (db.campaigns.Any(x => x.id == Id))
            {

                campaigns campaign = db.campaigns.Find(Id);

                try
                {

                    if (campaign.campaign_cover != "" || campaign.campaign_cover != null)
                    {

                        string[] parts = campaign.campaign_cover.Split('/');
                        string targetPart = parts.FirstOrDefault(part => part == "CampaignCover");
                        if (targetPart != null)
                        {
                            var dir = new DirectoryInfo(campaign.campaign_cover);

                            string directoryPath = Server.MapPath(System.IO.Path.GetDirectoryName(campaign.campaign_cover).Replace("\\", "/"));
                            Directory.Delete(directoryPath, true);
                        }

                    }
                }

                catch (IOException ex)
                {
                }

                db.campaign_products.RemoveRange(campaign.campaign_products.ToList());

                db.campaigns.Remove(campaign);

                db.SaveChanges();
                var list = db.campaigns
                        .ToList()
                        .Select(x => new
                        {
                            x.id,
                            x.campaign_title,
                            x.campaign_cover,
                            EndDate = x.campaign_end_date != null ? x.campaign_end_date.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                            StartDate = x.campaign_start_date != null ? x.campaign_start_date.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",




                        })
                        .ToList();
                return Json(new { success = true, message = "Kampanya Başarıyla Silindi", campaigns = list }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kampanya Bulunamadı" }, JsonRequestBehavior.AllowGet);


            }




        }



        public ActionResult EditCampaign(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            campaigns campaign = db.campaigns.Find(Id);

            if (campaign == null)
            {
                return HttpNotFound();
            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.ToList();
            model.campaigns = db.campaigns.Where(x => x.id == Id).Include(x => x.campaign_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        [HttpPost]
        public ActionResult EditCampaign(HttpPostedFileBase Img, int Id, string Title, string StartDate, string EndDate, string Products)
        {

            if (db.campaigns.Any(x => x.id == Id))
            {

                if (Title != "")
                {


                    if (Img!=null)
                    {

                       

                        campaigns campaign = db.campaigns.Find(Id);



                        campaign.campaign_start_date = DateTime.Parse(StartDate);
                        campaign.campaign_end_date = DateTime.Parse(EndDate);




                        var path = Server.MapPath("~/Content/CampaignCover/" + Title.Replace(" ", "-") + "/");
                        String imgPath = null;
                        if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                        {
                            Directory.CreateDirectory(path); // Klasörü oluştur
                            if (Title != campaign.campaign_title)
                            {
                                var oldpath = Server.MapPath("~/Content/CampaignCover/" + campaign.campaign_title.Replace(" ", "-") + "/");

                                if (Directory.Exists(oldpath))
                                {


                                    Directory.Delete(oldpath, true);



                                }
                            }

                        }
                        else
                        {

                            if (Directory.Exists(path))
                            {
                                // Klasördeki tüm dosyaları sil
                                foreach (string file in Directory.GetFiles(path))
                                {
                                    System.IO.File.Delete(file);
                                }

                                // Klasördeki tüm alt klasörleri ve onların içeriğini sil
                                foreach (string subDirectory in Directory.GetDirectories(path))
                                {
                                    Directory.Delete(subDirectory, true);
                                }
                            }
                        }
                        campaign.campaign_title = Title;

                        if (Img != null)
                        {

                            if (Img.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(Img.FileName);
                                var filePath = Path.Combine(path, fileName);
                                Img.SaveAs(filePath);
                            }

                            imgPath = "/Content/CampaignCover/" + Title.Replace(" ", "-") + "/" + Path.GetFileName(Img.FileName);

                        }


                        // Resmi bir klasöre kaydetme

                        campaign.campaign_cover = imgPath;
                        db.SaveChanges();

                        if (campaign.campaign_products.Any())
                        {
                            db.campaign_products.RemoveRange(campaign.campaign_products);
                            db.SaveChanges();
                        }

                        if (Products != "")
                        {
                            string[] productsIds = Products.Split(',');

                            // String dizisini int dizisine dönüştür
                            int[] products = Array.ConvertAll(productsIds, int.Parse);

                            foreach (var item in products)
                            {
                                if (!db.campaign_products.Any(cp => cp.product_id == item && cp.product_id == campaign.id))
                                {
                                    campaign_products cp = new campaign_products() { product_id = item, camapign_id = campaign.id };
                                    db.campaign_products.Add(cp);
                                    db.SaveChanges();
                                }

                            }

                        }


                        var list = db.campaigns
                          .ToList()
                          .Select(x => new
                          {
                              x.id,
                              x.campaign_title,
                              x.campaign_cover,
                              EndDate = x.campaign_end_date != null ? x.campaign_end_date.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                              StartDate = x.campaign_start_date != null ? x.campaign_start_date.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",




                          })
                          .ToList();
                        return Json(new { success = true, message = "Kampanya Başarıyla Düzenlendi.", campaigns = list });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Resim Eklemeniz Gerekmektedir." });

                    }


                }
                else
                {
                    return Json(new { success = false, message = "Başlık Kısmı Boş Geçilemez." });

                }
            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kampanya Mevcut Değil." });

            }



        }



    }
}