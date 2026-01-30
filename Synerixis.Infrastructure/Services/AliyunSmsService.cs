using AlibabaCloud.SDK.Dysmsapi20170525;
using AlibabaCloud.SDK.Dysmsapi20170525.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Synerixis.Infrastructure.Services
{
    public class AliyunSmsService
    {
        private readonly Client _client;
        private readonly IConfiguration _config;
        private readonly ILogger<AliyunSmsService> _logger;

        public AliyunSmsService(IConfiguration config, ILogger<AliyunSmsService> logger)
        {
            _config = config;
            _logger = logger;

            var accessKeyId = _config["AliyunSms:AccessKeyId"];
            var accessKeySecret = _config["AliyunSms:AccessKeySecret"];

            var clientConfig = new AlibabaCloud.OpenApiClient.Models.Config
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
                Endpoint = "dysmsapi.aliyuncs.com",
                RegionId = "cn-hangzhou"
            };

            _client = new Client(clientConfig);
        }

        public async Task<bool> SendVerificationCodeAsync(string phone, string code)
        {
            var signName = _config["AliyunSms:SignName"];
            var templateCode = _config["AliyunSms:TemplateCode"];

            if (string.IsNullOrEmpty(signName) || string.IsNullOrEmpty(templateCode))
            {
                _logger.LogError("阿里云短信配置缺失");
                return false;
            }

            var request = new SendSmsRequest
            {
                PhoneNumbers = phone,
                SignName = signName,
                TemplateCode = templateCode,
                TemplateParam = "{\"code\":\"" + code + "\"}"
            };

            try
            {
                var response = await _client.SendSmsAsync(request);

                if (response.Body.Code == "OK")
                {
                    _logger.LogInformation("短信发送成功: {Phone}, 验证码 {Code}", phone, code);
                    return true;
                }

                _logger.LogWarning("短信发送失败: {Phone}, 错误 {Code} - {Message}",
                    phone, response.Body.Code, response.Body.Message);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送短信异常: {Phone}", phone);
                return false;
            }
        }
    }
}