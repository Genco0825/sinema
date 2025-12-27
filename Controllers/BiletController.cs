using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinemaOtomasyonu.Models;
using System.Text.Json;

namespace SinemaOtomasyonu.Controllers
{
    public class BiletController : Controller
    {
        private readonly SinemaContext _context;

        public BiletController(SinemaContext context)
        {
            _context = context;
        }

        // ADIM 1: Bilet Tipi Seçimi
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BiletTipiSec(string biletTipi)
        {
            var sepet = HttpContext.Session.GetString("Sepet");
            SepetOzetModel model = sepet == null ? new SepetOzetModel() : JsonSerializer.Deserialize<SepetOzetModel>(sepet);
            
            model.BiletTipi = biletTipi;
            
            HttpContext.Session.SetString("Sepet", JsonSerializer.Serialize(model));
            return RedirectToAction("SeansSecimi");
        }

        // ADIM 2: Film ve Seans Seçimi
        public IActionResult SeansSecimi()
        {
            var seanslar = _context.Seanslar
                .Include(s => s.Film)
                .Include(s => s.Salon)
                .OrderBy(s => s.BaslangicSaati)
                .ToList();
            return View(seanslar);
        }

        [HttpPost]
        public IActionResult SeansSec(int seansId)
        {
            var sepet = HttpContext.Session.GetString("Sepet");
            if (sepet == null) return RedirectToAction("Index");
            
            SepetOzetModel model = JsonSerializer.Deserialize<SepetOzetModel>(sepet);
            model.SecilenSeansId = seansId;

            // Seans Detaylarını da alıp modele ekleyelim (Opsiyonel ama iyi olur)
            var seans = _context.Seanslar.Include(s => s.Film).FirstOrDefault(s => s.SeansId == seansId);
            if(seans != null) model.SecilenFilm = seans.Film;

            HttpContext.Session.SetString("Sepet", JsonSerializer.Serialize(model));
            return RedirectToAction("KoltukSecimi");
        }

        // ADIM 3: Koltuk Seçimi
        public IActionResult KoltukSecimi()
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (sepetJson == null) return RedirectToAction("Index");
            
            SepetOzetModel model = JsonSerializer.Deserialize<SepetOzetModel>(sepetJson);
            if (model.SecilenSeansId == null) return RedirectToAction("SeansSecimi");

            int seansId = model.SecilenSeansId.Value;

            // SP Çağır: sp_DoluKoltuklariGetir
            // Raw SQL ile int listesi almak
            var doluKoltuklar = _context.Database
                .SqlQueryRaw<int>("EXEC sp_DoluKoltuklariGetir @SeansId = {0}", seansId)
                .ToList();

            ViewBag.DoluKoltuklar = doluKoltuklar;
            
            // Salon kapasitesini de öğrenelim
            var seans = _context.Seanslar.Include(s => s.Salon).FirstOrDefault(s => s.SeansId == seansId);
            ViewBag.Kapasite = seans?.Salon?.Kapasite ?? 50;

            return View();
        }

        [HttpPost]
        public IActionResult KoltukSec(int koltukNo)
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (sepetJson == null) return RedirectToAction("Index");
            
            SepetOzetModel model = JsonSerializer.Deserialize<SepetOzetModel>(sepetJson);
            model.SecilenKoltukNo = koltukNo;

            HttpContext.Session.SetString("Sepet", JsonSerializer.Serialize(model));
            return RedirectToAction("Index", "Bufe");
        }
    }
}
