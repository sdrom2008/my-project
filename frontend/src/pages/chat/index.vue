<template>
  <view class="chat-page">
    <!-- 顶部标题栏 -->
    <view class="header">
      <text>智能店小二</text>
      <text class="seller-name">{{ nickname || '商家' }}</text>
    </view>

    <!-- 消息列表（占满剩余空间） -->
    <scroll-view scroll-y class="messages" :scroll-top="scrollTop">
      <view v-for="(msg, index) in messages" :key="index" :class="msg.isFromUser ? 'user-msg' : 'ai-msg'">
        <!-- 普通文本 -->
        <view v-if="msg.messageType === 'text' || !msg.messageType" class="text-bubble">
          {{ msg.content }}
        </view>

        <!-- 商品优化结果卡片 -->
        <view v-if="msg.messageType === 'optimize_result'" class="optimize-card">
          <view class="card-header">商品优化完成！</view>

          <view class="section">
            <text class="label">优化标题：</text>
            <text class="value">{{ msg.data?.optimizedTitle || '暂无' }}</text>
          </view>

          <view class="section">
            <text class="label">优化描述：</text>
            <text class="value">{{ msg.data?.optimizedDescription || '暂无' }}</text>
          </view>

          <view class="section marketing">
            <text class="label">营销方案：</text>
            <view v-if="msg.data?.marketingPlan">
              <text class="sub-label">关键卖点：</text>
              <view v-for="(point, i) in msg.data.marketingPlan.keySellingPoints || []" :key="i" class="point">
                • {{ point }}
              </view>

              <text class="sub-label">短视频脚本：</text>
              <text class="value">{{ msg.data.marketingPlan.shortVideoScript || '暂无' }}</text>

              <text class="sub-label">种草文案：</text>
              <text class="value">{{ msg.data.marketingPlan.plantingText || '暂无' }}</text>

              <text class="sub-label">直播话术：</text>
              <text class="value">{{ msg.data.marketingPlan.liveScript || '暂无' }}</text>
            </view>
            <text v-else>暂无营销方案</text>
          </view>

          <view class="section images">
            <text class="label">图片优化建议：</text>
            <text v-for="(prompt, i) in msg.data?.imagePrompts || []" :key="i">
              {{ i + 1 }}. {{ prompt }}
            </text>
          </view>
        </view>
      </view>
    </scroll-view>

    <!-- 输入区（固定底部） -->
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
const conversationId = ref<string>('')

onMounted(() => {
  const savedId = uni.getStorageSync('currentConversationId')
  if (savedId) {
    conversationId.value = savedId
    console.log('加载会话ID:', conversationId.value)
  }

  messages.value.push({
    isFromUser: false,
    messageType: 'text',
    content: `欢迎 ${nickname.value}！我是你的智能店小二，一句话告诉我需求，我来帮你优化商品、生成营销短语、做客服~`
  })
})

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

    const BASE_URL = 'https://127.0.0.1:7092'  // 替换成你的地址

    console.log('发送消息，会话ID:', conversationId.value || '新建')

    const res = await uni.request({
      url: `${BASE_URL}/api/chat/send`,
      method: 'POST',
      header: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      data: {
        message: currentMsg,
        conversationId: conversationId.value || undefined
      }
    }) as any

    console.log('后端完整返回:', JSON.stringify(res.data, null, 2))

    if (res.statusCode === 200) {
      const reply = res.data

      // 追加所有新消息（去重）
      const newMessages = reply.messages?.filter((m: any) => 
        !messages.value.some(existing => existing.content === m.content && existing.isFromUser === m.isFromUser)
      ) || []

      messages.value.push(...newMessages)

      // 更新会话ID
      if (reply.conversationId && reply.conversationId !== conversationId.value) {
        console.log('更新会话ID:', reply.conversationId)
        conversationId.value = reply.conversationId
        uni.setStorageSync('currentConversationId', reply.conversationId)
      }

      scrollTop.value = 999999
    } else {
      uni.showToast({ title: res.data?.message || '发送失败', icon: 'error' })
    }
  } catch (err) {
    console.error('发送异常:', err)
    uni.showToast({ title: '网络错误', icon: 'error' })
  } finally {
    sending.value = false
  }
}
</script>

<style>
.chat-page {
  height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  padding: 20rpx;
  background: #07c160;
  color: white;
  text-align: center;
  font-size: 36rpx;
}

.messages {
  flex: 1;
  padding: 20rpx;
  background: #f8f8f8;
  overflow-y: auto;
}

.user-msg {
  text-align: right;
  margin: 20rpx 0;
}

.ai-msg {
  text-align: left;
  margin: 20rpx 0;
}

.text-bubble {
  display: inline-block;
  padding: 24rpx 32rpx;
  background: #fff;
  border-radius: 24rpx;
  max-width: 70%;
  box-shadow: 0 4rpx 12rpx rgba(0,0,0,0.1);
}

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

.input-bar {
  display: flex;
  padding: 20rpx;
  background: white;
  border-top: 1rpx solid #eee;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 1000;
}

input {
  flex: 1;
  padding: 20rpx 32rpx;
  background: #f0f0f0;
  border-radius: 50rpx;
  margin-right: 20rpx;
}

button {
  width: 160rpx;
  background: #07c160;
  color: white;
  border-radius: 50rpx;
}
</style>