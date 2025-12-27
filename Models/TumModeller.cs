using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // List<> için gerekli

namespace SinemaOtomasyonu.Models
{
    // =============================================
    // VERİTABANI SINIFLARI (ENTITIES)
    // =============================================

    [Table("Kullanicilar")]
    public class Kullanici
    {
        [Key]
        public int KullaniciId { get; set; }

        [Required]
        public string AdSoyad { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Sifre { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "Musteri"; // Admin veya Musteri
    }

    [Table("Filmler")]
    public class Film
    {
        [Key]
        public int FilmId { get; set; }

        [Required]
        public string FilmAdi { get; set; } = "";

        public string? Aciklama { get; set; }
        public string? AfisYolu { get; set; }

        public int SureDakika { get; set; }
        public decimal Fiyat { get; set; }
    }

    [Table("Salonlar")]
    public class Salon
    {
        [Key]
        public int SalonId { get; set; }

        [Required]
        public string SalonAdi { get; set; } = "";

        public int Kapasite { get; set; }
    }

    [Table("Seanslar")]
    public class Seans
    {
        [Key]
        public int SeansId { get; set; }

        public int FilmId { get; set; }
        public int SalonId { get; set; }
        public DateTime BaslangicSaati { get; set; }

        // Navigation Properties
        public virtual Film? Film { get; set; }
        public virtual Salon? Salon { get; set; }
    }

    [Table("Biletler")]
    public class Bilet
    {
        [Key]
        public int BiletId { get; set; }

        public int SeansId { get; set; }
        public int KullaniciId { get; set; }
        public int KoltukNo { get; set; }
        public decimal Fiyat { get; set; }
        public DateTime SatinAlmaTarihi { get; set; }

        public virtual Seans? Seans { get; set; }
        public virtual Kullanici? Kullanici { get; set; }
    }

    [Table("BufeUrunler")]
    public class BufeUrun
    {
        [Key]
        public int UrunId { get; set; }

        [Required]
        public string UrunAdi { get; set; } = "";

        public decimal Fiyat { get; set; }
        public int StokAdeti { get; set; }
    }

    [Table("BufeSatislar")]
    public class BufeSatis
    {
        [Key]
        public int SatisId { get; set; }

        public int KullaniciId { get; set; }
        public int UrunId { get; set; }
        public int Adet { get; set; }
        public decimal ToplamFiyat { get; set; }
        public DateTime SatisTarihi { get; set; }

        public virtual Kullanici? Kullanici { get; set; }
        public virtual BufeUrun? Urun { get; set; }
    }

    // =============================================
    // VIEW MODELS (TEK DOSYADA)
    // =============================================

    // Sepetin özetini ve tipleri tutan model
    public class SepetOzetModel
    {
        public string BiletTipi { get; set; } = "Tam"; // Tam veya Ogrenci
        public List<SepetElemani> BufeUrunleri { get; set; } = new List<SepetElemani>();
        
        // Ek özellikler sepet gösterimi için
        public int? SecilenSeansId { get; set; }
        public int? SecilenKoltukNo { get; set; }
        public Film? SecilenFilm { get; set; }
        
        // Sepet toplamını hesaplamak için yardımcı özellik
        public decimal ToplamTutar 
        { 
            get 
            {
                decimal biletFiyati = SecilenFilm != null ? SecilenFilm.Fiyat : 0;
                // Öğrenci ise %20 indirim (SQL Fonksiyonu kullanılamazsa burada basit mantık)
                if(BiletTipi == "Ogrenci") biletFiyati *= 0.8m; 

                decimal bufeToplami = 0;
                foreach(var urun in BufeUrunleri)
                {
                    bufeToplami += urun.ToplamTutar;
                }

                return biletFiyati + bufeToplami;
            } 
        }
    }

    public class SepetElemani
    {
        public int UrunId { get; set; }
        public string UrunAdi { get; set; } = "";
        public decimal Fiyat { get; set; }
        public int Adet { get; set; }

        public decimal ToplamTutar => Fiyat * Adet;
    }

    // =============================================
    // DATABASE CONTEXT (TRIGGER AYARLARI EKLENDİ)
    // =============================================
    public class SinemaContext : DbContext
    {
        public SinemaContext(DbContextOptions<SinemaContext> options) : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Film> Filmler { get; set; }
        public DbSet<Salon> Salonlar { get; set; }
        public DbSet<Seans> Seanslar { get; set; }
        public DbSet<Bilet> Biletler { get; set; }
        public DbSet<BufeUrun> BufeUrunler { get; set; }
        public DbSet<BufeSatis> BufeSatislar { get; set; }

        // İŞTE EKLENEN KRİTİK KISIM BURASI 👇
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Biletler tablosunda trigger olduğunu EF Core'a bildiriyoruz
            modelBuilder.Entity<Bilet>()
                .ToTable(tb => tb.HasTrigger("trg_AyniKoltukEngelle"));

            // BufeSatislar tablosunda trigger olduğunu bildiriyoruz
            modelBuilder.Entity<BufeSatis>()
                .ToTable(tb => tb.HasTrigger("trg_StokDusur"));

            base.OnModelCreating(modelBuilder);
        }
    }
}