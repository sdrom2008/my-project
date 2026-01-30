using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Domain.Common
{
    public static class WxDecryptHelper
    {
        public class PhoneInfo
        {
            public string PhoneNumber { get; set; }
            public string PurePhoneNumber { get; set; }
            public string CountryCode { get; set; }
            public Watermark Watermark { get; set; }
        }

        public class Watermark
        {
            public string Timestamp { get; set; }
            public string AppId { get; set; }
        }

        public static PhoneInfo DecryptPhone(string encryptedData, string iv, string sessionKey, string appId)
        {
            var key = Convert.FromBase64String(sessionKey);
            var ivBytes = Convert.FromBase64String(iv);
            var data = Convert.FromBase64String(encryptedData);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = ivBytes;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);

            var json = Encoding.UTF8.GetString(decrypted);
            var phoneInfo = JsonConvert.DeserializeObject<PhoneInfo>(json);

            if (phoneInfo.Watermark.AppId != appId)
                throw new Exception("AppId 不匹配");

            return phoneInfo;
        }
    }
}
