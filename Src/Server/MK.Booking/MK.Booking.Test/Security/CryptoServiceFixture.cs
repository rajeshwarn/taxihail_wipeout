using apcurium.MK.Booking.Security;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Security
{
    [TestFixture]
    public class CryptoServiceFixture
    {
        private const string StringToEncode =
            "janshduiahsdmaidjaidjuadhuasjdasjdoiahjdsoahsdoadasda" +
            "ssheo4i3jtdi4j58m3riom389ruwj90gkw-90tgk02gmk0925mgk9" +
            "054jg0mj8954jg90j50gm45-g-4goi3jt8943j5ti3m54t89gj454" +
            "irjm834mtu4904g86kf908w4369043j5f8906654b8wjp6iw-0435" +
            "o6[0-4w5906ge94e9t2yja25jgge967g=";
     
        private string _encodedString;

        [SetUp]
        public void Setup()
        {
            _encodedString = CryptoService.Encrypt(StringToEncode);
        }

        [Test]
        public void when_decrypting_we_get_same_value_as_string_to_encode()
        {
            var decodedString = CryptoService.Decrypt(_encodedString);

            Assert.AreEqual(StringToEncode, decodedString);
        }

        [Test]
        public void when_encrypting_and_decrypting_we_get_same_value_as_string_to_encode()
        {
            var encodedString = CryptoService.Encrypt(StringToEncode);
            var decodedString = CryptoService.Decrypt(encodedString);

            Assert.AreEqual(StringToEncode, decodedString);
        }
    }
}