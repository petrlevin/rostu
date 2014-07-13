using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Crypto
{
    /// <summary>
    /// Рассширение для шифрование и дешифрования строк
    /// </summary>
    public static class StringExtensioncs
    {
        /// <summary>
        /// Зашифровать строку
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        static public string Encrypt(this string toEncrypt, byte[] key)
        {
            return new Cripter().Encrypt(toEncrypt, key);
        }

        /// <summary>
        /// Зашифровать строку
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <returns></returns>
        static public string Encrypt(this string toEncrypt)
        {
            return new Cripter().Encrypt(toEncrypt, SecurityKey);
        }

        /// <summary>
        /// Расшифровать строку
        /// </summary>
        /// <param name="fromDecrypt"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public static string Decrypt(this string fromDecrypt, byte[] key)
        {
            return new Cripter().Decrypt(fromDecrypt, key);
        }

        /// <summary>
        /// Расшифровать строку
        /// </summary>
        /// <param name="fromDecrypt"></param>
        /// <returns></returns>
        public static string Decrypt(this string fromDecrypt)
        {
            return new Cripter().Decrypt(fromDecrypt, SecurityKey);
        }


        static private readonly byte[] SecurityKey =
        new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

    }
}
