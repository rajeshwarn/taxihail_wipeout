using System;
using System.Security.Cryptography;
using System.Text;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Services;
using NUnit.Framework;
using PCLCrypto;

namespace apcurium.MK.Common.Tests.Crypto
{
    [TestFixture]
    public class CryptoFixture
    {
        [Test]
        public void Crypto_Hash_String_test()
        {
            var cryptographyService = new CryptographyService(WinRTCrypto.CryptographicEngine, WinRTCrypto.SymmetricKeyAlgorithmProvider, WinRTCrypto.HashAlgorithmProvider, new Logger());

            var valueString = "3nNkJ5EcI7yyi56ifLSAA";
            var hashedString = cryptographyService.GetHashString(valueString, CryptographyHashAlgorithm.Md5);
            var hashedStringWithoutPcl = CryptographyHelperWithoutPcl.GetHashString(valueString);

            Assert.AreEqual(hashedString, hashedStringWithoutPcl);
        }

        [Test]
        public void Encrypt_Settings_test()
        {
            var cryptographyService = new CryptographyService(WinRTCrypto.CryptographicEngine, WinRTCrypto.SymmetricKeyAlgorithmProvider, WinRTCrypto.HashAlgorithmProvider, new Logger());

            var valueString = "3nNkJ5EcI7yyi56ifLSAA";
            var encrypted = cryptographyService.Encrypt(valueString);
            var encryptedWithoutPcl = SettingsEncryptorWithoutPcl.Encrypt(valueString);

            Assert.AreEqual(encrypted, encryptedWithoutPcl);
        }

        [Test]
        public void Decrypt_Settings_test()
        {
            var cryptographyService = new CryptographyService(WinRTCrypto.CryptographicEngine, WinRTCrypto.SymmetricKeyAlgorithmProvider, WinRTCrypto.HashAlgorithmProvider, new Logger());

            var valueByte = new byte[] { 56, 99, 56, 132, 151, 253, 216, 41, 121, 141, 13, 158, 180, 215, 74, 5 };
            var decrypted = cryptographyService.Decrypt(valueByte);
            var decryptedWithoutPcl = SettingsEncryptorWithoutPcl.Decrypt(valueByte);

            Assert.AreEqual(decrypted, decryptedWithoutPcl);
        }

        [Test]
        public void Generate_Hash_test()
        {
            var cryptographyService = new CryptographyService(WinRTCrypto.CryptographicEngine, WinRTCrypto.SymmetricKeyAlgorithmProvider, WinRTCrypto.HashAlgorithmProvider, new Logger());

            var url = "http://localhost/apcurium/page&client=" + "gme-taxihailinc";
            var encoding = new ASCIIEncoding();
            var uri = new Uri(url);
            var encodedPathAndQueryBytes = encoding.GetBytes(uri.LocalPath + uri.Query);

            var usablePrivateKey = "y20g3ePo3ROg4mB-5Vh9C2SojLw=".Replace("-", "+").Replace("_", "/");
            var privateKeyBytes = Convert.FromBase64String(usablePrivateKey);
            var algorithm = new HMACSHA1(privateKeyBytes);

            var hashByteWithoutPcl = algorithm.ComputeHash(encodedPathAndQueryBytes);
            var hashWithoutPcl = Convert.ToBase64String(hashByteWithoutPcl).Replace("+", "-").Replace("/", "_");
            var hash = cryptographyService.GenerateHash(uri.LocalPath + uri.Query, usablePrivateKey).Replace("+", "-").Replace("/", "_");

            Assert.AreEqual(hash, hashWithoutPcl);
        }
    }
}
