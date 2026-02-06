using System;
using System.Collections.Generic;

namespace KargoTakip.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException() : base("Bir veya daha fazla doğrulama hatası oluştu.")
        {
            Errors = new List<string>();
        }

        public ValidationException(IEnumerable<string> errors) : this()
        {
            Errors.AddRange(errors);
        }
    }
}