<template>
  <view class="splash">
    <image src="/static/logo.png" mode="widthFix" class="logo" />
    <text class="title">Synerixis</text>
    <text class="slogan">AI 智能伙伴</text>
  </view>
</template>

<script>
export default {
  onLoad() {
    setTimeout(() => {
      uni.reLaunch({
        url: '/pages/login/choose-login'  // 跳转到选择页
      });
    }, 4000);  // 你的 4秒动画
  }
}
</script>

<style>
.splash {
  height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f172a, #1e293b);
}

.logo {
  width: 200rpx;
  height: 200rpx;
  animation: pulse 2s infinite;
}

.title {
  font-size: 64rpx;
  font-weight: bold;
  color: #60a5fa;
  margin-top: 40rpx;
}

.slogan {
  font-size: 32rpx;
  color: #94a3b8;
  margin-top: 20rpx;
}

@keyframes pulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}
</style>