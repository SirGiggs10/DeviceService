using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.Encryption.SimpleBasicEncryption
{
    /// <summary>
    /// A Simple Utility Class to Encrypt and Decrypt Text
    /// </summary>
    /// <remarks>Author: MAC</remarks>
    public static class SimpleBasicEncryptionUtility
    {
        /// <summary>
        /// Synchronously Encrypt Text
        /// </summary>
        /// <param name="textToEncrypt">Text To Encrypt</param>
        /// <returns>Tuple. FirstItem: Encryption Success Status, SecondItem: Encrypted Text</returns>
        public static (bool, string) EncryptText(string textToEncrypt)
        {
            var encryptedText = string.Empty;

            try
            {
                encryptedText = Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedText));

                return (true, encryptedText);
            }
            catch(Exception ex)
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Synchronously Decrypt Text
        /// </summary>
        /// <param name="textToDecrypt">Text To Decrypt</param>
        /// <returns>Tuple. FirstItem: Decryption Success Status, SecondItem: Decrypted Text</returns>
        public static (bool, string) DecryptText(string textToDecrypt)
        {
            var decryptedText = string.Empty;

            try
            {
                decryptedText = Encoding.UTF8.GetString(Convert.FromBase64String(textToDecrypt));

                return (true, decryptedText);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }
    }
}
