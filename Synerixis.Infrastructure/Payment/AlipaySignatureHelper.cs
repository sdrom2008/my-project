using Microsoft.Extensions.Configuration;
using Synerixis.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Synerixis.Infrastructure.Payment
{
    public static class AlipaySignatureHelper
    {
        /// <summary>
        /// 生成 RSA2 签名
        /// </summary>
        public static string Sign(string content, string privateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);  // PKCS8 格式
            // 如果是 PKCS1 格式，用： rsa.ImportRSAPrivateKey(...)

            var data = Encoding.UTF8.GetBytes(content);
            var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// 验证支付宝回调签名
        /// </summary>
        public static bool Verify(string content, string sign, string alipayPublicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(alipayPublicKey), out _);

            var data = Encoding.UTF8.GetBytes(content);
            var signature = Convert.FromBase64String(sign);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 构造待签名字符串（按字典序排序）
        /// </summary>
        public static string GetSignContent(Dictionary<string, string> parameters)
        {
            var sorted = new SortedDictionary<string, string>(parameters);
            var sb = new StringBuilder();
            foreach (var kv in sorted)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "sign" && kv.Key != "sign_type")
                {
                    sb.Append(kv.Key).Append("=").Append(kv.Value).Append("&");
                }
            }
            return sb.ToString().TrimEnd('&');
        }
    }
}