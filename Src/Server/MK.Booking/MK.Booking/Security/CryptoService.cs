using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace apcurium.MK.Booking.Security
{
    public static class CryptoService
    {
        private const string CertificateName = "CN=TaxiHailEncrypt";

        private static X509Certificate2 GetCertificate()
        {
            var x509Store = new X509Store(StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.ReadOnly);

            X509Certificate2 cert = null;
            foreach (var storeCertificate in x509Store.Certificates)
            {
                if (CertificateName.ToLower() == storeCertificate.Subject.ToLower())
                {
                    cert = storeCertificate;
                    break;
                }
            }

            x509Store.Close();

            if (cert == null)
            {
                throw new CryptographicException("The X.509 certificate could not be found.");
            }

            return cert;
        }

        /// <summary>
        /// Max string length to encode depends on the Certificate key length (max: ((KeySize - 384) / 8) + 37)
        /// </summary>
        /// <param name="plainString"></param>
        /// <returns></returns>
        public static string Encrypt(string plainString)
        {
            var rsa = (RSACryptoServiceProvider)GetCertificate().PublicKey.Key;
            var bytestoEncrypt = ASCIIEncoding.ASCII.GetBytes(plainString);
            var encryptedBytes = rsa.Encrypt(bytestoEncrypt, false);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string encryptedString)
        {
            var cert = GetCertificate();
            if (!cert.HasPrivateKey)
            {
                throw new Exception("The X.509 certicate does not contain a private key for decryption");
            }

            var rsa = (RSACryptoServiceProvider)cert.PrivateKey;
            var bytestodecrypt = Convert.FromBase64String(encryptedString);
            var plainbytes = rsa.Decrypt(bytestodecrypt, false);
            var enc = new ASCIIEncoding();
            return enc.GetString(plainbytes);
        }
    }
}