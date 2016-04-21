using apcurium.MK.Common.Diagnostic;
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
            var hashedString = cryptographyService.GetHashString(valueString);
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


    }
}
