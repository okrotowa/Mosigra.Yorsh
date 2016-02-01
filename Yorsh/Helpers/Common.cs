﻿using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Yorsh
{
	public static class Base64
	{
		public static string Encode(string plainText)
		{
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

			return Convert.ToBase64String(plainTextBytes);
		}

		public static string Decode(string base64EncodedData)
		{
			var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

			return Encoding.UTF8.GetString(base64EncodedBytes);
		}

		/// <summary>
		/// AES (Advanced Encryption Standard) implementation with 128-bit key (default)
		/// - 128-bit AES is approved  by NIST, but not the 256-bit AES
		/// - 256-bit AES is slower than the 128-bit AES (by about 40%)
		/// - Use it for secure data protection
		/// - Do NOT use it for data protection in RAM (in most common scenarios)
		/// </summary>
		public static class AES
		{
			public static int KeyLength = 128;
			private const string SaltKey = "ShMG8hLyZ7k~Ge5@";
			private const string VIKey = "~6YUi0Sv5@|{aOZO"; // TODO: Generate random VI each encryption and store it with encrypted value

			public static string Encrypt(byte[] value, string password)
			{
				var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
				var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
				var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

				using (var memoryStream = new MemoryStream())
				{
					using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						cryptoStream.Write(value, 0, value.Length);
						cryptoStream.FlushFinalBlock();
						cryptoStream.Close();
						memoryStream.Close();

						return Convert.ToBase64String(memoryStream.ToArray());
					}
				}
			}

			public static string Encrypt(string value, string password)
			{
				return Encrypt(Encoding.UTF8.GetBytes(value), password);
			}

			public static string Decrypt(string value, string password)
			{
				var cipherTextBytes = Convert.FromBase64String(value);
				var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
				var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None };
				var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

				using (var memoryStream = new MemoryStream(cipherTextBytes))
				{
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
					{
						var plainTextBytes = new byte[cipherTextBytes.Length];
						var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

						memoryStream.Close();
						cryptoStream.Close();

						return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
					}
				}
			}


			public class B64X
			{
				public static byte[] Key = Guid.NewGuid().ToByteArray();

				public static string Encode(string value)
				{
					return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Key));
				}

				public static string Decode(string value)
				{
					return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Key));
				}

				public static string Encrypt(string value, string key)
				{
					return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(key)));
				}

				public static string Decrypt(string value, string key)
				{
					return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Encoding.UTF8.GetBytes(key)));
				}

				private static byte[] Encode(byte[] bytes, byte[] key)
				{
					var j = 0;

					for (var i = 0; i < bytes.Length; i++)
					{
						bytes[i] ^= key[j];

						if (++j == key.Length)
						{
							j = 0;
						}
					}

					return bytes;
				}
			}
		}
	}
}

