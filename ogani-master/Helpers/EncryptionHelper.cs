using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace ogani_master.Helpers
{
	
	public static class EncryptionHelper
	{
		public static string Encrypt(string text, string key)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			using (var aes = Aes.Create())
			{
				var keyBytes = Encoding.UTF8.GetBytes(key);
				aes.Key = keyBytes.Take(32).ToArray();
				aes.IV = keyBytes.Take(16).ToArray();

				var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (var ms = new MemoryStream())
				{
					using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					{
						using (var sw = new StreamWriter(cs))
						{
							sw.Write(text);
						}
					}
					return Convert.ToBase64String(ms.ToArray());
				}
			}
		}

		public static string Decrypt(string cipherText, string key)
		{
			if (string.IsNullOrEmpty(cipherText))
				return cipherText;

			using (var aes = Aes.Create())
			{
				var keyBytes = Encoding.UTF8.GetBytes(key);
				aes.Key = keyBytes.Take(32).ToArray();
				aes.IV = keyBytes.Take(16).ToArray();

				var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
				{
					using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
					{
						using (var sr = new StreamReader(cs))
						{
							return sr.ReadToEnd();
						}
					}
				}
			}
		}
	}

}
