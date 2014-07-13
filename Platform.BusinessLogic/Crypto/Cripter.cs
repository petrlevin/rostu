using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Platform.BusinessLogic.Crypto
{
    public class Cripter
    {
        public string Encrypt(string toEncrypt, byte[] keyArray)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            try
            {
                try
                {
                    ICryptoTransform cTransform = tdes.CreateEncryptor();

                    byte[] resultArray =
                      cTransform.TransformFinalBlock(toEncryptArray, 0,
                      toEncryptArray.Length);
                    return Convert.ToBase64String(resultArray, 0, resultArray.Length);

                }
                catch (Exception ex)
                {
                    throw new PlatformException(String.Format("Ошибка шифрования строки '{0}'. ", toEncrypt), ex);
                }

            }
            finally
            {
                tdes.Clear();
                tdes.Dispose();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cipherString"></param>
        /// <param name="keyArray"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public string Decrypt(string cipherString, byte[] keyArray)
        {
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);



            


            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            try
            {
                try
                {

                    var cTransform = tdes.CreateDecryptor();
                    byte[] resultArray = cTransform.TransformFinalBlock(
                        toEncryptArray, 0, toEncryptArray.Length);
                    return Encoding.UTF8.GetString(resultArray);
                }
                catch (Exception ex)
                {
                    throw new PlatformException(String.Format("Ошибка шифрования строки '{0}'. ", cipherString), ex);
                }

            }
            finally
            {
                tdes.Clear();
                tdes.Dispose();
            }


        }

    }
}
