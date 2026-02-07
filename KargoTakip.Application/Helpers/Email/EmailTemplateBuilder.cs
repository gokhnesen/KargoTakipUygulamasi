using System;
using System.Text;
using CargoReceiver = KargoTakip.Domain.Entities.CargoReceiver;

namespace KargoTakip.Application.Helpers.Email
{
    public static class EmailTemplateBuilder
    {
        /// <summary>Şirket adı - tüm e-postalarda kullanılır.</summary>
        public const string CompanyName = "Kargonuzu Gezdiriyoruz";

        /// <summary>Şirket sloganı.</summary>
        public const string CompanyTagline = "Kargonuzu güvenle takip edin";

        /// <summary>Logo URL (isteğe bağlı). Boşsa sadece metin header kullanılır.</summary>
        public static string? LogoUrl { get; set; }

        private const string HeaderBgColor = "#E85D04";
        private const string FooterTextColor = "#6B7280";
        private const string DividerColor = "#E5E7EB";

        /// <summary>E-posta gövdesini şirket header ve footer ile sarar.</summary>
        public static string WrapWithBranding(string bodyHtml)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px; margin:0 auto; font-family:'Segoe UI',Arial,sans-serif; font-size:15px; line-height:1.5; color:#374151;"">");
            sb.AppendLine(GetEmailHeaderHtml());
            sb.AppendLine(@"<tr><td style=""padding:24px 32px; background:#ffffff;"">");
            sb.AppendLine(bodyHtml);
            sb.AppendLine("</td></tr>");
            sb.AppendLine(GetEmailFooterHtml());
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        /// <summary>Şirket logosu ve adı ile e-posta header HTML.</summary>
        public static string GetEmailHeaderHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"<tr><td style=""background:{HeaderBgColor}; padding:20px 32px; text-align:center;"">");
            if (!string.IsNullOrEmpty(LogoUrl))
            {
                sb.AppendLine($@"<img src=""{LogoUrl}"" alt=""{CompanyName}"" width=""120"" height=""40"" style=""display:block; margin:0 auto 12px; max-height:48px;"" />");
            }
            sb.AppendLine($@"<h1 style=""margin:0; color:#ffffff; font-size:22px; font-weight:700;"">{CompanyName}</h1>");
            sb.AppendLine($@"<p style=""margin:6px 0 0; color:rgba(255,255,255,0.9); font-size:13px;"">{CompanyTagline}</p>");
            sb.AppendLine("</td></tr>");
            return sb.ToString();
        }

        /// <summary>Şirket adı ve telif ile e-posta footer HTML.</summary>
        public static string GetEmailFooterHtml()
        {
            var year = DateTime.UtcNow.Year;
            var sb = new StringBuilder();
            sb.AppendLine($@"<tr><td style=""padding:20px 32px; background:#F9FAFB; border-top:1px solid {DividerColor}; text-align:center; font-size:12px; color:{FooterTextColor};"">");
            sb.AppendLine($@"<p style=""margin:0 0 4px; font-weight:600; color:#374151;"">{CompanyName}</p>");
            sb.AppendLine($@"<p style=""margin:0;"">© {year} {CompanyName}. Tüm hakları saklıdır.</p>");
            sb.AppendLine("</td></tr>");
            return sb.ToString();
        }

        public static string BuildCargoStatusUpdateEmail(
            CargoReceiver receiver,
            KargoTakip.Domain.Entities.Cargo cargo,
            string statusMessage,
            string location,
            string description)
        {
            var inner = new StringBuilder();
            inner.AppendLine($"<h2 style=\"margin:0 0 16px; font-size:18px; color:#111827;\">Merhaba {receiver.Name} {receiver.Surname},</h2>");
            inner.AppendLine($"<p style=\"margin:0 0 12px;\">Takip numarası <strong>{cargo.TrackingNumber}</strong> olan kargonuzun durumu güncellendi.</p>");
            inner.AppendLine($@"<hr style=""border:none; border-top:1px solid #E5E7EB; margin:20px 0;"" />");
            inner.AppendLine($"<p style=\"margin:0 0 8px;\"><strong>Yeni durum:</strong> {statusMessage}</p>");
            inner.AppendLine($"<p style=\"margin:0 0 8px;\"><strong>Konum:</strong> {location}</p>");
            inner.AppendLine($"<p style=\"margin:0 0 8px;\"><strong>Açıklama:</strong> {description}</p>");
            inner.AppendLine($"<p style=\"margin:0 0 16px;\"><strong>Güncelleme tarihi:</strong> {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC)</p>");
            inner.AppendLine($@"<hr style=""border:none; border-top:1px solid #E5E7EB; margin:20px 0;"" />");
            inner.AppendLine("<p style=\"margin:0 0 8px;\">Kargonuzu takip etmek için takip numaranızı web sitemizde kullanabilirsiniz.</p>");
            inner.AppendLine("<p style=\"margin:0;\">İyi günler dileriz.</p>");

            return WrapWithBranding(inner.ToString());
        }
    }
}
