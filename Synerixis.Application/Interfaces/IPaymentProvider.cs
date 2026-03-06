using Microsoft.AspNetCore.Http;
using Synerixis.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.Interfaces
{
    public interface IPaymentProvider
    {
        string Channel { get; }  // "wechat", "alipay"

        Task<PaymentCreateResult> CreateOrderAsync(PaymentCreateRequest request, Guid sellerId);

        Task<PaymentNotifyResult> HandleNotifyAsync(HttpRequest httpRequest);
    }
}
