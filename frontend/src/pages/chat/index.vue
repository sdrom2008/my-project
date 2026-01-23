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

        <!-- 商品优化结果卡片 -->
        <view v-if="msg.messageType === 'optimize_result'" class="optimize-card">
          <view class="card-header">商品优化完成！</view>
          <view class="section">
            <text class="label">优化标题：</text>
            <text class="value">{{ msg.data?.optimized_title || '暂无' }}</text>
          </view>
          <view class="section">
            <text class="label">优化描述：</text>
            <text class="value">{{ msg.data?.optimized_description || '暂无' }}</text>
          </view>
          <view class="section marketing">
            <text class="label">营销方案：</text>
            <view v-if="msg.data?.marketing_plan">
              <text>短视频脚本：{{ msg.data.marketing_plan.short_video_script || '暂无' }}</text>
              <text>种草文案：{{ msg.data.marketing_plan.planting_text || '暂无' }}</text>
              <text>直播话术：{{ msg.data.marketing_plan.live_script || '暂无' }}</text>
              <view class="points">
                <text>关键卖点：</text>
                <text v-for="(point, i) in msg.data.marketing_plan.key_selling_points" :key="i">- {{ point }}</text>
              </view>
            </view>
          </view>
          <view class="section images">
            <text class="label">图片优化建议：</text>
            <text v-for="(prompt, i) in msg.data?.image_prompts || []" :key="i">{{ i + 1 }}. {{ prompt }}</text>
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
import { ref, onMounted } from 'vue'

const messages = ref<any[]>([])
const inputMessage = ref('')
const sending = ref(false)
const scrollTop = ref(0)
const nickname = ref(uni.getStorageSync('nickname') || '商家')

// 加载历史消息（可选，后续完善）
onMounted(() => {
  // 可以从 storage 或后端拉历史
  messages.value.push({
    isFromUser: false,
    messageType: 'text',
    content: '欢迎使用 NexusAI 智能店小二！一句话告诉我需求，我来帮你优化商品、写营销方案、做客服~'
  })
})

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

    const BASE_URL = 'https://127.0.0.1:7092'  // 必须公网地址

    const res = await uni.request({
      url: `${BASE_URL}/api/chat/send`,
      method: 'POST',
      header: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      data: {
        message: currentMsg
        // conversationId: 当前会话ID（后续支持多会话时传）
      }
    }) as any

    if (res.statusCode === 200) {
      // 追加所有新消息（后端返回整个历史或增量）
      messages.value.push(...res.data.messages.filter((m: any) => !messages.value.some(existing => existing.content === m.content)))
      scrollTop.value = 999999  // 滚动到底部
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
.seller-name {
  font-size: 28rpx;
  margin-left: 20rpx;
}
.messages {
  flex: 1;
  padding: 20rpx;
  background: #f8f8f8;
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
  background: white;
  border-radius: 24rpx;
  padding: 32rpx;
  margin: 20rpx 0;
  box-shadow: 0 8rpx 24rpx rgba(0,0,0,0.15);
}
.card-header {
  font-size: 40rpx;
  font-weight: bold;
  color: #07c160;
  margin-bottom: 24rpx;
}
.section {
  margin: 24rpx 0;
}
.label {
  font-weight: bold;
  color: #333;
  display: block;
  margin-bottom: 8rpx;
}
.value {
  color: #666;
}
.input-bar {
  display: flex;
  padding: 20rpx;
  background: white;
  border-top: 1rpx solid #eee;
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