using DeviceService.Core.Helpers.Common;
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
        #region AES Old Implementation
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
        #endregion


        #region AES New Implementation
        public static byte[] EncryptNew(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                aes.BlockSize = 128;
                aes.KeySize = 128;
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        //byte[] c;
                        //cs.Write(c = Encoding.UTF8.GetBytes(plainText), 0, c.Length);
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data
            return encrypted;
        }
        public static string DecryptNew(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        public static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, int keySize = 128)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = cipherMode;
                rijAlg.KeySize = keySize;
                rijAlg.Key = key;
                rijAlg.IV = IV;
                //rijAlg.Padding = PaddingMode.Zeros;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream. 
            return encrypted;
        }

        public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV, CipherMode cipherMode = CipherMode.CBC, int keySize = 128)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.KeySize = keySize;
                rijAlg.Mode = cipherMode;
                rijAlg.Key = Key;
                rijAlg.IV = IV;
                //rijAlg.Padding = PaddingMode.Zeros;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        #region AES Encrypt Text
        public static string AES_EncryptText(string textToEncrypt, string aesKey, string aesIV, int aesCipherMode = Utils.AES_Mode_Default, int aesKeySize = Utils.AES_KeySize_Default, int aesReturnType = Utils.AES_ReturnType_Default)
        {
            try
            {
                var cipherMode = Utils.GetCipherMode(aesCipherMode);
                var keySize = Utils.GetKeySize(aesKeySize);

                var encryptedTextBytes = EncryptStringToBytes(textToEncrypt, Encoding.UTF8.GetBytes(aesKey), Encoding.UTF8.GetBytes(aesIV), cipherMode, keySize);

                return (aesReturnType == Utils.AES_ReturnType_Base64) ? HelperUtility.StringBytesToBase64(encryptedTextBytes) : HelperUtility.StringBytesToHex(encryptedTextBytes, true);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region AES Decrypt Text
        public static string AES_DecryptText(string textToDecrypt, string aesKey, string aesIV, int aesCipherMode = Utils.AES_Mode_Default, int aesKeySize = Utils.AES_KeySize_Default, int aesReturnType = Utils.AES_ReturnType_Default)
        {
            try
            {
                var cipherMode = Utils.GetCipherMode(aesCipherMode);
                var keySize = Utils.GetKeySize(aesKeySize);
                var textToDecryptBytes = (aesReturnType == Utils.AES_ReturnType_Base64) ? HelperUtility.Base64ToStringBytes(textToDecrypt) : HelperUtility.HexToStringBytes(textToDecrypt);

                var decryptedText = DecryptStringFromBytes(textToDecryptBytes, Encoding.UTF8.GetBytes(aesKey), Encoding.UTF8.GetBytes(aesIV), cipherMode, keySize);

                return decryptedText;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #endregion
    }
}
