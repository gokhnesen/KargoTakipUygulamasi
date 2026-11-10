# KargoTakip

Kargo gönderilerinin oluşturulduğu, durumunun güncellendiği ve takip numarasıyla sorgulandığı bir operasyon uygulaması. Müşteri tarafında herkese açık takip, operasyon tarafında listeleme, filtreleme ve hareket kaydı var.

Backend .NET 10 ve Clean Architecture ile, arayüz Angular 20 ile yazıldı. Uygulama .NET Aspire üzerinden API, RabbitMQ ve istemciyi birlikte ayağa kaldırır.

## Ne yapıyor

- Takip numarasıyla kargo sorgulama. Sonuçta güncel durum, son konum, gönderici, alıcı ve hareket geçmişi döner.
- Yeni gönderi oluşturma. Takip numarası kriptografik rastgele üretilir.
- Operasyon listesi: durum, tarih, takip numarası, gönderici, alıcı ve e-posta ile filtre; tarih, durum veya takip numarasına göre sıralama; sayfalama.
- Durum güncelleme. Bir gönderi şu aşamalardan geçer: teslim alındı, işleniyor, yolda, dağıtımda, teslim edildi, teslim edilemedi.
- Personel hesabı: kayıt, e-posta onayı, parola, 5 hatalı denemede 15 dakika kilitleme.
- Girişte 6 haneli e-posta kodu (10 dakika). Daha önce doğrulanmış cihaz bu adımı atlar.
- Onay ve giriş mailleri dayanıklı bir RabbitMQ kuyruğuna yazılır; arka plan tüketicisi gönderir. Brevo SMTP açıksa gerçek posta gider, kapalıysa geliştirme servisi devreye girer.

## Teknik olarak öne çıkanlar

- **CQRS.** Komut ve sorgular MediatR ile ayrıldı. FluentValidation, MediatR pipeline’ında çalışır.
- **Specification.** Liste sorguları tek bir spesifikasyonda birleşir: filtre, include, sıralama ve sayfalama. Arama varken toplam kayıt sayısı yerine bir fazla satır çekilerek sonraki sayfa anlaşılır.
- **Arama indeksleri.** Takip numarası, durum + oluşturulma tarihi, gönderici adı, alıcı adı-soyadı ve e-posta üzerinde, silinmemiş kayıtları hedefleyen filtreli indeksler var.
- **Toplu veri.** SQL Server `SqlBulkCopy` ile milyonlarca gönderi ve hareket basılabiliyor. Amaç, liste ve arama sorgularını gerçek hacimde denemek. Uç nokta yetkilendirilmiş ve yalnızca SQL Server’da çalışır.
- **E-posta kuyruğu.** Yayınlayan servis ile tüketen `BackgroundService` ayrı. Kuyruk dayanıklı, mesajlar kalıcı, tüketici bağlantı kopunca yeniden dener ve iş bitince ack verir.
- **Kimlik.** ASP.NET Core Identity, JWT (rol claim’leri dahil), e-posta onayı zorunlu, güvenilir cihaz çerezi.
- **Gözlemlenebilirlik.** Serilog (konsol ve dosya), istek logu, OpenTelemetry ve `/health`, `/alive` sağlık uçları (Aspire service defaults).
- **Hata modeli.** Doğrulama, kimlik, bulunamadı, iş kuralı ve dış servis hataları global middleware’de ayrı HTTP kodlarına map edilir.

## Mimari

```
KargoTakip.Client          Angular arayüz
KargoTakip.API             HTTP, JWT, Swagger, e-posta tüketicisi
KargoTakip.Application     use case’ler, doğrulama, spesifikasyonlar
KargoTakip.Domain          varlıklar ve durumlar
KargoTakip.Infrastructure  RabbitMQ, e-posta, OTP önbelleği
KargoTakip.Persistence     EF Core, Identity, repository, migrasyonlar
KargoTakip.AppHost         Aspire: API + RabbitMQ + Angular
KargoTakip.ServiceDefaults OpenTelemetry, health check, service discovery
```

Bağımlılık içeriye doğru akar. Domain hiçbir katmanı tanımaz. Application yalnızca arayüzleri tanımlar; EF Core, RabbitMQ ve SMTP bu arayüzlerin arkasında kalır.

## Teknolojiler

