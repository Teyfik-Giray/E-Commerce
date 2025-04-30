using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;


namespace deneme.Controllers
{
    public class AdminController : Controller
    {
        eticaretEntities1 db = new eticaretEntities1();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login() 
        {
            return View();
        }
        
        public ActionResult ProductManagement()
        {

            var uniqueCategories = db.kategoriler
                                  .Where(k => db.urunler.Any(u => u.kategori_id == k.id))
                                  .Distinct()
                                  .ToList();

            var products = db.urunler.Include("kategoriler").ToList();

            var viewModel = new ProductManagementViewModel
            {
                Products = products,
                Categories = uniqueCategories
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult AddProduct(HttpPostedFileBase productImage, string productName, decimal productPrice, string productDescription, int categoryId, int stockQuantity)
        {
            if (productImage != null && productImage.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate a unique file name
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Resize image to 800x800 pixels
                ResizeImage(productImage.InputStream, filePath, 800, 800);

                // Save the resized image path to the database
                urunler newProduct = new urunler()
                {
                    urun_gorseli = "/Uploads/" + fileName,
                    urun_adi = productName,
                    urun_fiyatı = (double?)productPrice,
                    urun_aciklama = productDescription,
                    kategori_id = categoryId
                };

                db.urunler.Add(newProduct);
                db.SaveChanges();
                // Stok kaydı ekle
                stoklar stock = new stoklar()
                {
                    urun_id = newProduct.id,
                    stok_miktari = stockQuantity
                };

                db.stoklar.Add(stock);
                db.SaveChanges();
            }

            return RedirectToAction("ProductManagement");
        }

        // Method to resize image
        private void ResizeImage(Stream inputStream, string outputPath, int newWidth, int newHeight)
        {
            using (var image = Image.FromStream(inputStream))
            {
                var thumbnail = new Bitmap(newWidth, newHeight);
                using (var graphic = Graphics.FromImage(thumbnail))
                {
                    graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    graphic.DrawImage(image, 0, 0, newWidth, newHeight);
                }
                thumbnail.Save(outputPath, ImageFormat.Jpeg);
            }
        }
        public ActionResult AddProduct()
        {
            ViewBag.Categories = db.urunler
                .Where(u => u.kategoriler != null) // Null kategorileri filtrele
                .GroupBy(u => u.kategoriler.id) // Kategori ID’ye göre gruplandır
                .Select(g => g.FirstOrDefault().kategoriler.id) // Her gruptan sadece bir tanesini al
                .ToList();

            return View();
        }

        // GET: Admin/EditProduct/{id}
        public ActionResult EditProduct(int id)
        {
            var product = db.urunler.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Admin/EditProduct/{id}
        
        [HttpPost]
        public ActionResult EditProduct(int id, string productName, decimal productPrice, int stockQuantity, HttpPostedFileBase productImage)
        {
            var product = db.urunler.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            product.urun_adi = productName;
            product.urun_fiyatı = (double?)productPrice;

            if (productImage != null && productImage.ContentLength > 0)
            {
                string imagePath = "/Uploads/" + productImage.FileName;
                string serverPath = Server.MapPath(imagePath);
                productImage.SaveAs(serverPath);
                product.urun_gorseli = imagePath;
            }

            // stoklar tablosunda ürünün stoğunu güncelle
            var stok = db.stoklar.FirstOrDefault(s => s.urun_id == id);
            if (stok != null)
            {
                stok.stok_miktari = stockQuantity;
                db.Entry(stok).State = EntityState.Modified;
            }
            else
            {
                // stok kaydı yoksa yeni ekle
                db.stoklar.Add(new stoklar
                {
                    urun_id = id,
                    stok_miktari = stockQuantity
                });
            }

            db.SaveChanges();
            return RedirectToAction("ProductManagement");
        }

        // GET: Admin/DeleteProduct/{id}
        public ActionResult DeleteProduct(int id)
        {
            var product = db.urunler.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            db.urunler.Remove(product);
            db.SaveChanges();

            return RedirectToAction("ProductManagement");
        }

       
        // GET: Kullanıcılar (Listeleme)
        public ActionResult UserManagement()
        {
            var users = db.kullanicilar.ToList();
            return View(users);
        }

        // GET: Yeni Kullanıcı Ekle
        public ActionResult AddUser()
        {
            return View();
        }

        // POST: Yeni Kullanıcı Ekle
        [HttpPost]
        public ActionResult AddUser(string adi, string soyadi, string kullanici_adi, string sifre)
        {
            if (ModelState.IsValid)
            {
                var newUser = new kullanicilar
                {
                    adi = adi,
                    soyadi = soyadi,
                    kullanici_adi = kullanici_adi,
                    sifre = sifre // Şifreyi hash'leyerek kaydetmek güvenlik açısından önemlidir!
                };

                db.kullanicilar.Add(newUser);
                db.SaveChanges();
                return RedirectToAction("UserManagement");
            }
            return View();
        }

        // GET: Kullanıcı Düzenle
        public ActionResult EditUser(int id)
        {
            var user = db.kullanicilar.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Kullanıcı Düzenle
        [HttpPost]
        public ActionResult EditUser(int id, string adi, string soyadi, string kullanici_adi, string sifre)
        {
            var user = db.kullanicilar.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            user.adi = adi;
            user.soyadi = soyadi;
            user.kullanici_adi = kullanici_adi;

            // Şifreyi kontrol et, boş ise eski şifreyi bırak
            if (!string.IsNullOrEmpty(sifre))
            {
                user.sifre = sifre; // Şifreyi burada da hash'lemek gerekir!
            }

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("UserManagement");
        }


        // GET: Kullanıcı Sil
        public ActionResult DeleteUser(int id)
        {
            var user = db.kullanicilar.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            db.kullanicilar.Remove(user);
            db.SaveChanges();
            return RedirectToAction("UserManagement");
        }
        // Kategori ekleme action
        [HttpPost]
        public ActionResult AddCategory(string categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                var category = new kategoriler { kategori_adi = categoryName };
                db.kategoriler.Add(category);
                db.SaveChanges();
            }

            return RedirectToAction("ProductManagement"); // Sayfayı yeniden yükler
        }
        public ActionResult OdemeSistemleri()
        {
            return View();
        }
        public ActionResult MusteriTalepleri()
        {
            var model = new ProductManagementViewModel
            {
                Talepler = db.talepler.OrderByDescending(t => t.talep_id).ToList()
            };

            return View(model);
        }
    }
}