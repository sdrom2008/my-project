// 统一响应类型（建议写在 types/chat.d.ts）
interface AgentResponse {
  conversationId: string
  replyText: string
  messageType: 'text' | 'optimize_result' | 'marketing_copy' | 'image_prompt' | 'order_query' | 'error'
  success: boolean
  errorMessage?: string
  data?: any  // 根据 messageType 动态解析
}

// 示例：渲染优化结果
if (msg.messageType === 'optimize_result') {
  const data = msg.data as {
    optimizedTitle: string
    optimizedDescription: string
    marketingPlan: {
      shortVideoScript: string
      plantingText: string
      liveScript: string
      keySellingPoints: string[]
    }
    imagePrompts: string[]
  }

  // 渲染卡片
}