| Katman | Seçim |
| --- | --- |
| API | ASP.NET Core, .NET 10 |
| Uygulama | MediatR, FluentValidation |
| Veri | EF Core 10, SQL Server, ASP.NET Core Identity |
| Mesajlaşma | RabbitMQ |
| E-posta | Brevo SMTP (opsiyonel) |
| Kimlik | JWT Bearer |
| Log | Serilog, OpenTelemetry |
| Arayüz | Angular 20, standalone component, RxJS |
| Orkestrasyon | .NET Aspire 9.5 |
| API dokümantasyonu | Swagger |

## Ekranlar

| Rota | İş |
| --- | --- |
| `/` | Ana sayfa, takip kutusu ve sonuç zaman çizelgesi |
| `/track` | Takip numarasıyla sorgulama |
| `/login` | Giriş ve e-posta doğrulama kodu |
| `/confirm-email` | Kayıt sonrası e-posta onayı |
| `/cargo-management` | Liste, arama, yeni gönderi, durum ve konum güncelleme |

## API

| Metot | Uç | İş |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Kayıt, onay maili kuyruğa yazılır |
| `POST` | `/api/auth/confirm-email` | E-posta onayı |
| `POST` | `/api/auth/resend-confirmation-email` | Onay mailini yeniden gönder |
| `POST` | `/api/auth/login` | Parola kontrolü; gerekirse OTP |
| `POST` | `/api/auth/verify-login-otp` | Kod doğrulama, JWT üretimi |
| `POST` | `/api/cargo` | Gönderi oluştur |
| `GET` | `/api/cargo` | Filtreli, sayfalı liste |
| `GET` | `/api/cargo/{id}` | Tekil gönderi |
| `GET` | `/api/cargo/track/{trackingNumber}` | Herkese açık takip |
| `POST` | `/api/cargo/movements` | Hareket ekle |
| `PUT` | `/api/cargo/movements/update` | Hareket güncelle |
| `GET` | `/api/cargo/{cargoId}/movements` | Bir gönderinin hareketleri |
| `POST` | `/api/admin/seed-data` | Toplu test verisi (yetkili) |
| `DELETE` | `/api/admin/clear-data` | Kargo verisini temizle (yetkili) |
| `GET` | `/api/admin/stats` | Kayıt sayıları (yetkili) |

Geliştirme ortamında Swagger, API’nin HTTPS portunda açılır.

## Çalıştırma

Gerekenler: .NET 10 SDK, Node.js, SQL Server, bir RabbitMQ (Aspire konteynerini kendi açar).

1. API için bağlantı dizesi ve JWT ayarı verin (`ConnectionStrings:DefaultConnection`, `JwtSettings:SecretKey`, `JwtSettings:Issuer`, `JwtSettings:Audience`, `JwtSettings:ExpirationMinutes`). Kullanıcı sırları veya ortam değişkeni yeterlidir; bu değerler repoya konmaz.
2. Veritabanını migrasyonlarla oluşturun:

```bash
dotnet ef database update --project KargoTakip.Persistence --startup-project KargoTakip.API
```

3. Aspire AppHost’u çalıştırın. API (`https://localhost:7055`), RabbitMQ (yönetim eklentisiyle) ve Angular (`https://localhost:4200`) birlikte kalkar:

```bash
dotnet run --project KargoTakip.AppHost
```

İstemciyi tek başına almak için `KargoTakip.Client/KargoTakipClient` içinde `npm install` ve `npm start`.

Brevo kullanılacaksa `Brevo:Enabled` true olmalı ve SMTP kullanıcı adı, parolası ile gönderen adresi doldurulmalıdır. Aksi halde mailler kuyruğa girer, geliştirme servisi gönderimi loglar.

## Veri modeli

- **Cargo.** Takip numarası, durum, son konum; gönderici ve alıcıya bağlı.
- **CargoSender / CargoReceiver.** Ad, adres, telefon; alıcıda e-posta.
- **CargoMovement.** Yeni durum, konum, açıklama, zaman.
- **ApplicationUser.** Identity kullanıcısı. E-posta onayı ve hesap kilidi açık.

Kayıtlar fiziksel silinmez. Sorgular `IsDeleted = 0` üzerinden gider ve indeksler de bu filtreyle tanımlıdır.
