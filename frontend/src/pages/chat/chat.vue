<template>
  <view class="chat-page">
    <scroll-view scroll-y class="msg-list" :scroll-into-view="scrollId">
      <view class="msg" v-for="(m, i) in messages" :key="m.id || i" :class="m.role">
        <view class="bubble">
          <view v-if="m.type === 'text'" class="text">{{ m.content }}</view>
          <view v-else-if="m.type === 'product_opt'" class="card">
            <text class="card-title">商品优化建议</text>
            <text>{{ m.content }}</text>
            <!-- 加更多结构化展示 -->
          </view>
          <view v-else class="text">{{ m.content }}</view>
        </view>
      </view>
    </scroll-view>

    <view class="input-area">
      <input v-model="inputText" placeholder="说点什么..." confirm-type="send" @confirm="send" />
      <button @tap="send" :loading="sending">发送</button>
    </view>
  </view>
</template>

<script>
	
	     const testbase = 'http://192.168.1.254:7092';
		 
export default {
  data() {
    return {
      id: '',
      messages: [],
      inputText: '',
      sending: false,
      scrollId: ''
    };
  },
  
  
  openChat(id) {
		  console.log('点击会话，ID：', id);  // 先打印看 id 是否有值

		  if (!id) {
			uni.showToast({ title: '会话ID为空', icon: 'none' });
			return;
		  }

		  uni.navigateTo({
			url: `/pages/chat/chat?conversationId=${id}`,  // 必须用 conversationId=xxx
			success: () => console.log('跳转成功'),
			fail: (err) => console.error('跳转失败：', err)
		  });
},

  onLoad(options) {
	  
		  console.log('聊天页 onLoad 参数：', options);

		  // 兼容两种参数名
		  this.id = options.conversationId || options.id || '';

		  console.log('当前 conversationId：', this.id);

		  if (this.id) {
			console.log('开始加载历史，会话ID：', this.id);
			this.loadHistory();
		  } else {
			console.log('新建会话，无 ID');
		  }

		  this.scrollToBottom();
  
  },

  methods: {
    async loadHistory() {
      const token = uni.getStorageSync('token');
      try {
        const res = await uni.request({
          url: testbase+`/api/chat/conversation/${this.id}`,
          header: { Authorization: `Bearer ${token}` }
        });

        if (res.statusCode === 200 && res.data.messages) {
          this.messages = res.data.messages.map(m => ({
            role: m.isFromUser ? 'user' : 'assistant',
            type: m.messageType,
            content: m.content,
            id: Date.now() + Math.random()
          }));
          this.scrollToBottom();
        }
      } catch (e) {
        uni.showToast({ title: '加载历史失败', icon: 'none' });
      }
    },

 async send() {
   const text = this.inputText.trim();
   if (!text) return;
 
   // 先显示用户消息
   const userMsg = { role: 'user', type: 'text', content: text, id: Date.now() };
   this.messages.push(userMsg);
   this.inputText = '';
   this.scrollToBottom();
 
   try {
     console.log('发送请求：', {
       conversationId: this.id || null,
       message: text
     });
	 console.log('发送的数据：', JSON.stringify({
	   ConversationId: this.conversationId,
	   Message: this.inputMessage
	 }, null, 2));
	 console.log('使用的 token：', uni.getStorageSync('token'));
 
     const res = await uni.request({
       url: testbase+`/api/chat/send`,
       method: 'POST',
       header: {
         'Content-Type': 'application/json',
         'Authorization': `Bearer ${uni.getStorageSync('token')}`
       },
       data: {
         //ConversationId: this.id || null,  // null 表示新建
		 ConversationId:  '00000000-0000-0000-0000-000000000000',
         Message: text
       }
     });
 
     console.log('发送响应：', res);
 
     if (res.statusCode === 200 && res.data) {
       // 更新 conversationId（如果是新建，后端会返回）
       if (!this.id && res.data.conversationId) {
         this.id = res.data.conversationId;
         console.log('新建会话成功，新 ID：', this.id);
       }
 
       // 追加 AI 回复
       const aiMsgs = res.data.messages
         .filter(m => !m.isFromUser)
         .map(m => ({
           role: 'assistant',
           type: m.messageType || 'text',
           content: m.content,
           id: Date.now() + Math.random()
         }));
 
       this.messages.push(...aiMsgs);
       this.scrollToBottom();
     } else {
       uni.showToast({ title: res.data?.message || '发送失败', icon: 'none' });
     }
   } catch (e) {
     console.error('发送失败：', e);
     uni.showToast({ title: e.errMsg || '网络错误', icon: 'none' });
   }
 },

    scrollToBottom() {
      this.$nextTick(() => {
        this.scrollId = 'msg-' + (this.messages.length - 1);
      });
    }
  }
};
</script>

<style>
.chat-page { height: 100vh; display: flex; flex-direction: column; }
.msg-list { flex: 1; padding: 20rpx; background: #f8f8f8; }
.msg { margin: 20rpx 0; display: flex; }
.user { justify-content: flex-end; }
.bubble { max-width: 70%; padding: 20rpx; border-radius: 20rpx; }
.user .bubble { background: #07c160; color: white; border-bottom-right-radius: 0; }
.assistant .bubble { background: #fff; border-bottom-left-radius: 0; box-shadow: 0 2rpx 8rpx rgba(0,0,0,0.1); }
.text { font-size: 32rpx; line-height: 48rpx; }
.card { background: #e6f7ff; padding: 30rpx; border-radius: 20rpx; border: 1rpx solid #91d5ff; }
.card-title { font-weight: bold; font-size: 36rpx; display: block; margin-bottom: 20rpx; }
.input-area { display: flex; padding: 20rpx; background: #fff; border-top: 1rpx solid #eee; }
input { flex: 1; border: 1rpx solid #ddd; border-radius: 40rpx; padding: 20rpx 30rpx; margin-right: 20rpx; }
button { background: #07c160; color: white; border-radius: 40rpx; }
</style>