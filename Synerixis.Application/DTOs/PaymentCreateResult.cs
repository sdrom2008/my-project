using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class PaymentCreateResult
    {
        public bool Success { get; set; }
        public Dictionary<string, object> PayParams { get; set; } = new();  // 前端调起参数
        public string PaymentUrl { get; set; }  // 支付宝等跳转链接
        public string Message { get; set; }
    }
}
