[README.md](https://github.com/user-attachments/files/24351689/README.md)
# Sinema Otomasyonu - Kurulum ve Kullanım Kılavuzu

Bu proje, ASP.NET Core MVC ve SQL Server kullanılarak geliştirilmiş kapsamlı bir Sinema Otomasyon Sistemidir. Aşağıdaki adımları takip ederek projeyi çalıştırabilirsiniz.

## 1. Veritabanı Kurulumu (ÇOK ÖNEMLİ)
1. Proje klasöründeki **`Setup.sql`** dosyasını açın.
2. SQL Server Management Studio (SSMS) veya Visual Studio içindeki SQL Server Object Explorer ile **`(localdb)\mssqllocaldb`** sunucusuna bağlanın.
3. `Setup.sql` içeriğini kopyalayıp yeni bir sorgu penceresinde çalıştırın (F5).
   - Bu işlem `SinemaOtomasyonu` veritabanını oluşturacak, tabloları, SP'leri, Trigger'ları ve örnek verileri yükleyecektir.

## 2. Projeyi Çalıştırma
1. Terminal veya Komut Satırını açın ve proje klasörüne gidin.
2. Aşağıdaki komutları sırasıyla çalıştırın:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Uygulama `https://localhost:7198` (veya benzeri bir port) adresinde çalışmaya başlayacaktır.

## 3. Giriş Bilgileri (Örnek Veriler)

### Admin Girişi (Yönetim Paneli için)
- **E-Posta:** `admin@sinema.com`
- **Şifre:** `1234`
- **Yetki:** Film ekleyebilir, tüm sistemi yönetir.

### Müşteri Girişi (Bilet Alımı için)
- **E-Posta:** `ahmet@gmail.com`
- **Şifre:** `1234`
- **Yetki:** Bilet tipi seçimi, film/seans seçimi, koltuk seçimi ve büfe alışverişi yapabilir.

## 4. Proje Özellikleri
- **Tek Dosya Modeller:** Tüm veritabanı sınıfları ve View Modeller `Models/TumModeller.cs` içinde toplanmıştır.
- **Trigger Koruması:** Aynı koltuğun tekrar satılmasını veritabanı seviyesinde `trg_AyniKoltukEngelle` trigger'ı önler.
- **Stok Takibi:** Büfe satışlarında `trg_StokDusur` trigger'ı devreye girer.
- **Transaction:** Ödeme sırasında bilet ve ürün satışı tek bir transaction ("Ya hepsi ya hiç") içinde yapılır.
