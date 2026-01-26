using KargoTakip.Domain.Entities;
using System;
using System.Text;

namespace KargoTakip.Application.Services
{
    public class EmailTemplateBuilder
    {
        public string BuildCargoStatusUpdateEmail(
            CargoReceiver receiver,
            Cargo cargo,
            string statusMessage,
            string location,
            string description)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<html><body>");
            sb.AppendLine($"<h2>Merhaba {receiver.Name} {receiver.Surname},</h2>");
            sb.AppendLine($"<p>Takip numarasi <strong>{cargo.TrackingNumber}</strong> olan kargonuzun durumu guncellendi.</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p><strong>Yeni Durum:</strong> {statusMessage}</p>");
            sb.AppendLine($"<p><strong>Konum:</strong> {location}</p>");
            sb.AppendLine($"<p><strong>Aciklama:</strong> {description}</p>");
            sb.AppendLine($"<p><strong>Guncelleme Tarihi:</strong> {DateTime.Now:dd.MM.yyyy HH:mm}</p>");
            sb.AppendLine($"<hr/>");
            sb.AppendLine($"<p>Kargonuzu takip etmek icin takip numaranizi kullanabilirsiniz.</p>");
            sb.AppendLine($"<p>Iyi gunler dileriz.</p>");
            sb.AppendLine($"</body></html>");
            return sb.ToString();
        }
    }
}
