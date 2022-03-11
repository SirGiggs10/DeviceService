using DeviceService.Core.Helpers.Encryption.SimpleBasicEncryption;

namespace DeviceService.Core.Helpers.Cloudinary
{
    public class CloudinarySettings
    {
        private string _CloudName { get; set; }
        public string CloudName
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_CloudName);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _CloudName = value;
            }
        }

        private string _ApiKey { get; set; }
        public string ApiKey
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_ApiKey);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _ApiKey = value;
            }
        }

        private string _ApiSecret { get; set; }
        public string ApiSecret
        {
            get
            {
                var decryptedTextObject = SimpleBasicEncryptionUtility.DecryptText(_ApiSecret);

                return decryptedTextObject.Item1 ? decryptedTextObject.Item2 : string.Empty;
            }
            set
            {
                _ApiSecret = value;
            }
        }
    }
}