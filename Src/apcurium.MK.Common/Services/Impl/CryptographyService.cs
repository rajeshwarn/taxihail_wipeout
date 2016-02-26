using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using PCLCrypto;

namespace apcurium.MK.Common.Services
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class PropertyEncryptAttribute : Attribute
    {
    }

    public class CryptographyService : ICryptographyService
    {
        private readonly byte[] _aes128Key = new byte[16] { 0xae, 0x11, 0xb7, 0x4a, 0x5a, 0x1a, 0x84, 0x13, 0x06, 0x5c, 0x49, 0xb4, 0x23, 0x96, 0x99, 0x7b };
        private readonly byte[] _initVector = new byte[16] { 0x53, 0x1e, 0x6d, 0x25, 0x80, 0xcc, 0xf0, 0xc1, 0x80, 0xcc, 0x52, 0x5e, 0x90, 0x46, 0xd8, 0x6c };

        private readonly ILogger _logger;
        private readonly ICryptographicEngine _cryptographicEngine;
        private readonly ISymmetricKeyAlgorithmProviderFactory _symmetricKeyAlgorithmProviderFactory;
        private readonly IHashAlgorithmProviderFactory _hashAlgorithmProviderFactory;

        public CryptographyService(ICryptographicEngine cryptographicEngine, 
            ISymmetricKeyAlgorithmProviderFactory symmetricKeyAlgorithmProviderFactory, 
            IHashAlgorithmProviderFactory hashAlgorithmProviderFactory,
            ILogger logger)
        {
            _cryptographicEngine = cryptographicEngine;
            _symmetricKeyAlgorithmProviderFactory = symmetricKeyAlgorithmProviderFactory;
            _hashAlgorithmProviderFactory = hashAlgorithmProviderFactory;
            _logger = logger;
        }

        public byte[] Encrypt(string data)
        {
            return EncryptStringToBytes_Aes(data, _aes128Key, _initVector);
        }

        public string Decrypt(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Length == 0)
            {
                return string.Empty;
            }

            return DecryptStringFromBytes_Aes(data, _aes128Key, _initVector);
        }

        public void SwitchEncryptionStringsDictionary(Type instanceType, string rootInstanceName, IDictionary<string, string> data, bool encrypt)
        {
            var instanceProperties = instanceType.GetRuntimeProperties();

            foreach (var property in instanceProperties.Where(pr => pr.CanRead && pr.CanWrite && pr.GetCustomAttribute(typeof(PropertyEncryptAttribute)) != null))
            {
                var key = (rootInstanceName == null ? "" : rootInstanceName + ".") + property.Name;

                if (property.PropertyType == typeof(string))
                {
                    if (!data.ContainsKey(key))
                    {
                        continue;
                    }

                    if (encrypt)
                    {
                        data[key] = data[key] != null ? ByteArrayToString(Encrypt(data[key])) : null;
                    }
                    else
                    {
                        try
                        {
                            data[key] = data[key] != null ? Decrypt(StringToByteArray(data[key])) : null;
                        }
                        catch (FormatException ex)
                        {
                            _logger.LogError(ex);
                        }
                        catch (ArgumentNullException ex)
                        {
                            _logger.LogError(ex);
                        }
                    }
                }
                else if (property.PropertyType.GetTypeInfo().BaseType == typeof(object))
                {
                    SwitchEncryptionStringsDictionary(property.PropertyType, key, data, encrypt);
                }
            }
        }

        public string GetHashString(string inputString)
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

        public string GetHashString(byte[] inputBytes)
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

        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] initVector)
        {
            var provider = _symmetricKeyAlgorithmProviderFactory.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var internalKey = provider.CreateSymmetricKey(key);
            byte[] iv = initVector; // this is optional, but must be the same for both encrypting and decrypting
            byte[] cipherText = _cryptographicEngine.Encrypt(internalKey, Encoding.UTF8.GetBytes(plainText), iv);

            return cipherText;
        }

        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] initVector)
        {
            var provider = _symmetricKeyAlgorithmProviderFactory.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var internalKey = provider.CreateSymmetricKey(key);
            byte[] iv = initVector; // this is optional, but must be the same for both encrypting and decrypting
            byte[] plainText = _cryptographicEngine.Decrypt(internalKey, cipherText, iv);

            return Encoding.UTF8.GetString(plainText, 0, plainText.Length);
        }

        private byte[] GetHash(string inputString)
        {
            var md5Hasher = _hashAlgorithmProviderFactory.OpenAlgorithm(HashAlgorithm.Md5);
            return md5Hasher.HashData(Encoding.UTF8.GetBytes(inputString));
        }

        private byte[] GetHash(byte[] inputBytes)
        {
            var md5Hasher = _hashAlgorithmProviderFactory.OpenAlgorithm(HashAlgorithm.Md5);
            return md5Hasher.HashData(inputBytes);
        }

        private string ByteArrayToString(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            var sb = new StringBuilder();

            foreach (var ch in data)
            {
                var hexValue = ch.ToString("x");

                if (hexValue.Length == 1)
                {
                    sb.Append("0");
                }

                sb.Append(hexValue);
            }

            return sb.ToString();
        }

        private byte[] StringToByteArray(string data)
        {
            if (data == null || data.Length % 2 != 0)
            {
                return null;
            }

            var byteData = new byte[data.Length / 2];

            for (var i = 0; i < data.Length / 2; i++)
            {
                byteData[i] = Convert.ToByte(data.Substring(i * 2, 2), 16);
            }

            return byteData;
        }
    }
}
