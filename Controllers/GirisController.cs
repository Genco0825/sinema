using Microsoft.AspNetCore.Mvc;
using SinemaOtomasyonu.Models;

namespace SinemaOtomasyonu.Controllers
{
    public class GirisController : Controller
    {
        private readonly SinemaContext _context;

        public GirisController(SinemaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GirisYap(string email, string sifre)
        {
            var kullanici = _context.Kullanicilar
                .FirstOrDefault(k => k.Email == email && k.Sifre == sifre);

            if (kullanici != null)
            {
                // Session'a kullanıcı bilgilerini kaydet
                HttpContext.Session.SetInt32("KullaniciId", kullanici.KullaniciId);
                HttpContext.Session.SetString("AdSoyad", kullanici.AdSoyad);
                HttpContext.Session.SetString("Rol", kullanici.Rol);

                if (kullanici.Rol == "Admin")
                {
                    return RedirectToAction("Index", "Yonetim");
                }
                else
                {
                    return RedirectToAction("Index", "Bilet");
                }
            }

            ViewBag.Hata = "Hatalı E-posta veya Şifre!";
            return View("Index");
        }

        public IActionResult CikisYap()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
