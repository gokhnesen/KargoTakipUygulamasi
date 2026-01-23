# Infobip SMS Entegrasyonu

## Genel Bak??
Bu proje, Infobip SMS servisi kullanarak kargo takip bildirimlerini gönderir. Her kargo hareketi (movement) de?i?ikli?inde al?c?ya otomatik SMS bildirimi gönderilir.

## Kurulum

### 1. Infobip Hesab? Olu?turma
1. [Infobip](https://www.infobip.com/) web sitesine gidin
2. Ücretsiz hesap olu?turun
3. Dashboard'dan **API Key** al?n

### 2. Yap?land?rma

`appsettings.json` dosyas?nda Infobip ayarlar?n? yap?land?r?n:

```json
{
  "Infobip": {
    "Enabled": true,
    "BaseUrl": "https://api.infobip.com",
    "ApiKey": "BURAYA_API_KEY_GIRIN",
    "Sender": "KargoTakip"
  }
}
```

**Önemli:** 
- `Enabled`: `true` yaparak Infobip SMS servisini aktif edin
- `ApiKey`: Infobip dashboard'dan ald???n?z API key'i buraya yaz?n
- `Sender`: SMS'lerde görünecek gönderici ad? (maksimum 11 karakter)

### 3. Development/Test için Mock Servis

Geli?tirme ortam?nda gerçek SMS göndermek istemiyorsan?z, `Enabled: false` yap?n. Bu durumda **MockSmsService** kullan?l?r ve SMS'ler sadece log'lara yaz?l?r.

```json
{
  "Infobip": {
    "Enabled": false
  }
}
```

## Mimari

### Servisler

1. **ISmsService (Interface)**: SMS gönderme sözle?mesi
   - Konum: `KargoTakip.Application/Interfaces/Notification/ISmsService.cs`

2. **InfobipSmsService**: Infobip API entegrasyonu
   - Konum: `KargoTakip.Infrastructure/Services/InfobipSmsService.cs`
   - Özellikler:
     - Telefon numaras? formatlama (Türkiye için +90)
     - HTTP client kullanarak Infobip API ça?r?s?
     - Kapsaml? loglama

3. **MockSmsService**: Test/development için sahte SMS servisi
   - Konum: `KargoTakip.Infrastructure/Services/MockSmsService.cs`
   - Gerçek SMS göndermez, sadece log'lar

4. **NotificationService**: SMS ve Email koordinatörü
   - Konum: `KargoTakip.Infrastructure/Services/NotificationService.cs`

### Bildirim Ak???

1. Yeni kargo hareketi olu?turulur (`CreateCargoMovementCommand`)
2. `CreateCargoMovementCommandHandler`:
   - Hareketi veritaban?na kaydeder
   - Kargonun durumunu günceller
   - `SendNotificationToReceiverAsync` metodunu ça??r?r
3. Al?c? bilgileri al?n?r
4. SMS mesaj? olu?turulur
5. `NotificationService` ? `InfobipSmsService` ? Infobip API
6. SMS al?c?ya iletilir

### SMS Mesaj Format?

```
Say?n [Ad] [Soyad], [Takip No] takip numaral? kargonuz [durum]. Konum: [konum]
```

Örnek:
```
Say?n Ahmet Y?lmaz, TK123456789 takip numaral? kargonuz da??t?mda. Konum: ?stanbul Anadolu Kurye
```

## Telefon Numaras? Format?

Servis otomatik olarak telefon numaralar?n? formatlar:

| Girdi | Ç?kt? |
|-------|-------|
| 0532 123 4567 | +905321234567 |
| 532-123-4567 | +905321234567 |
| (532) 123 4567 | +905321234567 |
| +905321234567 | +905321234567 |

## Test Etme

### 1. Mock Servis ile Test
```json
"Infobip": { "Enabled": false }
```
Log dosyas?nda SMS mesajlar?n? görebilirsiniz:
```
[MOCK SMS] Telefon: 05321234567, Mesaj: Say?n Ahmet Y?lmaz...
```

### 2. Gerçek Infobip ile Test
```json
"Infobip": { 
  "Enabled": true,
  "ApiKey": "your-api-key-here"
}
```

API endpoint'i test edin:
```bash
POST /api/cargo/movement
{
  "cargoId": "xxx",
  "newStatus": 2,
  "location": "?stanbul",
  "description": "Kargo da??t?ma ç?kt?",
  "sendNotification": true
}
```

## Hata Yönetimi

Servis a?a??daki durumlarda hata loglar:
- API Key eksik veya geçersiz
- Telefon numaras? format? hatal?
- Infobip API hatas? (network, authentication, vb.)
- Receiver bilgisi bulunamad?

Hatalar `NotificationService` taraf?ndan yakalan?r ve `CreateCargoMovementCommandHandler`'a bildirilir. Handler hata durumunda `NotificationSent: false` döner.

## Güvenlik

- **API Key'i asla kod içinde tutmay?n**
- Production ortam?nda User Secrets veya Azure Key Vault kullan?n
- API Key'i `.gitignore` ile koruyun

### User Secrets Kullan?m? (Önerilen)

```bash
dotnet user-secrets set "Infobip:ApiKey" "your-api-key-here"
```

## Maliyet

Infobip ücretlendirme sistemi:
- Pay-as-you-go: Her SMS için ücret
- Türkiye için ortalama: ~0.02 USD/SMS
- Free tier: Genellikle 100-200 ücretsiz SMS

## Sorun Giderme

### SMS gönderilmiyor
1. `Infobip:Enabled` true mu?
2. API Key do?ru mu?
3. Telefon numaras? geçerli mi?
4. Log dosyalar?n? kontrol edin

### SMS geç ula??yor
- Infobip sunucu yükü olabilir
- Operatör gecikmesi normal (1-5 saniye)

### Test SMS'i alam?yorum
- Infobip dashboard'dan SMS'in gönderildi?ini do?rulay?n
- Telefon numaras? format?n? kontrol edin
- Spam klasörünü kontrol edin (baz? operatörler)

## ?leti?im

Sorular?n?z için: [GitHub Issues](https://github.com/gokhnesen/KargoTakipUygulamasi/issues)
