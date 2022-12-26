using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using System;

namespace ItemChecker.Net.Session
{
    internal class RsaPassword : HttpRequest
    {
        public string Mod { get; set; }
        public string Exp { get; set; }
        public string TimeStamp { get; set; }
        public string EncryptedPassword { get; set; }

        internal static RsaPassword GetEncryptedPassword(string accountName, string password)
        {
            var publicKey = GetPasswordRSAPublicKey(accountName);

            byte[] encryptedPasswordBytes;
            using (var rsaEncryptor = new RSACryptoServiceProvider())
            {
                var passwordBytes = Encoding.ASCII.GetBytes(password);
                var rsaParameters = rsaEncryptor.ExportParameters(false);
                rsaParameters.Modulus = Convert.FromHexString(publicKey.Mod);
                rsaParameters.Exponent = Convert.FromHexString(publicKey.Exp);
                rsaEncryptor.ImportParameters(rsaParameters);
                encryptedPasswordBytes = rsaEncryptor.Encrypt(passwordBytes, false);
            }
            publicKey.EncryptedPassword = Convert.ToBase64String(encryptedPasswordBytes);

            return publicKey;
        }
        static RsaPassword GetPasswordRSAPublicKey(string accountName)
        {
            string response = RequestGetAsync("https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1/?account_name=" + accountName).Result;
            JObject json = JObject.Parse(response);

            var publicKey = new RsaPassword()
            {
                Mod = json["response"]["publickey_mod"].ToString(),
                Exp = json["response"]["publickey_exp"].ToString(),
                TimeStamp = json["response"]["timestamp"].ToString(),
            };

            return publicKey;
        }
    }
}