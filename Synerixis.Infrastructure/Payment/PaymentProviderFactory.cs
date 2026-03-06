using Microsoft.Extensions.DependencyInjection;
using Synerixis.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Infrastructure.Payment
{
    public class PaymentProviderFactory : IPaymentProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentProvider GetProvider(string channel)
        {
            return channel.ToLowerInvariant() switch
            {
                "wechat" => _serviceProvider.GetService<WechatPaymentProvider>(),
                "alipay" => _serviceProvider.GetService<AlipayPaymentProvider>(),
                _ => null
            };
        }
    }
}
