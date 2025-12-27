using Microsoft.AspNetCore.Mvc;
using SinemaOtomasyonu.Models;
using System.Text.Json;

namespace SinemaOtomasyonu.Controllers
{
    public class SepetController : Controller
    {
        private readonly SinemaContext _context;

        public SepetController(SinemaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var sepetJson = HttpContext.Session.GetString("Sepet");
            if (sepetJson == null)
            {
                ViewBag.Hata = "Sepetiniz boş.";
                return View(new SepetOzetModel());
            }

            SepetOzetModel model = JsonSerializer.Deserialize<SepetOzetModel>(sepetJson);
            
            // Seans ve Film bilgisini tazele (Session'da sadece ID tutmak daha güvenli olabilir ama biz obje tutmuştuk, ID ile check edelim)
            if (model.SecilenSeansId != null)
            {
                var seans = _context.Seanslar.Find(model.SecilenSeansId);
                if (seans != null) 
                {
                   var film = _context.Filmler.Find(seans.FilmId);
                   model.SecilenFilm = film;
                }
            }

            return View(model);
        }

        public IActionResult SepetiBosalt()
        {
            HttpContext.Session.Remove("Sepet");
            return RedirectToAction("Index");
        }
    }
}
