using Microsoft.AspNetCore.Mvc;
using SinemaOtomasyonu.Models;
using System.Text.Json;

namespace SinemaOtomasyonu.Controllers
{
    public class BufeController : Controller
    {
        private readonly SinemaContext _context;

        public BufeController(SinemaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var urunler = _context.BufeUrunler.Where(u => u.StokAdeti > 0).ToList();
            return View(urunler);
        }

        [HttpPost]
        public IActionResult SepeteEkle(int urunId, int adet)
        {
            var urun = _context.BufeUrunler.Find(urunId);
            if (urun == null) return RedirectToAction("Index");

            var sepetJson = HttpContext.Session.GetString("Sepet");
            SepetOzetModel model = sepetJson == null ? new SepetOzetModel() : JsonSerializer.Deserialize<SepetOzetModel>(sepetJson);

            // Varsa güncelle, yoksa ekle
            var mevcut = model.BufeUrunleri.FirstOrDefault(b => b.UrunId == urunId);
            if (mevcut != null)
            {
                mevcut.Adet += adet;
            }
            else
            {
                model.BufeUrunleri.Add(new SepetElemani
                {
                    UrunId = urun.UrunId,
                    UrunAdi = urun.UrunAdi,
                    Fiyat = urun.Fiyat,
                    Adet = adet
                });
            }

            HttpContext.Session.SetString("Sepet", JsonSerializer.Serialize(model));
            return RedirectToAction("Index");
        }

        public IActionResult SepeteGit()
        {
            return RedirectToAction("Index", "Sepet");
        }
    }
}
