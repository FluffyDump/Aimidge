using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Aimidge.Services
{
    [IgnoreAntiforgeryToken]

    public class CryptoService
    {
        private const string Key = "O1XFeDPaQFAykYcxZZeIM76y1bnTbk92";
        private const string InitVector = "6dwejNHPVlRIWXTE";
        public static string Encrypt(string plainText)
        {
            using (Aes aesEncrypt = Aes.Create())
            {
                aesEncrypt.Key = Encoding.UTF8.GetBytes(Key);
                aesEncrypt.IV = Encoding.UTF8.GetBytes(InitVector);

                ICryptoTransform encryptor = aesEncrypt.CreateEncryptor(aesEncrypt.Key, aesEncrypt.IV);

                try
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ecryption error: " + ex.Message);
                    return null;
                }
            }
        }
    }
}