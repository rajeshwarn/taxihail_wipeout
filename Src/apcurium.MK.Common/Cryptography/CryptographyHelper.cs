using System.Text;
using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore;
using PCLCrypto;

namespace apcurium.MK.Common.Cryptography
{
    public static class CryptographyHelper
    {
        public static byte[] GetHash(string inputString)
        {
            var md5Hasher = Mvx.Resolve<IHashAlgorithmProviderFactory>().OpenAlgorithm(HashAlgorithm.Md5);
            return md5Hasher.HashData(Encoding.UTF8.GetBytes(inputString));
        }

        public static byte[] GetHash(byte[] inputBytes)
        {
            var md5Hasher = Mvx.Resolve<IHashAlgorithmProviderFactory>().OpenAlgorithm(HashAlgorithm.Md5);
            return md5Hasher.HashData(inputBytes);
        }

        public static string GetHashString(string inputString)
        {
            if (!inputString.HasValue())
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string GetHashString(byte[] inputBytes)
        {
            if (inputBytes == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputBytes))
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

		public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] initVector)
		{
            var provider = Mvx.Resolve<ISymmetricKeyAlgorithmProviderFactory>().OpenAlgorithm(SymmetricAlgorithm.AesCbc);
            var internalKey = provider.CreateSymmetricKey(key);
            byte[] iv = initVector; // this is optional, but must be the same for both encrypting and decrypting
            byte[] cipherText = Mvx.Resolve<ICryptographicEngine>().Encrypt(internalKey, Encoding.UTF8.GetBytes(plainText), iv);

            return cipherText;

            //

		}

		public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] initVector)
		{
            var provider = Mvx.Resolve<ISymmetricKeyAlgorithmProviderFactory>().OpenAlgorithm(SymmetricAlgorithm.AesCbc);
            var internalKey = provider.CreateSymmetricKey(key);
            byte[] iv = initVector; // this is optional, but must be the same for both encrypting and decrypting
            byte[] plainText = Mvx.Resolve<ICryptographicEngine>().Decrypt(internalKey, cipherText, iv);

            return Encoding.UTF8.GetString(plainText, 0, plainText.Length);
		}
    }
}
