using System;

namespace KargoTakip.Application.Exceptions
{
    /// <summary>
    /// Harici servis (RabbitMQ, e-posta sağlayıcı vb.) hatalarında fırlatılır.
    /// </summary>
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message) : base(message)
        {
        }

        public ExternalServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
