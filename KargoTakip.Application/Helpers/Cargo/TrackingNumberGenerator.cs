using System;
using System.Security.Cryptography;
using System.Text;

namespace KargoTakip.Application.Helpers.Cargo
{
    public static class TrackingNumberGenerator
    {
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int TrackingNumberLength = 10;

        public static string Generate()
        {
            return GenerateSecureRandomString(TrackingNumberLength);
        }

        private static string GenerateSecureRandomString(int length)
        {
            var result = new StringBuilder(length);
            var buffer = new byte[sizeof(uint)];

            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    var randomNumber = BitConverter.ToUInt32(buffer, 0);
                    var index = randomNumber % Characters.Length;
                    result.Append(Characters[(int)index]);
                }
            }

            return result.ToString();
        }
    }
}
