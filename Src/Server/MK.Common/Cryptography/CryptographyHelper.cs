﻿using System.Security.Cryptography;
using System.Text;

namespace apcurium.MK.Common.Cryptography
{
    public static class CryptographyHelper
    {
        public static byte[] GetHash(string inputString)
        {
            var md5Hasher = MD5.Create();
            return md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static byte[] GetHash(byte[] inputBytes)
        {
            var md5Hasher = MD5.Create();
            return md5Hasher.ComputeHash(inputBytes);
        }

        public static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string GetHashString(byte[] inputBytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in GetHash(inputBytes))
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
