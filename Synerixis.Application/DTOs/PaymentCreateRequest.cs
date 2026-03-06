using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synerixis.Application.DTOs
{
    // 辅助 DTO（放在 Interfaces 或单独文件）
    public class PaymentCreateRequest
    {
        public string Channel { get; set; }          // 支付渠道：wechat / alipay
        public decimal Amount { get; set; }          // 金额（元）
        public string? Description { get; set; }      // 订单描述
        public string? OutTradeNo { get; set; }       // 商户订单号（可选，自生成）
        // 新增以下两个属性，解决当前报错
        public string NotifyUrl { get; set; }        // 异步通知地址（可选）
        public string? OpenId { get; set; }           // 微信支付专用：用户 OpenID（JSAPI/H5 需要）
        // 支付宝常用，可选添加
        public string ReturnUrl { get; set; }        // 前端同步返回地址（支付宝支付成功后跳转）
    }
}
