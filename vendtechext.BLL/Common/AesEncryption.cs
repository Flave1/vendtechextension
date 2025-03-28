using System.Security.Cryptography;
using System.Text;

namespace vendtechext.BLL.Common
{
    public class AesEncryption
    {
        const string key = "1234567890123456"; 
        const string iv = "1234567890123456";
        // Encrypt a string and return the encrypted data as a base64-encoded string
        public static string Encrypt(string plainText, string key = key, string iv = iv)
        {
            plainText = plainText + DateTime.UtcNow.ToString();
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.WriteAsync(plainText);
                    sw.FlushAsync();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // Decrypt a base64-encoded string and return the decrypted data as a string
        public static string Decrypt(string cipherText, string key = key, string iv = iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
