using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Infrastructure.Services
{
    public class SmsService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public SmsService(IConfiguration config, IHttpClientFactory factory)
        {
            _config = config;
            _http = factory.CreateClient();
        }

        public async Task SendVerificationCodeAsync(string phone, string code)
        {
            // 示例：阿里云短信
            var accessKeyId = _config["Aliyun:Sms:AccessKeyId"];
            var accessKeySecret = _config["Aliyun:Sms:AccessKeySecret"];
            var signName = _config["Aliyun:Sms:SignName"];
            var templateCode = _config["Aliyun:Sms:TemplateCode"];

            // 实际调用阿里云 API（用阿里云 SDK 或 HTTP）
            // 这里简化成日志输出
            Console.WriteLine($"发送验证码到 {phone}: {code}，签名：{signName}，模板：{templateCode}");

            // 真实集成示例（用阿里云 SDK）：
            // var client = new DefaultAcsClient(profile);
            // var request = new SendSmsRequest { PhoneNumbers = phone, SignName = signName, TemplateCode = templateCode, TemplateParam = "{\"code\":\"" + code + "\"}" };
            // await client.GetAcsResponseAsync(request);
        }
    }
}
