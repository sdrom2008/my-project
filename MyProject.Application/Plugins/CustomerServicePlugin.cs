using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace MyProject.Application.Plugins;

public class CustomerServicePlugin
{
    [KernelFunction("GetOrderStatus")]
    [Description("查询用户订单状态。需要订单号或手机号。如果没有订单号，返回提示。")]
    public string GetOrderStatus(
        [Description("订单号")] string orderId,
        [Description("用户手机号，可选")] string? phone = null)
    {
        // 模拟数据库查询（后期替换为真实 Repository）
        var statusMap = new Dictionary<string, string>
        {
            { "ORD123", "已发货，预计明天到达。快递单号：SF123456789" },
            { "ORD456", "已签收" },
            { "ORD789", "处理中" }
        };

        if (string.IsNullOrEmpty(orderId))
        {
            return "请提供订单号以查询状态。";
        }

        return statusMap.TryGetValue(orderId, out var status)
            ? $"订单 {orderId} 状态：{status}"
            : "未找到该订单，请检查订单号是否正确。";
    }
}