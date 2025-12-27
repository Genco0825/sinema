using Microsoft.AspNetCore.Mvc;
using SinemaOtomasyonu.Models;
using Microsoft.EntityFrameworkCore;

namespace SinemaOtomasyonu.Controllers
{
    public class YonetimController : Controller
    {
        private readonly SinemaContext _context;

        public YonetimController(SinemaContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Rol") == "Admin";
        }

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Giris");

            // Sayfaya hem filmleri hem de büfe ürünlerini gönderiyoruz
            ViewBag.BufeUrunleri = _context.BufeUrunler.ToList();
            var filmler = _context.Filmler.ToList();
            
            return View(filmler);
        }

        public IActionResult FilmEkle()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Giris");
            return View();
        }

        [HttpPost]
        public IActionResult FilmEkle(string filmAdi, string afisYolu, int sureDakika, decimal fiyat)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Giris");

            var yeniFilm = new Film
            {
                FilmAdi = filmAdi,
                AfisYolu = afisYolu,
                SureDakika = sureDakika,
                Fiyat = fiyat,
                Aciklama = "Vizyondaki Film"
            };

            _context.Filmler.Add(yeniFilm);
            _context.SaveChanges();

            // Otomatik Seans Oluşturma
            DateTime bugun = DateTime.Today;
            int[] saatler = { 10, 14, 18 };
            
            foreach (var saat in saatler)
            {
                _context.Seanslar.Add(new Seans
                {
                    FilmId = yeniFilm.FilmId,
                    SalonId = 1,
                    BaslangicSaati = bugun.AddHours(saat)
                });
            }
            _context.SaveChanges();
            
            TempData["Mesaj"] = "Film başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        // MEVCUT STOK GÜNCELLEME
        [HttpPost]
        public IActionResult StokGuncelle(int urunId, int miktar)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Giris");
            var urun = _context.BufeUrunler.Find(urunId);
            if (urun != null)
            {
                urun.StokAdeti += miktar;
                if (urun.StokAdeti < 0) urun.StokAdeti = 0;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // YENİ EKLENEN: SIFIRDAN ÜRÜN EKLEME METODU
        [HttpPost]
        public IActionResult BufeUrunEkle(string urunAdi, decimal fiyat, int baslangicStok)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Giris");

            var yeniUrun = new BufeUrun
            {
                UrunAdi = urunAdi,
                Fiyat = fiyat,
                StokAdeti = baslangicStok
            };

            _context.BufeUrunler.Add(yeniUrun);
            _context.SaveChanges();

            TempData["Mesaj"] = $"{urunAdi} büfeye eklendi!";
            return RedirectToAction("Index");
        }
    }
}