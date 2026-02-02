<template>
  <view class="profile-home">
    <!-- 个人信息概览卡片 -->
    <view class="user-card" @tap="toPersonalInfo">
      <image class="avatar" :src="user.avatarUrl || '/static/default-avatar.png'" mode="aspectFill" />
      <view class="info">
        <view class="nickname">{{ user.nickname || '商户' }}</view>
        <view class="phone">{{ user.phone ? '手机号: ' + user.phone : '未绑定手机号' }}</view>
        <view class="level">
          <text class="badge">{{ user.subscriptionLevel || '免费版' }}</text>
          <text class="quota">剩余 {{ user.freeQuota || 0 }} 条</text>
        </view>
      </view>
      <text class="arrow">></text>
    </view>

    <!-- 功能入口 -->
    <view class="action-grid">
      <view class="action-item" @tap="toShopSetting">
        <view class="icon shop">🏪</view>
        <text>店铺信息</text>
      </view>
      <view class="action-item" @tap="toAiPreference">
        <view class="icon ai">🤖</view>
        <text>AI 偏好</text>
      </view>
      <view class="action-item" @tap="toAccount">
        <view class="icon security">🔒</view>
        <text>账号安全</text>
      </view>
      <view class="action-item" @tap="toSubscribe">
        <view class="icon subscribe">💎</view>
        <text>订阅管理</text>
      </view>
    </view>

    <!-- 其他 -->
    <view class="other-list">
      <view class="list-item" @tap="contactUs">
        <text>联系客服</text>
        <text class="arrow">></text>
      </view>
      <view class="list-item" @tap="logout">
        <text>退出登录</text>
        <text class="arrow">></text>
      </view>
      <view class="version">Synerixis v1.0.0 | © 2026</view>
    </view>
  </view>
</template>

<script>
const testbase = 'http://192.168.1.254:7092';

export default {
  data() {
    return {
      user: {
        nickname: '',
        avatarUrl: '',
        phone: '',
        freeQuota: 0,
        subscriptionLevel: '免费版'
      }
    };
  },

  onShow() {
    this.loadUser();
  },

  methods: {
    async loadUser() {
      const token = uni.getStorageSync('token');
      if (!token) return uni.navigateTo({ url: '/pages/login/login' });

      const res = await uni.request({
        url: `${testbase}/api/seller/profile`,
        header: { Authorization: `Bearer ${token}` }
      });

      if (res.statusCode === 200) {
        this.user = {
          nickname: res.data.nickname || '',
          avatarUrl: res.data.avatarUrl || '',
          phone: res.data.phone || '',
          freeQuota: res.data.freeQuota || 0,
          subscriptionLevel: res.data.subscriptionLevel || '免费版'
        };
      }
    },

    toPersonalInfo() {
      uni.navigateTo({ url: '/pages/profile/profile-info' });
    },

    toShopSetting() {
      uni.navigateTo({ url: '/pages/profile/shop-setting' });
    },

    toAiPreference() {
      uni.navigateTo({ url: '/pages/profile/ai-preference' });
    },

    toAccount() {
      uni.navigateTo({ url: '/pages/profile/account-security' });
    },

    toSubscribe() {
      uni.navigateTo({ url: '/pages/pay/subscribe' });
    },

    logout() {
      uni.showModal({
        title: '退出登录',
        content: '确定要退出吗？',
        success: res => {
          if (res.confirm) {
            uni.removeStorageSync('token');
            uni.removeStorageSync('sellerId');
            uni.switchTab({ url: '/pages/login/login' });
          }
        }
      });
    },

    contactUs() {
      uni.showModal({
        title: '联系客服',
        content: '微信：synerixis_support\n邮箱：support@synerixis.com',
        showCancel: false
      });
    }
  }
};
</script>

<style>
.profile-home { background: #f8f9fa; min-height: 100vh; padding-bottom: 40rpx; }
.user-card { background: white; margin: 20rpx 30rpx; padding: 40rpx; border-radius: 24rpx; display: flex; align-items: center; box-shadow: 0 8rpx 24rpx rgba(0,0,0,0.08); }
.avatar { width: 140rpx; height: 140rpx; border-radius: 50%; background: #eee; }
.info { margin-left: 30rpx; flex: 1; }
.nickname { font-size: 40rpx; font-weight: bold; }
.phone { font-size: 28rpx; color: #666; margin-top: 8rpx; }
.level { margin-top: 12rpx; display: flex; align-items: center; }
.badge { padding: 4rpx 16rpx; border-radius: 20rpx; font-size: 24rpx; background: #007aff; color: white; }
.quota { font-size: 28rpx; color: #666; margin-left: 16rpx; }
.action-grid { display: flex; flex-wrap: wrap; padding: 0 30rpx; }
.action-item { width: 25%; text-align: center; margin-bottom: 40rpx; }
.icon { width: 120rpx; height: 120rpx; background: #f0f4ff; border-radius: 50%; margin: 0 auto 20rpx; display: flex; align-items: center; justify-content: center; font-size: 60rpx; box-shadow: 0 4rpx 12rpx rgba(0,0,0,0.1); }
.shop { background: #fff7e6; }
.ai { background: #e6f7ff; }
.security { background: #f0f5ff; }
.subscribe { background: #fff1f0; }
.other-list { background: white; margin: 0 30rpx; border-radius: 24rpx; overflow: hidden; box-shadow: 0 8rpx 24rpx rgba(0,0,0,0.08); }
.list-item { padding: 36rpx 40rpx; display: flex; justify-content: space-between; font-size: 32rpx; border-bottom: 1rpx solid #eee; }
.list-item:last-child { border-bottom: none; }
.arrow { color: #999; }
.version { text-align: center; padding: 40rpx 0; color: #999; font-size: 28rpx; }
</style>