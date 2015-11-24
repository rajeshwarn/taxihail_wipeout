using apcurium.MK.Common.Diagnostic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Cryptography
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class PropertyEncryptAttribute : Attribute
	{
	}

	public static class SettingsEncryptor
	{
		private static byte[] _aes128Key = new byte[16] { 0xae, 0x11, 0xb7, 0x4a, 0x5a, 0x1a, 0x84, 0x13, 0x06, 0x5c, 0x49, 0xb4, 0x23, 0x96, 0x99, 0x7b };
		private static byte[] _initVector = new byte[16] { 0x53, 0x1e, 0x6d, 0x25, 0x80, 0xcc, 0xf0, 0xc1, 0x80, 0xcc, 0x52, 0x5e, 0x90, 0x46, 0xd8, 0x6c };

		private static ILogger _logger;

		public static void SetLogger(ILogger logger)
		{
			_logger = logger;
		}

		public static byte[] Encrypt(string data)
		{
			return CryptographyHelper.EncryptStringToBytes_Aes(data, _aes128Key, _initVector);
		}

		public static string Decrypt(byte[] data)
		{
			if (data == null)
			{
				return null;
			}

			if (data.Length == 0)
			{
				return string.Empty;
			}

			return CryptographyHelper.DecryptStringFromBytes_Aes(data, _aes128Key, _initVector);
		}

		private static string ByteArrayToString(byte[] data)
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

		private static byte[] StringToByteArray(string data)
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

		public static void SwitchEncryptionStringsDictionary(Type instanceType, string rootInstanceName, IDictionary<string, string> data, bool encrypt)
		{
			var instanceProperties = instanceType.GetProperties();

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
						catch (CryptographicException ex)
						{
							_logger.LogError(ex);
						}
					}
				}
				else if (property.PropertyType.BaseType == typeof(object))
				{
					SwitchEncryptionStringsDictionary(instanceProperties[i].PropertyType, key, data, encrypt);
				}
			}
		}
	}
}