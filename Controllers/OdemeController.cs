using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinemaOtomasyonu.Models;
using System.Text.Json;

namespace SinemaOtomasyonu.Controllers
{
    public class OdemeController : Controller
    {
        private readonly SinemaContext _context;

        public OdemeController(SinemaContext context)
        {
            _context = context;
        }

        // 1. ADIM: Ödeme Formunu Göster
        [HttpGet]
        public IActionResult Index()
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (string.IsNullOrEmpty(sepetJson)) return RedirectToAction("Index", "Anasayfa");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var sepet = JsonSerializer.Deserialize<SepetOzetModel>(sepetJson, options);

            ViewBag.ToplamTutar = sepet.ToplamTutar;
            return View(); // Views/Odeme/Index.cshtml sayfasını açar
        }

        // 2. ADIM: "Öde" Butonuna Basılınca İşlemi Yap
        [HttpPost]
        public IActionResult OdemeYap(string kartNo, string sktAy, string sktYil, string cvv)
        {
            // Basit Validasyon
            if (string.IsNullOrEmpty(kartNo) || kartNo.Length < 16)
            {
                TempData["Hata"] = "Geçersiz kart numarası.";
                return RedirectToAction("Index");
            }

            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (sepetJson == null) return RedirectToAction("Index", "Giris");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            SepetOzetModel model = JsonSerializer.Deserialize<SepetOzetModel>(sepetJson, options);

            int? kullaniciId = HttpContext.Session.GetInt32("KullaniciId");
            // Eğer giriş yapmadıysa (Demo amaçlı) ID 2'yi kullan
            int gercekKullaniciId = kullaniciId ?? 2; 

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // A) BİLET KAYDI
                    if (model.SecilenSeansId != null && model.SecilenKoltukNo != null)
                    {
                        var seans = _context.Seanslar.Include(s => s.Film).FirstOrDefault(s => s.SeansId == model.SecilenSeansId);
                        decimal biletFiyat = seans.Film.Fiyat;

                        // Öğrenci İndirimi
                        if (model.BiletTipi == "Ogrenci")
                        {
                            // SQL Fonksiyonu veya direkt hesaplama
                             var indirimliFiyat = _context.Database
                                .SqlQueryRaw<decimal>("SELECT dbo.fn_OgrenciIndirimHesapla({0})", biletFiyat)
                                .AsEnumerable().FirstOrDefault();
                             biletFiyat = indirimliFiyat;
                        }

                        var bilet = new Bilet
                        {
                            SeansId = model.SecilenSeansId.Value,
                            KullaniciId = gercekKullaniciId,
                            KoltukNo = model.SecilenKoltukNo.Value,
                            Fiyat = biletFiyat,
                            SatinAlmaTarihi = DateTime.Now
                        };

                        _context.Biletler.Add(bilet);
                        _context.SaveChanges(); // Trigger burada çalışır
                    }

                    // B) BÜFE SATIŞI (SP Kullanarak)
                    foreach (var urun in model.BufeUrunleri)
                    {
                        _context.Database.ExecuteSqlRaw("EXEC sp_SatisYap @KullaniciId = {0}, @UrunId = {1}, @Adet = {2}", 
                            gercekKullaniciId, urun.UrunId, urun.Adet);
                    }

                    transaction.Commit();
                    
                    // Başarılı olunca sepeti sil
                    HttpContext.Session.Remove("Sepet");

                    return RedirectToAction("Basarili");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    
                    // Trigger Hatası Yakalama
                    string hata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    
                    if (hata.Contains("dolu") || hata.Contains("taken"))
                    {
                        TempData["Hata"] = "Üzgünüz! Seçtiğiniz koltuk ödeme sırasında başkası tarafından alındı.";
                        return RedirectToAction("Index", "Bilet"); // Koltuk seçimine geri gönder
                    }
                    else
                    {
                        TempData["Hata"] = "Ödeme başarısız: " + hata;
                        return RedirectToAction("Index"); // Ödeme ekranına geri dön
                    }
                }
            }
        }

        // 3. ADIM: Başarı Sayfası
        public IActionResult Basarili()
        {
            return View();
        }
    }
}