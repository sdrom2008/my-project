<!-- pages/conversations/index.vue -->
<template>
  <view class="container">
    <view class="header">
      <text>我的会话</text>
    </view>

    <scroll-view scroll-y class="list">
      <view class="conversation-item" v-for="item in conversations" :key="item.id" @tap="goToChat(item.id)">
        <view class="title">{{ item.title || '新对话' }}</view>
        <view class="last-message">{{ item.lastMessage || '暂无消息' }}</view>
        <view class="time">{{ item.lastActiveAt }}</view>
      </view>

      <view v-if="conversations.length === 0" class="empty">
        暂无会话，快去发起聊天吧～
      </view>
    </scroll-view>

    <view class="new-chat" @tap="startNewChat">
      <text>+ 新对话</text>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      conversations: []
    };
  },

  onShow() {
    this.loadConversations();
  },

  methods: {
    async loadConversations() {
      try {
        const res = await uni.request({
          url: `${this.$BASE_URL}/api/chat/conversations`,
          header: {
            Authorization: `Bearer ${uni.getStorageSync('token')}`
          }
        });

        if (res.data && res.data.length) {
          this.conversations = res.data.map(item => ({
            id: item.id,
            title: item.title,
            lastMessage: item.lastMessage,
            lastActiveAt: item.lastActiveAt
          }));
        }
      } catch (e) {
        uni.showToast({ title: '加载会话失败', icon: 'none' });
      }
    },

    goToChat(conversationId) {
      uni.navigateTo({
        url: `/pages/chat/chat?conversationId=${conversationId}`
      });
    },

    startNewChat() {
	  uni.removeStorageSync('conversationId');
      uni.navigateTo({
        url: '/pages/chat/chat'
      });
    }
  }
};
</script>

<style>
.container { height: 100vh; display: flex; flex-direction: column; }
.header { padding: 20rpx; font-size: 36rpx; text-align: center; background: #f8f8f8; }
.list { flex: 1; }
.conversation-item { padding: 30rpx; border-bottom: 1rpx solid #eee; }
.title { font-size: 32rpx; font-weight: bold; }
.last-message { font-size: 28rpx; color: #666; margin-top: 10rpx; }
.time { font-size: 24rpx; color: #999; margin-top: 10rpx; text-align: right; }
.new-chat { position: fixed; bottom: 40rpx; right: 40rpx; width: 120rpx; height: 120rpx; background: #07c160; color: white; border-radius: 60rpx; text-align: center; line-height: 120rpx; font-size: 48rpx; box-shadow: 0 4rpx 12rpx rgba(0,0,0,0.2); }
.empty { text-align: center; padding: 200rpx; color: #999; }
</style>