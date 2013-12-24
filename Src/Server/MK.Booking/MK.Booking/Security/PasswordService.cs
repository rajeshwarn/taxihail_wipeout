#region

using System;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace apcurium.MK.Booking.Security
{
    public class PasswordService : IPasswordService
    {
        private const string HashKey =
            "nTzeqlFYUZn50LLXUfDC05sAdk5Z686EuKxC7Apvg4xl3ChCQZ2Bn4Bq0WT2wKDe++csoTGb74XBTwaHpb+nmeU2qjwESH/WAqOL7ucMKHUXdLA1CvVRdpKsRBlJSeZKnNvlNlDRnBtaY+01hf/ShEotHHtdlBwnIn2cYM4z5js";

        private readonly string[] _strCharacters =
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
            "W", "X", "Y", "Z",
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"
        };

        private Random _random;

        public byte[] EncodePassword(string password, string salt)
        {
            var plainText = Encoding.Default.GetBytes(password + salt);
            var hashKey = Encoding.Default.GetBytes(HashKey);
            var keyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA1");

            keyedHashAlgorithm.Key = hashKey;

            return keyedHashAlgorithm.ComputeHash(plainText);
        }

        public string GeneratePassword()
        {
            _random = new Random(DateTime.Now.Second);
            var strPass = "";
            for (var x = 0; x <= 6; x++)
            {
                var p = _random.Next(0, _strCharacters.Length);
                strPass += _strCharacters[p];
            }
            return strPass;
        }

        public bool IsValid(string passwordSubmitted, string salt, byte[] password)
        {
            try
            {
                var plainText = Encoding.Default.GetBytes(passwordSubmitted + salt);
                var hashKey = Encoding.Default.GetBytes(HashKey);
                var keyedHashAlgorithm = KeyedHashAlgorithm.Create("HMACSHA1");

                keyedHashAlgorithm.Key = hashKey;

                var hashedPassword = keyedHashAlgorithm.ComputeHash(plainText);

                var dbValue = password;

                if (CompareBytesArray(hashedPassword, dbValue))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                //If something bad happens return false
                return false;
            }
        }

        private bool CompareBytesArray(byte[] a, byte[] b)
        {
            var result = true;

            if (a == null)
            {
                throw new ArgumentNullException("a");
            }
            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (a.Length != b.Length)
            {
                result = false;
            }
            else
            {
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i])
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}