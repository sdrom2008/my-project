using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    public class PaymentNotifyResult
    {
        public bool Success { get; set; }
        public string OutTradeNo { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }
}
