<template>
  <view class="chat-container">
    <view class="header">
      <text>智能店小二</text>
      <text class="seller-name">{{ nickname || '商家' }}</text>
    </view>

    <scroll-view class="messages" scroll-y :scroll-top="scrollTop">
      <view v-for="(msg, index) in messages" :key="index" :class="msg.isFromUser ? 'user-msg' : 'ai-msg'">
        <view v-if="msg.messageType === 'text' || !msg.messageType" class="text-bubble">
          {{ msg.content }}
        </view>

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

      <view v-if="loadingHistory" class="loading">
        <text>加载历史消息...</text>
      </view>
      <view v-if="sending" class="loading">
        <text>AI 思考中...</text>
      </view>
    </scroll-view>

    <view class="input-bar">
      <input 
        v-model="inputMessage" 
        placeholder="说点什么...（例如：帮我优化T恤详情）" 
        confirm-type="send" 
        @confirm="sendMessage" 
      />
      <button @tap="sendMessage" :disabled="sending || !inputMessage.trim()">发送</button>
    </view>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'

const messages = ref<any[]>([])
const inputMessage = ref('')
const sending = ref(false)
const loadingHistory = ref(false)
const scrollTop = ref(0)
const nickname = ref(uni.getStorageSync('nickname') || '商家')
const conversationId = ref<string>('')

const BASE_URL = 'https://127.0.0.1:7092'  // ← 替换成你的真实地址

onMounted(async () => {
  const savedId = uni.getStorageSync('currentConversationId')
  if (savedId) {
    conversationId.value = savedId
    console.log('恢复会话ID:', conversationId.value)
    await loadHistory()  // 加载历史消息
  }

  // 欢迎语（只在新建会话时显示）
  if (!savedId && messages.value.length === 0) {
    messages.value.push({
      isFromUser: false,
      messageType: 'text',
      content: `欢迎 ${nickname.value}！我是你的智能店小二，一句话告诉我需求，我来帮你优化商品、生成营销短语、做客服~`
    })
  }

  await nextTick()
  scrollTop.value = 999999
})

onLoad(options) {
  this.data.conversationId = options.conversationId || '';

  if (this.data.conversationId) {
    this.loadHistoryMessages();
  }
},
async loadHistoryMessages() {
  try {
    const res = await uni.request({
      url: `${BASE_URL}/api/chat/history`,
      method: 'GET',
      data: { conversationId: this.data.conversationId }
    });

    if (res.data.success) {
      const history = res.data.data.messages.map(msg => ({
        id: msg.timestamp,
        role: msg.isFromUser ? 'user' : 'assistant',
        type: msg.messageType,
        content: msg.content,
        data: msg.data
      }));

      this.setData({
        messages: history.reverse() // 倒序显示，最新在下
      });
      this.scrollToBottom();
    }
  } catch (e) {
    uni.showToast({ title: '加载历史失败', icon: 'none' });
  }
}

async sendMessage() {
  const content = this.data.inputValue.trim();
  if (!content) return;

  // 先渲染用户消息
  const userMsg = { role: 'user', type: 'text', content, id: Date.now() };
  this.data.messages.push(userMsg);
  this.setData({ messages: this.data.messages, inputValue: '' });
  this.scrollToBottom();

  try {
    const res = await uni.request({
      url: `${BASE_URL}/api/chat/message`,
      method: 'POST',
      data: {
        conversationId: this.data.conversationId,
        sellerId: wx.getStorageSync('sellerId'), // 从登录获取
        content
      }
    });

    if (res.data.success) {
      const aiMsgs = res.data.messages.filter(m => !m.isFromUser).map(msg => ({
        role: 'assistant',
        type: msg.messageType,
        content: msg.content,
        data: msg.data,
        id: Date.now() + Math.random()
      }));

      this.data.messages.push(...aiMsgs);
      this.setData({ messages: this.data.messages });
      this.scrollToBottom();

      // 更新 conversationId（如果首次创建）
      if (!this.data.conversationId && res.data.conversationId) {
        this.setData({ conversationId: res.data.conversationId });
      }
    } else {
      uni.showToast({ title: res.data.errorMessage || '发送失败', icon: 'none' });
    }
  } catch (e) {
    uni.showToast({ title: '网络错误', icon: 'none' });
  }
}

const loadHistory = async () => {
  if (!conversationId.value) return

  loadingHistory.value = true

  try {
    const token = uni.getStorageSync('token')
    if (!token) return uni.reLaunch({ url: '/pages/login/index' })

    const res = await uni.request({
      url: `${BASE_URL}/api/chat/conversation/${conversationId.value}`,
      header: { 'Authorization': `Bearer ${token}` }
    }) as any

    if (res.statusCode === 200) {
      const history = res.data.messages || []
      messages.value = history.map((m: any) => ({
        isFromUser: m.isFromUser,
        messageType: m.messageType,
        content: m.content,
        data: m.data
      }))
      console.log('加载历史消息成功:', messages.value.length, '条')
    } else {
      uni.showToast({ title: '加载历史失败', icon: 'error' })
    }
  } catch (err) {
    console.error('加载历史异常:', err)
    uni.showToast({ title: '网络错误', icon: 'error' })
  } finally {
    loadingHistory.value = false
    await nextTick()
    scrollTop.value = 999999
  }
}

watch(messages, async () => {
  await nextTick()
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

      if (!reply.success) {
        uni.showToast({ title: reply.errorMessage || 'AI 处理失败', icon: 'error' })
        return
      }

      // 追加新消息
      const newMessages = reply.messages || []
      messages.value.push(...newMessages.filter((m: any) => 
        !messages.value.some(existing => existing.content === m.content)
      ))

      // 更新会话ID
      if (reply.conversationId && reply.conversationId !== conversationId.value) {
        console.log('更新会话ID:', reply.conversationId)
        conversationId.value = reply.conversationId
        uni.setStorageSync('currentConversationId', reply.conversationId)
      }

      await nextTick()
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
    uni.showToast({ title: '网络错误，请检查地址', icon: 'error' })
  } finally {
    sending.value = false
  }
}
</script>

<style>
.chat-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
}

.header {
  padding: 30rpx;
  background: #07c160;
  color: white;
  text-align: center;
  font-size: 40rpx;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
}

.messages {
  flex: 1;
  padding: 160rpx 20rpx 160rpx 20rpx;
  background: #f8f8f8;
}

.user-msg {
  text-align: right;
  margin: 30rpx 0;
}

.ai-msg {
  text-align: left;
  margin: 30rpx 0;
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
  z-index: 100;
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

.loading {
  text-align: center;
  padding: 40rpx;
  color: #999;
  font-size: 28rpx;
}
</style>