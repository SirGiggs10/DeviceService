using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DeviceService.Core.Helpers.Encryption.EncryptionUtility
{
    /// <summary>
    /// Class to Handle AES (American Encryption Standard) Encryption and Decryption
    /// </summary>
    public static class AES
    {
        public static Exception Exception;

        #region Salt Encryption
        /// <summary>
        /// Encrypt a String with Respect to a Selected Salt Key
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="saltKey"></param>
        /// <returns></returns>
        public static string SaltEncrypt(string originalString, string saltKey)
        {
            try
            {
                var clearBytes = Encoding.Unicode.GetBytes(originalString.Trim());
                using (var encryptor = Aes.Create())
                {
                    if (encryptor == null) return originalString;
                    var pdb = new Rfc2898DeriveBytes(saltKey.Trim(),
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32); encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length); cs.Close();
                        }
                        originalString = Convert.ToBase64String(ms.ToArray());
                    }
                }
                return Encrypt(originalString);
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }

        /// <summary>
        /// Decrypt a String with its Corresponding Salt Key
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="saltKey"></param>
        /// <returns></returns>
        public static string SaltDecrypt(string cipherText, string saltKey)
        {
            try
            {
                cipherText = Decrypt(cipherText);
                var cipherBytes = Convert.FromBase64String(cipherText.Trim());
                using (var encryptor = Aes.Create())
                {
                    if (encryptor == null) return cipherText;
                    var pdb = new Rfc2898DeriveBytes(saltKey.Trim(),
                        new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32); encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length); cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return cipherText.Trim();
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }

        /// <summary>
        /// Check if Encryption is Valid using the Encryption Salt Key
        /// </summary>
        /// <param name="originalString"></param>
        /// <param name="encryptedString"></param>
        /// <param name="saltKey"></param>
        /// <returns></returns>
        public static bool IsSaltEncryptValid(string originalString, string encryptedString, string saltKey)
        {
            return (originalString.Trim() == SaltDecrypt(encryptedString.Trim(), saltKey.Trim()));
        }

        #endregion

        #region Encryption Engine
        private static Random Random => new Random();

        private static IEnumerable<Keys> _GetKeys()
        {
            var lst = new List<Keys>
            {
                new Keys {KeyId = 0, KeyCode = "E46BA11adfs4d6B"},
                new Keys {KeyId = 1, KeyCode = "1FdadfB4dr78C6y"},
                new Keys {KeyId = 2, KeyCode = "aerg01EdewdE024"},
                new Keys {KeyId = 3, KeyCode = "dgr323dafeD6533"},
                new Keys {KeyId = 4, KeyCode = "594dw55yevv3F09"},
                new Keys {KeyId = 5, KeyCode = "6sfg4096ddA3F6z"},
                new Keys {KeyId = 6, KeyCode = "Afsg424FdadE22q"},
                new Keys {KeyId = 7, KeyCode = "54rgACA5Fdae29i"},
                new Keys {KeyId = 8, KeyCode = "sfgsAFvadw6FF22"},
                new Keys {KeyId = 9, KeyCode = "Bsgrg57daf43hv1"},
            };
            return lst;
        }

        private static Keys _GetRandomKey()
        {
            return _GetKeys().FirstOrDefault(x => x.KeyId == Random.Next(0, 9)) ??
                   _GetKeys().FirstOrDefault(x => x.KeyId == 0);
        }

        private static string Encrypt(string originalText) { return _Encrypt(originalText, _GetRandomKey()); }

        private static string _Encrypt(string originalText, Keys objKey)
        {
            var clearBytes = Encoding.Unicode.GetBytes(originalText.Trim());
            using (var encryptor = Aes.Create())
            {
                if (encryptor == null) return originalText;

                var pdb = new Rfc2898DeriveBytes(objKey.KeyCode,
                    new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = pdb.GetBytes(32); encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    originalText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return objKey.KeyId + originalText.Trim();
        }

        private static string Decrypt(string cipherText)
        {
            var objKey = _GetKeys().FirstOrDefault(x => x.KeyId == int.Parse(cipherText.Substring(0, 1)));
            cipherText = cipherText.Remove(0, 1);
            return _Decrypt(cipherText, objKey);
        }

        private static string _Decrypt(string cipherText, Keys objKey)
        {
            var cipherBytes = Convert.FromBase64String(cipherText.Trim());
            using (var encryptor = Aes.Create())
            {
                if (encryptor == null) return cipherText;
                var pdb = new Rfc2898DeriveBytes(objKey.KeyCode, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = pdb.GetBytes(32); encryptor.IV = pdb.GetBytes(16);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length); cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText.Trim();
        }

        public static bool IsEncryptionValid(string originalString, string cipherText)
        {
            return Decrypt(cipherText) == originalString.Trim();
        }

        private class Keys { public int KeyId { get; set; } public string KeyCode { get; set; } }
        #endregion

        #region Special Keys
        /// <summary>
        /// Get Key by Length
        /// </summary>
        /// <param name="keyLength"></param>
        /// <returns></returns>
        public static string GetUniqueKey(int keyLength)
        {
            try
            {
                var key = string.Empty;
                var randomInteger = new Random();

                for (var i = 0; i < keyLength; i++)
                {
                    key += randomInteger.Next(0, 9).ToString();
                }

                return key;
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }
        #endregion
    }
}
