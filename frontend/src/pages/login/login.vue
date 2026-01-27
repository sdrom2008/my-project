<!-- pages/login/login.vue -->
<template>
  <view class="login-container">
    <image src="/static/logo.png" mode="widthFix" class="logo" />
    <view class="title">Synerixis - AI 智能伙伴</view>
    <view class="desc">一键接入，AI 帮你 24h 客服 & 增长</view>

    <button class="login-btn" open-type="getUserInfo" @getuserinfo="handleLogin">
      <text>微信一键登录</text>
    </button>

    <view class="tip">登录即同意《用户协议》和《隐私政策》</view>
  </view>
</template>

<script>
export default {
  methods: {
    async handleLogin(e) {
      if (!e.detail.userInfo) {
        uni.showToast({ title: '授权失败，请重试', icon: 'none' });
        return;
      }

      try {
        // 1. 获取 code
        const { code } = await uni.login({ provider: 'weixin' });

        // 2. 调用后端微信登录接口
        const res = await uni.request({
          url: `${this.$BASE_URL}/api/auth/wechat`,
          method: 'POST',
          data: { code }
        });

        if (res.data && res.data.token) {
          uni.setStorageSync('token', res.data.token);
          uni.setStorageSync('sellerId', res.data.sellerId);

          uni.showToast({ title: '登录成功', icon: 'success' });
          uni.switchTab({ url: '/pages/conversations/conversations' });
        } else {
          uni.showToast({ title: res.data?.message || '登录失败', icon: 'none' });
        }
      } catch (err) {
        uni.showToast({ title: '网络错误', icon: 'none' });
      }
    }
  }
};
</script>

<style>
.login-container { height: 100vh; display: flex; flex-direction: column; align-items: center; justify-content: center; background: linear-gradient(to bottom, #f0f4ff, #fff); }
.logo { width: 300rpx; margin-bottom: 40rpx; }
.title { font-size: 52rpx; font-weight: bold; margin-bottom: 20rpx; }
.desc { font-size: 32rpx; color: #666; margin-bottom: 80rpx; text-align: center; }
.login-btn { width: 600rpx; height: 100rpx; line-height: 100rpx; background: #07c160; color: white; border-radius: 50rpx; font-size: 36rpx; margin-top: 80rpx; }
.tip { font-size: 28rpx; color: #999; margin-top: 40rpx; }
</style>