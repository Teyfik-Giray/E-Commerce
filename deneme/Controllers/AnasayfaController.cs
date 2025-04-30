using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace deneme.Controllers
{
    public class AnasayfaController : Controller
    {
        eticaretEntities1 db = new eticaretEntities1();
        // GET: Anasayfa
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Shop()
        {
            //var urunler = db.urunler.ToList(); 
            //ViewBag.Kategoriler = db.kategoriler.ToList(); 
            //return View(urunler);
            var urunler = db.urunler.ToList(); 
            var stoklar = db.stoklar.ToList();
            var kategoriler = db.kategoriler.ToList();


            // MyViewModel örneği oluştur
            var viewModel = new ProductManagementViewModel()
            {
                Stoklar = stoklar,
                Products = urunler,
                Categories = kategoriler

            };
            return View(viewModel);

        }
        public ActionResult Cart() 
        {
            return View();
        }
        public ActionResult Checkout() 
        {
            return View();
        }

        public ActionResult MesafeliSatis()
        {
            return View();
        }
        public ActionResult GizlilikPolitikasi()
        {
            return View();
        }
        public ActionResult İadeDegisim()
        {
            return View();
        }
        public ActionResult CerezPolitikasi()
        {
            return View();
        }
        public ActionResult AydınlatmaMetni()
        {
            return View();
        }
        public ActionResult Talep()
        {
            return View();
        }
        public ActionResult UrunDetay()
        {
            return View();
        }
        [HttpPost]
        public JsonResult TalepKaydet(TalepViewModel model)
        {
            try
            {
                talepler yeniTalep = new talepler
                {
                    ad = model.ad,
                    soyad = model.soyad,
                    eposta = model.eposta,
                    tel = model.tel,
                    sehir = model.sehir,
                    urun = model.urun,
                    aciklama = model.aciklama,
                    gun = model.gun
                };

                db.talepler.Add(yeniTalep);
                db.SaveChanges();

                return Json(new { success = true, message = "Talebiniz başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}