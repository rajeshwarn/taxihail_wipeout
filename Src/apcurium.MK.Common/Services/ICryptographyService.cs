using System;
using System.Collections.Generic;

namespace apcurium.MK.Common.Services
{
    public interface ICryptographyService
    {
        byte[] Encrypt(string data);

        string Decrypt(byte[] data);

        void SwitchEncryptionStringsDictionary(Type instanceType, string rootInstanceName,
            IDictionary<string, string> data, bool encrypt);

        string GetHashString(string inputString);

        string GetHashString(byte[] inputBytes);

        byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] initVector);

        string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] initVector);
    }
}
