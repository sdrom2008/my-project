<template>
  <view class="chat-page">
    <!-- 顶部标题栏 -->
    <view class="header">
      <text>智能店小二</text>
      <text class="seller-name">{{ nickname || '商家' }}</text>
    </view>

    <!-- 消息列表 -->
    <scroll-view scroll-y class="messages" :scroll-top="scrollTop">
      <view v-for="(msg, index) in messages" :key="index" :class="msg.isFromUser ? 'user-msg' : 'ai-msg'">
        <!-- 普通文本 -->
        <view v-if="msg.messageType === 'text' || !msg.messageType" class="text-bubble">
          {{ msg.content }}
        </view>

        <!-- 商品优化结果卡片（匹配后端大写字段） -->
        <view v-if="msg.messageType === 'optimize_result'" class="optimize-card">
          <view class="card-header">商品优化完成！</view>

          <view class="section">
            <text class="label">优化标题：</text>
            <text class="value">{{ msg.data?.OptimizedTitle || '暂无' }}</text>
          </view>

          <view class="section">
            <text class="label">优化描述：</text>
            <text class="value">{{ msg.data?.OptimizedDescription || '暂无' }}</text>
          </view>

          <view class="section marketing">
            <text class="label">营销方案：</text>
            <view v-if="msg.data?.MarketingPlan">
              <text class="sub-label">关键卖点：</text>
              <view v-for="(point, i) in msg.data.MarketingPlan.KeySellingPoints || []" :key="i" class="point">
                • {{ point }}
              </view>

              <text class="sub-label">广告短语：</text>
              <view v-for="(phrase, i) in msg.data.MarketingPlan.AdShortPhrases || []" :key="i" class="point">
                • {{ phrase }}
              </view>

              <text class="sub-label">社交文案：</text>
              <text class="value">{{ msg.data.MarketingPlan.SocialMediaCopy || '暂无' }}</text>

              <text class="sub-label">短视频脚本：</text>
              <text class="value">{{ msg.data.MarketingPlan.ShortVideoScript || '暂无' }}</text>

              <text class="sub-label">直播话术：</text>
              <text class="value">{{ msg.data.MarketingPlan.LiveScript || '暂无' }}</text>
            </view>
            <text v-else>暂无营销方案</text>
          </view>

          <view class="section images">
            <text class="label">图片优化建议：</text>
            <text v-for="(prompt, i) in msg.data?.ImagePrompts || []" :key="i">
              {{ i + 1 }}. {{ prompt }}
            </text>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 输入区 -->
    <view class="input-bar">
      <input v-model="inputMessage" placeholder="说点什么...（例如：帮我优化T恤详情）" />
      <button @click="sendMessage" :disabled="sending || !inputMessage.trim()">发送</button>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'

const messages = ref<any[]>([])
const inputMessage = ref('')
const sending = ref(false)
const scrollTop = ref(0)
const nickname = ref(uni.getStorageSync('nickname') || '商家')
const conversationId = ref<string>('')  // 统一用 string，默认空字符串

// 欢迎语 + 加载已保存的会话ID
onMounted(() => {
  const savedId = uni.getStorageSync('currentConversationId')
  if (savedId) {
    conversationId.value = savedId
    console.log('加载已保存会话ID:', conversationId.value)
  } else {
    console.log('无历史会话，将创建新会话')
  }

  if (messages.value.length === 0) {
    messages.value.push({
      isFromUser: false,
      messageType: 'text',
      content: `欢迎 ${nickname.value}！我是你的智能店小二，一句话告诉我需求，我来帮你优化商品、生成营销短语、做客服~`
    })
  }
})

// 自动滚动到底部
watch(messages, () => {
  scrollTop.value = 999999
}, { deep: true })

const sendMessage = async () => {
  if (!inputMessage.value.trim()) return

  const userMsg = {
    isFromUser: true,
    messageType: 'text',
    content: inputMessage.value
  }
  messages.value.push(userMsg)

  const currentMsg = inputMessage.value
  inputMessage.value = ''
  sending.value = true

  try {
    const token = uni.getStorageSync('token')
    if (!token) {
      uni.showToast({ title: '请先登录', icon: 'error' })
      uni.reLaunch({ url: '/pages/login/index' })
      return
    }

    const BASE_URL = 'https://127.0.0.1:7092'  // 替换成你的真实内网IP或ngrok地址

    console.log('发送消息，携带会话ID:', conversationId.value || '新建会话')

    const res = await uni.request({
      url: `${BASE_URL}/api/chat/send`,
	  timeout: 30000, // 设置超时时间为30秒
      method: 'POST',
      header: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      data: {
        message: currentMsg,
        conversationId: conversationId.value || undefined  // 传字符串或不传
      }
    }) as any

    console.log('后端完整返回:', JSON.stringify(res.data, null, 2))
	if (res.statusCode === 200) {
	  const reply = res.data
	  console.log('reply.conversationId:', reply.conversationId)
	  console.log('reply.messages 长度:', reply.messages?.length)
	  console.log('第一条 AI 回复 messageType:', reply.messages?.find(m => !m.isFromUser)?.messageType)
	  console.log('AI 回复 data:', JSON.stringify(reply.messages?.find(m => !m.isFromUser)?.data, null, 2))

	  // ... 原有追加消息代码
	}

    if (res.statusCode === 200) {
      const reply = res.data

      // 追加新消息（去重）
      const newMessages = reply.messages.filter((m: any) => 
        !messages.value.some(existing => 
          existing.content === m.content && existing.isFromUser === m.isFromUser
        )
      )
      messages.value.push(...newMessages)

      // 保存/更新会话ID（后端返回的一定是字符串格式的 Guid）
      if (reply.conversationId && reply.conversationId !== conversationId.value) {
        console.log('更新会话ID:', reply.conversationId)
        conversationId.value = reply.conversationId
        uni.setStorageSync('currentConversationId', reply.conversationId)
      }

      scrollTop.value = 999999
    } else {
      uni.showToast({ title: res.data?.message || '发送失败', icon: 'error' })
      if (res.statusCode === 401) {
        uni.removeStorageSync('token')
        uni.reLaunch({ url: '/pages/login/index' })
      }
    }
  } catch (err) {
    console.error('发送异常:', err)
    uni.showToast({ title: '网络错误，请检查地址或重试', icon: 'error' })
  } finally {
    sending.value = false
  }
}
</script>

<style>
/* 保持原样式，可微调卡片 */
.optimize-card {
  background: #f0fff0;
  border: 1rpx solid #90ee90;
  border-radius: 24rpx;
  padding: 32rpx;
  margin: 20rpx 0;
}
.card-header {
  font-size: 40rpx;
  font-weight: bold;
  color: #228b22;
  margin-bottom: 24rpx;
}
.section {
  margin: 24rpx 0;
}
.label, .sub-label {
  font-weight: bold;
  color: #333;
  display: block;
  margin-bottom: 8rpx;
}
.value, .point {
  color: #555;
}
.point {
  margin-left: 32rpx;
  display: block;
}
.input-bar input {
  background: #f0f0f0;
  border-radius: 50rpx;
  padding: 20rpx 32rpx;
}
</style>