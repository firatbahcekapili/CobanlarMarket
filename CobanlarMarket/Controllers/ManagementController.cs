using CobanlarMarket.Models;
using Iyzipay.Model;
using Iyzipay.Request;
using Iyzipay;
using Iyzipay.Model.V2;
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
using Iyzipay.Model.V2.Subscription;
using System.Web.WebPages;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using System.Data.Entity.Core;
using System.Data.Entity.Validation;
using System.Numerics;
using HtmlAgilityPack;
using MailKit.Security;
using MimeKit;

namespace CobanlarMarket.Controllers
{
    public class ManagementController : Controller
    {

        private CobanlarMarketEntities db = new CobanlarMarketEntities();
        private readonly string seoFilePath;
        private EmailService _emailService = new EmailService();

        // GET: Management
        public ActionResult Dashboard()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();

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
        [ValidateAntiForgeryToken]
        public JsonResult AddProduct(IEnumerable<HttpPostedFileBase> Imgs, string Name, string SubCategory, string SubSubCategory, int categoryType, string Description, string Size, string Color, int Quantity, string Price, string OldPrice)
        {

            var encodedName = EncodeFileName(Name);

            var path = Server.MapPath("~/Content/ProductImg/" + encodedName + "/");
            List<string> imgPaths = new List<string>();
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
                        var originalFileName = Path.GetFileNameWithoutExtension(img.FileName);
                        var extension = Path.GetExtension(img.FileName);

                        // Dosya adını encode et
                        var encodedFileName = EncodeFileName(originalFileName) + extension;

                        var filePath = Path.Combine(path, encodedFileName);
                        img.SaveAs(filePath);

                        imgPaths.Add("/Content/ProductImg/" + encodedName + "/" + encodedFileName);
                    }
                }
            }

            else
            {
                return Json(new { success = false, message = "En az 1 resim eklemelisiniz!" });
            }







            try
            {

                if (SubSubCategory == "null")
                {
                    SubSubCategory = null;
                }


                products product = new products() { name = Name, subcategory_id = int.Parse(SubCategory), sub_subcategory_id = SubSubCategory == "null" ? null : SubSubCategory, category_type = categoryType, cover = imgPaths == null ? null : imgPaths[0], description = Description, created_at = DateTime.Now, status = true };
                if (!TryValidateModel(product))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();

                    return Json(new { success = false, message = errors });
                }
                db.products.Add(product);
                db.SaveChanges();

                decimal? price = null;
                decimal? old_price = null;

                if (!string.IsNullOrEmpty(OldPrice) && decimal.TryParse(OldPrice.Replace(".", ","), out decimal parsedOldPrice))
                {
                    old_price = parsedOldPrice;
                }

                if (!string.IsNullOrEmpty(Price) && decimal.TryParse(Price.Replace(".", ","), out decimal parsedPrice))
                {
                    price = parsedPrice;
                }
                string Sku = (Name + product.id).Replace(" ", "");

                products_skus products_skus = new products_skus() { product_id = product.id, size_attribute_id = -1, color_attribute_id = -1, sku = Sku, quantity = Quantity, price = price, old_price = old_price, created_at = DateTime.Now };
                if (!TryValidateModel(products_skus))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();
                    db.products.Remove(product);
                    db.SaveChanges();

                    return Json(new { success = false, message = errors });
                }
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

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });

            }



            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return Json(new { success = true, message = "Ürün başarıyla Eklendi!" });


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
        public JsonResult GetAllSubCategories()
        {

            var SubCategories = db.sub_categories
                                  .Select(c => new
                                  {
                                      c.id,
                                      c.name,
                                      c.description,
                                      c.parent_id
                                  })
                                  .ToList();

            return Json(SubCategories, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetAllSubSubCategories()
        {

            var SubCategories = db.sub_subcategories
                                  .Select(c => new
                                  {
                                      c.id,
                                      c.name,
                                      c.description,
                                      c.parent_sub_category_id
                                  })
                                  .ToList();

            return Json(SubCategories, JsonRequestBehavior.AllowGet);
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
        [ValidateAntiForgeryToken]
        public JsonResult RemoveProduct(int Id)
        {
            products product = db.products.Find(Id);
            product.status = false;

            //if (!db.order_item.Any(x=>x.product_id==product.id))
            //{
            //    foreach (var item in product.product_images.ToList())
            //    {
            //        var path = Server.MapPath(item.image_path);

            //        if (System.IO.File.Exists(path))
            //        {
            //            // Dosyayı sil
            //            System.IO.File.Delete(path);
            //            db.product_images.Remove(item);
            //        }

            //        // Klasörün yolunu al (Dosya yolundan klasör yolunu elde etmek için)
            //        var directoryPath = Path.GetDirectoryName(path);

            //        // Klasörde başka dosya kalmadıysa, klasörü sil
            //        if (Directory.Exists(directoryPath) && !Directory.EnumerateFiles(directoryPath).Any())
            //        {
            //            Directory.Delete(directoryPath);
            //        }
            //    }
            //}


            db.SaveChanges();
            var list = db.products
                    .Where(x => x.status == true)
                    .ToList()
                    .Select(x => new
                    {
                        x.id,
                        x.name,
                        x.description,
                        x.summary,
                        x.cover,
                        x.subcategory_id,
                        created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                        product_skus_count = x.products_skus.Count(),
                        product_sku_quantity = x.products_skus.FirstOrDefault().quantity,
                        product_sku = x.products_skus.FirstOrDefault().sku,
                        sold_count = x.order_item.Where(o => o.status == "success" && o.product_id == x.id).Sum(y => y.quantity)

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
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.id == Id).ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();

            products product = db.products.Find(Id);

            if (product == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(IEnumerable<HttpPostedFileBase> Imgs, int Id, string Name, int? SubCategory, string SubSubCategory, int categoryType, string Description, int Quantity, string Price, string OldPrice)
        {
            try
            {
                if (db.products.Any(x => x.id == Id))
                {
                    var product = db.products.Find(Id);

                    if (SubSubCategory == "null")
                    {
                        SubSubCategory = null;
                    }


                    products tempproduct = new products() { name = Name, cover = product.cover, subcategory_id = SubCategory, sub_subcategory_id = SubSubCategory == "null" ? null : SubSubCategory, category_type = categoryType, status = true };

                    if (!TryValidateModel(tempproduct))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                      .Select(e => e.ErrorMessage)
                                                      .ToList();
                        return Json(new { success = false, message = errors });
                    }

                    if (product != null)
                    {
                        product.name = Name;
                        product.description = Description;
                        product.subcategory_id = SubCategory;
                        product.sub_subcategory_id = SubSubCategory;

                        product.category_type = categoryType;
                        product.products_skus.FirstOrDefault().size_attribute_id =-1;
                        product.products_skus.FirstOrDefault().color_attribute_id =  -1;
                        if (product.products_skus.FirstOrDefault().sku.IsNullOrWhiteSpace())
                        {
                            product.products_skus.FirstOrDefault().sku= (Name + product.id).Replace(" ", "");
                        }
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



                        var encodedName = EncodeFileName(Name);



                        var path = Server.MapPath("~/Content/ProductImg/" + encodedName + "/");

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var existingFiles = db.product_images
                            .Where(p => p.product_id == product.id)
                            .Select(p => p.image_path)
                            .ToList();

                        var savedFiles = new List<string>();
                        var skippedFiles = new List<string>();  // Mevcut dosyalar için yeni liste
                        bool hasError = false;
                        var errorMessage = "";

                        if (Imgs != null && Imgs.Any())
                        {
                            foreach (var img in Imgs)
                            {
                                if (img != null && img.ContentLength > 0)
                                {

                                    var originalFileName = Path.GetFileNameWithoutExtension(img.FileName);
                                    var extension = Path.GetExtension(img.FileName);

                                    // Dosya adını encode et
                                    var encodedFileName = EncodeFileName(originalFileName) + extension;

                                    var filePath = Path.Combine(path, encodedFileName);




                                    var imgPath = "/Content/ProductImg/" + encodedName + "/" + encodedFileName;

                                    if (!System.IO.File.Exists(filePath))
                                    {
                                        // Eğer dosya yoksa kaydet
                                        img.SaveAs(filePath);

                                        if (!db.product_images.Any(p => p.image_path == imgPath))
                                        {
                                            db.product_images.Add(new product_images { product_id = product.id, image_path = imgPath });
                                            db.SaveChanges();
                                        }

                                        savedFiles.Add(imgPath);
                                    }
                                    else
                                    {
                                        // Dosya zaten varsa listeye ekle ve hata vermeden devam et
                                        skippedFiles.Add(encodedFileName);
                                    }
                                }
                            }

                            // Eğer resim yüklendiyse cover'ı ilk resim ile güncelle
                            if (product.product_images.Any())
                            {
                                product.cover = product.product_images.OrderByDescending(x => x.id).LastOrDefault().image_path;
                            }
                        }

                        if (!TryValidateModel(product))
                        {
                            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                          .Select(e => e.ErrorMessage)
                                                          .ToList();

                            return Json(new { success = false, message = errors });
                        }

                        db.SaveChanges();

                        var list = db.product_images
                            .Where(x => x.product_id == Id)
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
                            return Json(new
                            {
                                success = true,
                                message = "Ürün başarıyla güncellendi!",
                                savedFiles = savedFiles,   // Kaydedilen dosyalar
                                skippedFiles = skippedFiles, // Atlanan (zaten var olan) dosyalar
                                files = list
                            });
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
            if (db.product_images.Where(x => x.product_id == img.product_id).Count() == 1)
            {
                return Json(new { success = false, message = "En az bir resim bulunmak zorundadır." }, JsonRequestBehavior.AllowGet);
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

                }
                else
                {
                    // Dosya bulunamadıysa 404 döndür
                    return Json(new { success = false, message = "Resim Bulunamadı" }, JsonRequestBehavior.AllowGet);
                }


                db.SaveChanges();

                // Başarı mesajı döndür
                return Json(new { success = true, message = "Resim başarıyla silindi" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

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
            model.order_item = db.order_item.ToList();
            return View(model);
        }






        public ActionResult AddCategory()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddCategory(HttpPostedFileBase Img, string Name, string Description)
        {

            string imgpath = null;
            if (!db.categories.Any(x => x.name == Name))
            {
                if (Img != null)


                {
                    var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                    var extension = Path.GetExtension(Img.FileName);

                    // Dosya adını encode et
                    var fileName = EncodeFileName(originalFileName) + extension;
                    var folderName = EncodeFileName(Name);

                    var path = Path.Combine(Server.MapPath("~/Content/CategoriesImg/" + folderName + "/"));

                    if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                    {
                        Directory.CreateDirectory(path); // Klasörü oluştur
                        ViewBag.Message = "Yeni klasör başarıyla oluşturuldu.";
                    }
                    else
                    {

                        ViewBag.Message = "Klasör zaten var.";
                    }




                    Img.SaveAs(Path.Combine(Server.MapPath("~/Content/CategoriesImg/" + folderName + "/"), fileName));
                    imgpath = "/Content/CategoriesImg/" + folderName + "/" + fileName;




                }
                if (Name != "")
                {
                    categories category = new categories()
                    {
                        name = Name,
                        cover = imgpath,
                        description = Description,
                        created_at = DateTime.Now,
                    };

                    if (!TryValidateModel(category))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();

                        return Json(new { success = false, message = errors });
                    }



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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                            subCategory.parent_id = null;


                        }

                        // Alt kategorileri güncelle
                        foreach (var subCategory in category.sub_categories.ToList())
                        {
                            subCategory.parent_id = null;


                        }

                        foreach (var cc in db.coupon_categories.Where(x => x.category_id == Id))
                        {
                            cc.category_id = null;
                        }





                        db.categories.Remove(category);


                        db.SaveChanges();

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
                        return Json(new { success = true, message = "Kategori Başarıyla Silindi.", list = listt });

                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();

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
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(HttpPostedFileBase Img, int Id, string Name, string Description)
        {

            if (db.categories.Any(x => x.id == Id))
            {

                if (!Name.IsNullOrWhiteSpace())
                {

                    string imgPath = null;




                    categories category = db.categories.Find(Id);
                    categories tempcategory = new categories();

                    category.name = Name;
                    category.description = Description;

                    tempcategory.name = Name;
                    tempcategory.description = Description;
                    tempcategory.cover = "null";
                    if (!TryValidateModel(tempcategory))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();

                        return Json(new { success = false, message = errors });
                    }






                    if (Img != null)
                    {

                        if (Img != null && Img.ContentLength > 0)
                        {


                            var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                            var extension = Path.GetExtension(Img.FileName);

                            // Dosya adını encode et
                            var fileName = EncodeFileName(originalFileName) + extension;
                            var folderName = EncodeFileName(Name);

                            var path = Server.MapPath("~/Content/CategoriesImg/" + folderName + "/");

                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }



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
                            imgPath = "/Content/CategoriesImg/" + folderName + "/" + fileName;


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
                    return Json(new { success = false, message = "İsim Boş Geçilemez." });

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

                return Json(new { success = false, message = "Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                try
                {
                    category.cover = null;
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {

                    return Json(new { success = false, message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage }, JsonRequestBehavior.AllowGet);

                }

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
        [ValidateAntiForgeryToken]
        public JsonResult EditSubCategory(int Id, int ParentId, string Name, string Description)
        {

            if (db.sub_categories.Any(x => x.id == Id))
            {

                if (Name != null && Description != null)
                {
                    sub_categories subcategory = db.sub_categories.Find(Id);

                    sub_categories tempsub = new sub_categories();
                    tempsub.name = Name;
                    tempsub.description = Description;
                    tempsub.parent_id = ParentId;

                    if (!TryValidateModel(tempsub))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();

                        return Json(new { success = false, message = errors });
                    }


                    subcategory.name = Name;
                    subcategory.description = Description;
                    subcategory.parent_id = ParentId;
                    db.SaveChanges();





                    db.SaveChanges();
                    return Json(new { success = true, message = "Alt Kategori başarıyla düzenlendi." });
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

        public ActionResult EditSubSubCategory(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            sub_subcategories subcategory = db.sub_subcategories.Find(Id.ToString());

            if (subcategory == null)
            {
                return HttpNotFound();
            }
            return View(subcategory);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditSubSubCategory(int Id, int ParentId, string Name, string Description)
        {

            if (db.sub_subcategories.Any(x => x.id == Id.ToString()))
            {

                if (Name != null && Description != null)
                {
                    sub_subcategories subcategory = db.sub_subcategories.Find(Id.ToString());

                    sub_subcategories tempsub = new sub_subcategories();
                    tempsub.name = Name;
                    tempsub.description = Description;
                    tempsub.parent_sub_category_id = ParentId;

                    if (!TryValidateModel(tempsub))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                                                .ToList();

                        return Json(new { success = false, message = errors });
                    }


                    subcategory.name = Name;
                    subcategory.description = Description;
                    subcategory.parent_sub_category_id = ParentId;


                    db.SaveChanges();

                    foreach (var item in db.products.Where(x => x.sub_subcategory_id == subcategory.id))
                    {
                        item.subcategory_id = ParentId;

                    }
                    db.SaveChanges();

                    return Json(new { success = false, message = "Alt Alt Kategori başarıyla düzenlendi." });
                }
            }
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            return Json(new { redirectUrl = Url.Action("CategoryList", "Management") });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveSubCategory(int Id)
        {
            using (db)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var category = db.sub_categories.FirstOrDefault(c => c.id == Id);

                        if (category == null)
                        {
                            return Json(new { success = false, message = "Alt Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
                        }


                        foreach (var product in db.products.Where(x => x.subcategory_id == Id))
                        {
                            product.subcategory_id = null;
                        }
                        foreach (var cc in db.coupon_categories.Where(x => x.subcategory_id == Id))
                        {
                            cc.subcategory_id = null;
                        }
                        foreach (var cc in db.sub_subcategories.Where(x => x.parent_sub_category_id == Id))
                        {
                            cc.parent_sub_category_id = null;
                        }



                        db.sub_categories.Remove(category);

                        db.SaveChanges();

                        transaction.Commit();



                        return Json(new { success = true, message = "Alt Kategori Başarıyla Silindi." });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveSubSubCategory(int Id)
        {
            using (db)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var category = db.sub_subcategories.FirstOrDefault(c => c.id == Id.ToString());

                        if (category == null)
                        {
                            return Json(new { success = false, message = "Alt Alt Kategori Bulunamadı" }, JsonRequestBehavior.AllowGet);
                        }

                        foreach (var product in db.products.Where(x => x.sub_subcategory_id == Id.ToString()))
                        {
                            product.sub_subcategory_id = null;

                        }




                        db.sub_subcategories.Remove(category);

                        db.SaveChanges();

                        transaction.Commit();


                        //var listt = db.sub_categories
                        // .ToList()
                        // .Select(x => new
                        // {
                        //     x.id,
                        //     x.name,
                        //     x.description,
                        //     created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",
                        //     parentCategory = db.categories.FirstOrDefault(c => c.id == x.parent_id) == null ? "Kategori Yok" : db.categories.FirstOrDefault(c => c.id == x.parent_id).name
                        // })
                        // .ToList();
                        return Json(new { success = true, message = "Alt Alt Kategori Başarıyla Silindi." });

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddSubCategory(int ParentId, string Name, string Description)
        {

            if (db.categories.Any(x => x.id == ParentId))
            {

                if (Name != "")
                {
                    sub_categories subcategory = new sub_categories()
                    {
                        parent_id = ParentId,
                        name = Name,
                        description = Description,
                        created_at = DateTime.Now,
                    };
                    if (!TryValidateModel(subcategory))
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage)
                                                 .ToList();

                        return Json(new { success = false, message = errors });
                    }

                    db.sub_categories.Add(subcategory);
                    db.SaveChanges();

                    var listt = db.sub_categories
                .ToList()  // Retrieve data from database before formatting
                .Select(x => new
                {
                    x.id,
                    x.name,
                    x.description,

                })
                .ToList();

                    return Json(new { success = true, message = "Alt Kategori başarıyla eklendi.", list = listt });

                }
                else
                {
                    return Json(new { success = false, message = "İsim boş geçilemez." });

                }

            }
            else
            {
                return Json(new { success = false, message = "Böyle bir kategori bulunamadı." });

            }


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddSubSubCategory(int ParentId, string Name, string Description)
        {
            if (db.sub_categories.Any(x => x.id == ParentId))
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {

                    var lastId = db.sub_subcategories
                                   .OrderByDescending(x => x.id)
                                   .FirstOrDefault()?.id;

                    int newId = 1;

                    if (!string.IsNullOrEmpty(lastId))
                    {

                        newId = int.Parse(lastId) + 1;
                    }


                    while (db.sub_subcategories.Any(x => x.id == newId.ToString()))
                    {
                        newId++;
                    }


                    sub_subcategories subsubcategory = new sub_subcategories()
                    {
                        id = newId.ToString(),
                        parent_sub_category_id = ParentId,
                        name = Name,
                        description = Description,
                        created_at = DateTime.Now,
                    };

                    try
                    {
                        if (!TryValidateModel(subsubcategory))
                        {
                            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                          .Select(e => e.ErrorMessage)
                                                          .ToList();

                            return Json(new { success = false, message = errors });
                        }

                        db.sub_subcategories.Add(subsubcategory);
                        db.SaveChanges();
                        return Json(new { success = true, message = "Alt Alt Kategori başarıyla eklendi." });
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        var validationErrors = ex.EntityValidationErrors
                            .SelectMany(e => e.ValidationErrors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

                        return Json(new { success = false, message = validationErrors });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "İsim boş geçilemez." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Böyle bir kategori bulunamadı." });
            }
        }



        public ActionResult CategoryList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();
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
        [ValidateAntiForgeryToken]
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
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                        db.SaveChanges();
                        db.product_attributes.Remove(attribute);
                        db.SaveChanges();
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
        [ValidateAntiForgeryToken]
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


        public ActionResult OrderDetail(int? Id)
        {


            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            order_details order = db.order_details.FirstOrDefault(x => x.id == Id);

            if (order == null)
            {
                return HttpNotFound();
            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.order_details = db.order_details.Where(x => x.id == Id).Include(x => x.order_item).ToList();
            model.order_item = db.order_item.Where(x => x.order_id == Id).Include(x => x.products).Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.payment_details = db.payment_details.Where(x => x.order_id == Id).ToList();
            model.coupons = db.coupons.ToList();
            model.addresses = db.addresses.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditReferenceId(String ReferenceId, int OrderId)
        {


            if (db.order_details.Any(x => x.id == OrderId))
            {
                var order = db.order_details.FirstOrDefault(x => x.id == OrderId);
                if (long.TryParse(ReferenceId, out long referenceid))
                {
                    order.reference_id = referenceid;
                    db.SaveChanges();
                }
                else
                {
                    return Json(new { success = false, message = "Bir hata oluştu." }, JsonRequestBehavior.AllowGet);

                }


                return Json(new { success = true, message = "Takip kodu başarıyla düzenlendi." }, JsonRequestBehavior.AllowGet);

            }
            else
            {

                return Json(new { success = false, message = "Sipariş bulunamadı." }, JsonRequestBehavior.AllowGet);

            }



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
            model.payment_details = db.payment_details.ToList();
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
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Şifre Doğrulama
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


        [HttpPost]
        public ActionResult AddUser(HttpPostedFileBase Img, string Name, string Surname, string Username, string Password, string Email, string Tel, string Birthdate)
        {

            string imgpath = null;
            if (!db.users.Any(x => x.username == Username))
            {
                if (db.users.Any(x => x.email == Email))
                {
                    return Json(new { success = false, message = "Bu E-Maile sahip bir kullanıcı bulunmaktadır." }, JsonRequestBehavior.AllowGet);

                }
                if (db.users.Any(x => x.phone_number == Tel))
                {
                    return Json(new { success = false, message = "Bu telefon numarasına sahip bir kullanıcı bulunmaktadır." }, JsonRequestBehavior.AllowGet);

                }
                users tempuser = new users() { first_name = Name, last_name = Surname, username = Username, password = HashPassword(Password), email = Email, phone_number = Tel, birth_of_date = DateTime.Parse(Birthdate), avatar = imgpath, role = false, created_at = DateTime.Now, status = true };

                if (!TryValidateModel(tempuser))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();


                    return Json(new { success = false, message = errors });
                }

                if (Img != null)
                {

                    var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                    var extension = Path.GetExtension(Img.FileName);

                    // Dosya adını encode et
                    var fileName = EncodeFileName(originalFileName) + extension;
                    var folderName = EncodeFileName(Username);
                    var path = Path.Combine(Server.MapPath("~/Content/UserImg/" + folderName + "/"));

                    if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                    {
                        Directory.CreateDirectory(path); // Klasörü oluştur
                        ViewBag.Message = "Yeni klasör başarıyla oluşturuldu.";
                    }
                    else
                    {

                        ViewBag.Message = "Klasör zaten var.";
                    }




                    Img.SaveAs(Path.Combine(Server.MapPath("~/Content/UserImg/" + folderName + "/"), fileName));
                    imgpath = "/Content/UserImg/" + folderName + "/" + fileName;

                }


                users user = new users() { first_name = Name, last_name = Surname, username = Username, password = Password, email = Email, phone_number = Tel, birth_of_date = DateTime.Parse(Birthdate), avatar = imgpath, role = false, created_at = DateTime.Now, status = true };







                db.users.Add(user);

                db.SaveChanges();
                cart cart = new cart()
                {
                    user_id = user.id,
                    created_at = DateTime.Now
                };
                db.cart.Add(cart);
                db.SaveChanges();
            }
            else
            {
                return Json(new { success = false, message = "Bu kullanıcı adına sahip bir kullanıcı bulunmaktadır." }, JsonRequestBehavior.AllowGet);

            }

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();

            return Json(new { success = true, message = "Kullanıcı başarıyla eklendi." }, JsonRequestBehavior.AllowGet);


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
                        return Json(new { success = false, message = "Şifre en az 8 karakterden oluşmalıdır." }, JsonRequestBehavior.AllowGet);

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
                    }
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Kullanıcı bulunamadı" }, JsonRequestBehavior.AllowGet);
        }

     
        public ActionResult UserList()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.Include(x=>x.order_details).ToList();
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
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();

            model.carts = db.cart.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddCoupon(string Code, string Type, string Value, string MinPrice, string MaxPrice, string StartDate, string EndDate, string CategoryIds, string Products)
        {
            List<int> categories = new List<int>(); ;
            List<int> subcategories = new List<int>(); ;
            List<int> subsubcategories = new List<int>(); ;

            var cIds = CategoryIds.Split(',');

            foreach (var c in cIds)
            {
                var details = c.Split('-');
                if (details[0] == "c")
                {
                    categories.Add(int.Parse(details[1]));
                }
                else if (details[0] == "sc")
                {
                    subcategories.Add(int.Parse(details[1]));

                }
                else if (details[0] == "ssc")
                {
                    subsubcategories.Add(int.Parse(details[1]));

                }

            }


            if (!db.coupons.Any(x => x.Code == Code && x.IsActive == true))
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

                coupons coupon = new coupons() { Code = Code, DiscountType = Type, DiscountValue = value, MinimumSpend = min_price, MaxDiscountAmount = max_price, StartDate = DateTime.Parse(StartDate), EndDate = DateTime.Parse(EndDate), CreatedAt = DateTime.Now, IsActive = true, Status = true };

                if (!TryValidateModel(coupon))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();


                    return Json(new { success = false, message = errors });
                }


                db.coupons.Add(coupon);
                db.SaveChanges();





                if (categories.Count() != 0)
                {

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

                if (subcategories.Count() != 0)
                {


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


                if (subsubcategories.Count() != 0)
                {


                    foreach (var item in subsubcategories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.sub_subcategory_id == item.ToString() && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { sub_subcategory_id = item.ToString(), coupon_id = coupon.Id };
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

            if (!db.coupons.Any(x => x.Id == id))
            {
                return HttpNotFound();
            }

            var discountTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Yüzde", Value = "Yüzde" },
                new SelectListItem { Text = "Sabit", Value = "Sabit" }
            };
            var selectedDiscountType = db.coupons.FirstOrDefault(x => x.Id == id)?.DiscountType;
            if (selectedDiscountType != "")
            {
                discountTypes.FirstOrDefault(x => x.Value == selectedDiscountType).Selected = true;
            }

            ViewBag.DiscountTypes = discountTypes;
            var coupon = db.coupons.Find(id);
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.Where(x => x.Id == id).Include(c => c.coupon_categories).Include(c => c.coupon_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.sub_subcategories = db.sub_subcategories.ToList();


            model.carts = db.cart.ToList();
            return View(model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditCoupon(int Id, string Code, string Type, string Value, string MinPrice, string MaxPrice, string StartDate, string EndDate, string CategoryIds, string Products, string Status)
        {
            List<int> categories = new List<int>(); ;
            List<int> subcategories = new List<int>(); ;
            List<int> subsubcategories = new List<int>(); ;

            var cIds = CategoryIds.Split(',');

            foreach (var c in cIds)
            {
                var details = c.Split('-');
                if (details[0] == "c")
                {
                    categories.Add(int.Parse(details[1]));
                }
                else if (details[0] == "sc")
                {
                    subcategories.Add(int.Parse(details[1]));

                }
                else if (details[0] == "ssc")
                {
                    subsubcategories.Add(int.Parse(details[1]));

                }

            }

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

                var tempcoupon = db.coupons.FirstOrDefault(x => x.Id == Id);
                tempcoupon.Code = Code;
                tempcoupon.DiscountType = Type;
                tempcoupon.DiscountValue = value;
                tempcoupon.MinimumSpend = min_price;
                tempcoupon.MaxDiscountAmount = max_price;
                tempcoupon.StartDate = DateTime.Parse(StartDate);
                tempcoupon.EndDate = DateTime.Parse(EndDate);
                tempcoupon.IsActive = Status == "Aktif" ? true : false;
                if (!TryValidateModel(tempcoupon))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();


                    return Json(new { success = false, message = errors });
                }


                var coupon = db.coupons.FirstOrDefault(x => x.Id == Id);
                coupon.Code = Code;
                coupon.DiscountType = Type;
                coupon.DiscountValue = value;
                coupon.MinimumSpend = min_price;
                coupon.MaxDiscountAmount = max_price;
                coupon.StartDate = DateTime.Parse(StartDate);
                coupon.EndDate = DateTime.Parse(EndDate);
                //Default Pasif
                tempcoupon.IsActive = Status == "Aktif" ? true : false;

                db.SaveChanges();
                if (db.coupon_categories.Any(x => x.coupon_id == coupon.Id && x.category_id != null))
                {
                    var categoriesToRemove = db.coupon_categories.Where(x => x.coupon_id == coupon.Id).ToList();
                    db.coupon_categories.RemoveRange(categoriesToRemove);
                    db.SaveChanges();

                }
                if (categories.Count() != 0)
                {

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

                if (subcategories.Count() != 0)
                {


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


                if (subsubcategories.Count() != 0)
                {


                    foreach (var item in subsubcategories)
                    {
                        if (!db.coupon_categories.Any(cc => cc.sub_subcategory_id == item.ToString() && cc.coupon_id == coupon.Id))
                        {
                            coupon_categories cc = new coupon_categories() { sub_subcategory_id = item.ToString(), coupon_id = coupon.Id };
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
            var expiredCoupons = db.coupons
               .Where(c => c.EndDate <= DateTime.Now && c.IsActive == true)
           .ToList();

            expiredCoupons.ForEach(c => c.IsActive = false);

            db.SaveChanges();

            AllViewModel model = new AllViewModel();
            model.products = db.products.ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.Where(c => c.Status == true).Include(c => c.coupon_categories).Include(c => c.coupon_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();

            model.carts = db.cart.ToList();
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveCoupon(int Id)
        {

            if (db.coupons.Any(x => x.Id == Id))
            {

                coupons coupon = db.coupons.Find(Id);

                //db.coupon_categories.RemoveRange(coupon.coupon_categories.ToList());
                //db.coupon_products.RemoveRange(coupon.coupon_products.ToList());
                //db.coupons.Remove(coupon);

                coupon.Status = false;
                coupon.IsActive = false;

                db.SaveChanges();

                return Json(new { success = true, message = "Kupon Başarıyla Silindi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kupon Bulunamadı" }, JsonRequestBehavior.AllowGet);


            }




        }



        public ActionResult AddCampaign()
        {
            AllViewModel model = new AllViewModel();
            model.products = db.products.Where(x => x.status == true).ToList();
            model.product_attributes = db.product_attributes.ToList();
            model.users = db.users.ToList();
            model.campaigns = db.campaigns.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddCampaign(HttpPostedFileBase Img, string Title, string StartDate, string EndDate, string Products)
        {

            try
            {


                if (Img == null)
                {
                    return Json(new { success = false, message = "Kampanya resmi eklenmesi zorunludur." }, JsonRequestBehavior.AllowGet);

                }


                var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                var extension = Path.GetExtension(Img.FileName);

                // Dosya adını encode et
                var fileName = EncodeFileName(originalFileName) + extension;
                var encodedTitle = EncodeFileName(Title);

                var path = Server.MapPath("~/Content/CampaignCover/" + encodedTitle + "/");
                String imgPath = null;
                if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                {
                    Directory.CreateDirectory(path); // Klasörü oluştur
                }

                if (Img != null)
                {

                    if (Img.ContentLength > 0)
                    {

                        var filePath = Path.Combine(path, fileName);
                        Img.SaveAs(filePath);
                    }

                    imgPath = "/Content/CampaignCover/" + encodedTitle + "/" + fileName;

                }


                // Resmi bir klasöre kaydetme

                campaigns campaign = new campaigns() { campaign_cover = imgPath, campaign_title = Title, campaign_start_date = DateTime.Parse(StartDate), campaign_end_date = DateTime.Parse(EndDate), is_active = true };

                if (!TryValidateModel(campaign))
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                                                  .ToList();

                    return Json(new { success = false, message = errors });
                }

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

            var expiredCampaigns = db.campaigns
            .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
            .ToList();

            expiredCampaigns.ForEach(c => c.is_active = false);

            db.SaveChanges();


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
        [ValidateAntiForgeryToken]
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

                return Json(new { success = true, message = "Kampanya Başarıyla Silindi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Kampanya Bulunamadı" }, JsonRequestBehavior.AllowGet);


            }




        }



        public ActionResult EditCampaign(int? Id)
        {

            var expiredCampaigns = db.campaigns
          .Where(c => c.campaign_end_date <= DateTime.Now && c.is_active == true)
          .ToList();

            expiredCampaigns.ForEach(c => c.is_active = false);

            db.SaveChanges();


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
            model.products = db.products.Where(x => x.status == true).ToList();
            model.users = db.users.ToList();
            model.coupons = db.coupons.ToList();
            model.campaigns = db.campaigns.Where(x => x.id == Id).Include(x => x.campaign_products).ToList();
            model.categories = db.categories.ToList();
            model.sub_categories = db.sub_categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCampaign(HttpPostedFileBase Img, int Id, string Title, string StartDate, string EndDate, string Products, string Status)
        {

            if (db.campaigns.Any(x => x.id == Id))
            {

                if (Title != "")
                {
                    campaigns tempcampaign = new campaigns();
                    tempcampaign.campaign_title = Title;

                    tempcampaign.campaign_start_date = DateTime.Parse(StartDate);
                    tempcampaign.campaign_end_date = DateTime.Parse(EndDate);

                    if (Img != null)
                    {
                        tempcampaign.campaign_cover = "cover var";
                        if (!TryValidateModel(tempcampaign))
                        {
                            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                                          .Select(e => e.ErrorMessage)
                                                          .ToList();

                            return Json(new { success = false, message = errors });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Resim eklemeniz gerekmektedir." });

                    }


                    if (Img != null)
                    {



                        campaigns campaign = db.campaigns.Find(Id);



                        campaign.campaign_start_date = DateTime.Parse(StartDate);
                        campaign.campaign_end_date = DateTime.Parse(EndDate);



                        var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                        var extension = Path.GetExtension(Img.FileName);

                        // Dosya adını encode et
                        var fileName = EncodeFileName(originalFileName) + extension;
                        var encodedTitle = EncodeFileName(Title);

                        var path = Server.MapPath("~/Content/CampaignCover/" + encodedTitle + "/");
                        String imgPath = null;
                        if (!Directory.Exists(path)) // Klasör daha önce oluşturulmamışsa
                        {
                            Directory.CreateDirectory(path); // Klasörü oluştur
                            if (Title != campaign.campaign_title)
                            {
                                var oldpath = Server.MapPath("~/Content/CampaignCover/" + EncodeFileName(campaign.campaign_title) + "/");

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

                                var filePath = Path.Combine(path, fileName);
                                Img.SaveAs(filePath);
                            }

                            imgPath = "/Content/CampaignCover/" + encodedTitle + "/" + fileName;

                        }




                        campaign.campaign_cover = imgPath;

                        //Default olarak Pasif

                        campaign.is_active = Status == "Aktif" ? true : false;

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

        [HttpPost]
        public async Task<JsonResult> Refund(String PaymentId)
        {

            Options options = new Options();
            options.ApiKey = "sandbox-lfDKd5dEcP9SvjEbRdOaMGX5LOYVcYgO";
            options.SecretKey = "G4GKghvkujw7YYchDECfiW6MzhfTLhsq";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";


            var paymentrequest = new RetrievePaymentRequest
            {
                PaymentId = PaymentId
            };

            var payment = await Payment.Retrieve(paymentrequest, options);


            if (payment != null)
            {
                CreateAmountBasedRefundRequest request = new CreateAmountBasedRefundRequest();
                request.ConversationId = payment.ConversationId;
                request.Locale = Locale.TR.ToString();
                request.Price = payment.PaidPrice;
                request.PaymentId = payment.PaymentId;

                string hostName = Dns.GetHostName();

                string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
                request.Ip = myIP;

                Iyzipay.Model.Refund refund = await Iyzipay.Model.Refund.CreateAmountBasedRefundRequest(request, options);


                if (refund.Status == "success")
                {
                    //iade başarılı

                    var p = db.payment_details.FirstOrDefault(x => x.paymentId == PaymentId);
                    p.status = "İade";


                    foreach (var item in db.order_item.Where(x => x.order_id == p.order_id))
                    {
                        item.status = "iade";
                        item.products.products_skus.FirstOrDefault().quantity += item.quantity;

                    }

                    db.SaveChanges();

                    var list = db.order_item.Where(x => x.order_id == p.order_id).ToList()
                      .Select(x => new
                      {
                          x.id,
                          x.order_id,
                          x.products.name,
                          x.quantity,
                          x.products.cover,
                          paymentstatus = db.payment_details.Where(y => y.id == x.order_details.payment_id).First().status,
                          userName = db.users.Where(y => y.id == x.order_details.user_id).First().username,
                          created_at = x.created_at != null ? x.created_at.Value.ToString("dd/MM/yyyy HH:mm:ss") : "N/A",


                      })
                      .ToList();




                    return Json(new { success = true, message = "İade işlemi başarıyla tamamlandı.", list = list });

                }
                else
                {
                    if (refund.ErrorMessage == "Ödeme önceden iptal edilmiştir" || refund.ErrorMessage== "Ödeme önceden iade edilmiştir")
                    {
                        var p = db.payment_details.FirstOrDefault(x => x.paymentId == PaymentId);
                        p.status = "İade";


                        foreach (var item in db.order_item.Where(x => x.order_id == p.order_id))
                        {
                            item.status = "iade";
                            item.products.products_skus.FirstOrDefault().quantity += item.quantity;

                        }

                        db.SaveChanges();

                    }

                    //iade başarısız
                    return Json(new { success = false, message = refund.ErrorMessage });

                }

            }
            return Json(new { success = false, message = payment.ErrorMessage });



        }


        public JsonResult GetNotification()
        {

            var user = Session["User"] as users;
            if (user != null)
            {

                var list = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
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
                        c.users.avatar
                    },

                });
                return Json(new { success = true, list = list });

            }
            else
            {
                return Json(new { success = false, message = "Kullanıcı Bulunamadı" });

            }



        }
        [HttpPost]
        public JsonResult RemoveNotification(int id)
        {

            var user = Session["User"] as users;
            if (user != null)
            {

                var n = db.notification.Find(id);

                if (n != null)
                {
                    n.status = false;
                    db.SaveChanges();
                    var list = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
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
                            c.users.avatar
                        }
                    });
                    return Json(new { success = true, list = list });
                }
                else
                {

                    return Json(new { success = false });

                }



            }
            else
            {
                return Json(new { success = false, message = "Kullanıcı Bulunamadı" });

            }



        }

        public JsonResult RemoveAllNotification()
        {

            var user = Session["User"] as users;
            if (user != null)
            {

                var n = db.notification.Where(x => x.status == true);

                if (n != null)
                {
                    foreach (var item in n)
                    {
                        item.status = false;
                    }
                    db.SaveChanges();
                    var list = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
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
                    return Json(new { success = true, list = list });
                }
                else
                {

                    return Json(new { success = false });

                }



            }
            else
            {
                return Json(new { success = false, message = "Kullanıcı Bulunamadı" });

            }



        }

        [HttpPost]
        public JsonResult ReadNotification(int id)
        {

            var user = Session["User"] as users;
            if (user != null)
            {

                var n = db.notification.FirstOrDefault(x => x.id == id && x.status == true);

                if (n != null)
                {

                    n.is_read = true;

                    db.SaveChanges();
                    var list = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
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
                    return Json(new { success = true, list = list });
                }
                else
                {

                    return Json(new { success = false });

                }



            }
            else
            {
                return Json(new { success = false, message = "Kullanıcı Bulunamadı" });

            }



        }


        public JsonResult ReadAllNotification()
        {

            var user = Session["User"] as users;
            if (user != null)
            {

                var n = db.notification.Where(x => x.status == true && x.is_read == false);

                if (n != null)
                {
                    foreach (var item in n)
                    {
                        item.is_read = true;
                    }
                    db.SaveChanges();
                    var list = db.notification.Where(x => x.status == true).OrderByDescending(o => o.id).Select(c => new
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
                    return Json(new { success = true, list = list });
                }
                else
                {

                    return Json(new { success = false });

                }



            }
            else
            {
                return Json(new { success = false, message = "Kullanıcı Bulunamadı" });

            }



        }
        [HttpPost]
        public string isDelivered(int id)
        {

            if (db.order_details.Any(x => x.id == id))
            {
                var isDelivered = db.order_details.FirstOrDefault(x => x.id == id).is_delivered.ToString();

                return isDelivered;
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult ChangeDeliveryStatus(int Id)
        {
            if (!db.order_details.Any(x => x.id == Id))
            {
                return Json(new { success = false, message = "Sipariş Bulunamadı" });

            }

            var order = db.order_details.Find(Id);
            order.is_delivered = order.is_delivered == true ? false : true;
            db.SaveChanges();

            return Json(new { success = true, message = "Sipariş Teslimat Durumu Değiştirildi", status = order.is_delivered });

        }


        public ActionResult Shipping()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();
            model.company_details = db.company_details.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }


        public ActionResult PageManagement()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();
            model.company_details = db.company_details.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditShipping(String MinAmount, String ShippingCost)
        {
            var cd = db.company_details.FirstOrDefault();

            if (cd == null)
            {
                return Json(new { success = false, message = "Bulunamadı" });

            }

            decimal? minamount = null;
            decimal? shippingcost = null;

            if (decimal.TryParse(MinAmount.Replace(".", ","), out decimal parsedAmount))
            {
                minamount = parsedAmount;
            }
            else  // Boş bırakıldıysa veya geçersiz değerse varsayılan değer
            {
                minamount = 300;
            }

            if (decimal.TryParse(ShippingCost.Replace(".", ","), out decimal parsedShipping))
            {
                shippingcost = parsedShipping;
            }
            else  // Boş bırakıldıysa veya geçersiz değerse varsayılan değer
            {
                shippingcost = 30;
            }

            cd.min_amonunt_for_free_shipping = minamount;
            cd.shipping_cost = shippingcost;

            db.SaveChanges();

            return Json(new { success = true, message = "Başarıyla Güncellendi" });

        }

        [HttpPost]
        public JsonResult EditTop(String content)
        {
            try
            {
                db.company_details.FirstOrDefault().homepage_top_text = content.Trim();
                db.SaveChanges();

            }
            catch (DbEntityValidationException e)
            {

                return Json(new { success = false, message = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }


        [HttpPost]
        public JsonResult EditSliderTitle(String content)
        {
            try
            {
                db.company_details.FirstOrDefault().homepage_slider_title = content;
                db.SaveChanges();

            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



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
        public JsonResult EditSliderImg(HttpPostedFileBase Img)
        {
            try
            {
                if (Img != null)
                {
                    string imgpath = null;

                    var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                    var extension = Path.GetExtension(Img.FileName);

                    // Dosya adını encode et
                    var fileName = EncodeFileName(originalFileName) + extension;


                    var path = Path.Combine(Server.MapPath("~/Content/PageContentImg/SliderImg/"));
                    imgpath = "/Content/PageContentImg/SliderImg/" + fileName;

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
                    db.company_details.FirstOrDefault().homepage_slider_img = imgpath;
                    db.SaveChanges();

                }


            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }



        [HttpPost]
        public JsonResult EditSliderText(String content)
        {
            try
            {
                db.company_details.FirstOrDefault().homepage_slider_text = content;
                db.SaveChanges();

            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }

        [HttpPost]
        public JsonResult EditBottomHeader(String content)
        {
            try
            {
                db.company_details.FirstOrDefault().homepage_bottom_header = content;
                db.SaveChanges();

            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }

        [HttpPost]
        public JsonResult EditBottomText(String content)
        {
            try
            {
                db.company_details.FirstOrDefault().homepage_bottom_text = content;
                db.SaveChanges();

            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }


        [HttpPost]
        public JsonResult EditBottomImg(HttpPostedFileBase Img)
        {
            try
            {
                if (Img != null)
                {
                    string imgpath = null;

                    var originalFileName = Path.GetFileNameWithoutExtension(Img.FileName);
                    var extension = Path.GetExtension(Img.FileName);

                    // Dosya adını encode et
                    var fileName = EncodeFileName(originalFileName) + extension;



                    var path = Path.Combine(Server.MapPath("~/Content/PageContentImg/BottomImg/"));
                    imgpath = "/Content/PageContentImg/BottomImg/" + fileName;

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
                    db.company_details.FirstOrDefault().homepage_bottom_img = imgpath;
                    db.SaveChanges();

                }


            }
            catch (Exception)
            {

                return Json(new { success = false, message = "Bir hata oluştu" });

            }

            return Json(new { success = true, message = "Başarıyla Güncellendi" });



        }




        public ActionResult SeoSettings()
        {
            var allSeoSettings = LoadAllSeoSettings();




            AllViewModel model = new AllViewModel();
            model.SeoSettings = allSeoSettings;

            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();
            model.company_details = db.company_details.ToList();
            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            return View(model);
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


        public JsonResult LoadSeoSettings(string page)
        {
            var seoData = LoadAllSeoSettings();
            seoData.TryGetValue(page, out var seoSettings);

            var result = new
            {
                Page = page,
                SeoSettings = seoSettings ?? new SeoModel()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveSeoSettings(string Page, string Title, string Keywords, string Description, bool NoIndex, bool NoFollow)
        {

            SeoModel model = new SeoModel()
            {
                Page = Page,
                Title = Title,
                Keywords = Keywords,
                Description = Description,
                NoIndex = NoIndex,
                NoFollow = NoFollow
            };
            if (ModelState.IsValid)
            {
                var seoData = LoadAllSeoSettings();


                if (seoData.ContainsKey(model.Page))
                {
                    // Mevcut sayfa için güncelleme
                    seoData[model.Page] = model;
                }
                else
                {
                    // Yeni SEO kaydı ekleme
                    seoData.Add(model.Page, model);
                }

                SaveSeoSettingsToFile(seoData);
                return Json(new { success = true, message = "SEO ayarları başarıyla kaydedildi!" });
            }

            return Json(new { success = false, message = "Ayarlar kaydedilemedi!" });
        }


        // SEO ayarlarını dosyaya kaydeder veya günceller
        private void SaveSeoSettingsToFile(Dictionary<string, SeoModel> seoData)
        {
            string path = Server.MapPath("~/Content/seoSettings.json");
            var jsonData = JsonConvert.SerializeObject(seoData, Formatting.Indented);
            System.IO.File.WriteAllText(path, jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteSeoSettings(string Page)
        {
            if (string.IsNullOrEmpty(Page))
            {
                return Json(new { success = false, message = "Sayfa adı boş olamaz." });
            }

            var seoData = LoadAllSeoSettings();
            if (seoData.ContainsKey(Page))
            {
                seoData.Remove(Page);
                SaveSeoSettingsToFile(seoData);
                return Json(new { success = true, message = "SEO ayarları başarıyla silindi!" });
            }
            return Json(new { success = false, message = "SEO ayarları bulunamadı!" });
        }
        public ActionResult GetSeoSettings()
        {
            // JSON dosyasının yolu
            string path = Server.MapPath("~/Content/seoSettings.json");


            var json = System.IO.File.ReadAllText(path);
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EBulten()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();

            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.newsletters = db.newsletter.ToList();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveMail(int Id)
        {

            if (db.newsletter.Any(x => x.id == Id))
            {

                newsletter mail = db.newsletter.FirstOrDefault(x => x.id == Id);


                db.newsletter.Remove(mail);
                db.SaveChanges();

                return Json(new { success = true, message = "Mail Başarıyla Silindi" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = false, message = "Böyle Bir Mail Bulunamadı" }, JsonRequestBehavior.AllowGet);


            }




        }

        public ActionResult TopluMesaj()
        {


            AllViewModel model = new AllViewModel();
            model.products = db.products.Include(x => x.products_skus).ToList();
            model.products_skus = db.products_skus.ToList();
            model.users = db.users.ToList();
            model.order_details = db.order_details.Include(o => o.order_item).ToList();
            model.order_item = db.order_item.Include(o => o.products_skus).Include(o => o.products).ToList();
            model.payment_details = db.payment_details.ToList();

            model.categories = db.categories.ToList();
            model.carts = db.cart.ToList();
            model.newsletters = db.newsletter.ToList();
            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public JsonResult SendBulkMail(string subject, string content)
        {
            var mailList = db.newsletter.Where(x => x.status == true);


            if (subject.IsNullOrWhiteSpace())
            {
                return Json(new { success = false, message = "Konu boş geçilemez." }, JsonRequestBehavior.AllowGet);

            }

            if (mailList.Any())
            {
                int count = 0;
                foreach (var mail in mailList)
                {
                    try
                    {
                        // HtmlAgilityPack ile HTML içeriği yükleme
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(content);

                        // Tüm resimleri al
                        var images = doc.DocumentNode.SelectNodes("//img");

                        // MimeMessage oluşturma
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Çobanlar Market", "info@cobanlarmarket.com"));
                        message.To.Add(new MailboxAddress("", mail.email)); // Alıcı adresi
                        message.Subject = subject;

                        var bodyBuilder = new BodyBuilder { HtmlBody = doc.DocumentNode.OuterHtml };

                        // Resimleri inline olarak ekleyelim
                        if (images != null)
                        {
                            int imageIndex = 0;
                            foreach (var img in images)
                            {
                                var src = img.GetAttributeValue("src", string.Empty);
                                if (src.StartsWith("data:image"))
                                {
                                    // Base64 verisini çözme
                                    var base64Data = src.Substring(src.IndexOf(',') + 1);
                                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                                    // Resmi inline olarak ekleme
                                    var imagePart = new MimePart("image", "jpeg")
                                    {
                                        Content = new MimeContent(new MemoryStream(imageBytes)),
                                        ContentId = $"image{imageIndex++}",
                                        ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                                        ContentTransferEncoding = ContentEncoding.Base64
                                    };

                                    // Inline resmin cid'sini img tag'ine ekleme
                                    img.SetAttributeValue("src", $"cid:image{imageIndex - 1}");

                                    // Resmi e-postaya ekleme
                                    bodyBuilder.Attachments.Add(imagePart);
                                }
                            }
                        }

                        // Gövdeyi e-postaya ekleyelim
                        message.Body = bodyBuilder.ToMessageBody();
                   

                        // SMTP ile e-postayı gönderme
                        using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                        {
                            smtp.Connect("srvm15.trwww.com", 465, SecureSocketOptions.SslOnConnect); // SMTP sunucu bilgilerini kullanın
                            smtp.Authenticate("info@cobanlarmarket.com", "Sananelan6151."); // SMTP kullanıcı adı ve şifresi
                            smtp.Send(message); // E-postayı gönder
                            smtp.Disconnect(true);
                        }

                        count++;
                    }
                    catch (Exception e)
                    {
                        // Hata loglama
                        return Json(new { success = false, message = "Bir hata oluştu: " + e.Message }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { success = true, message = count + " kişiye mail yollandı." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Bültene kayıtlı üye bulunamadı." }, JsonRequestBehavior.AllowGet);
        }


    }
}