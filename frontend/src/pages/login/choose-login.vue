<template>
  <view class="choose-login">
    <!-- 红圈结构完全不变（和启动页一样，三行） -->
    <view class="brand">
      <image src="/static/logo.png" mode="widthFix" class="logo" />
      <br/>
      <text class="title">Synerixis</text>
      <br/>
      <text class="slogan">AI 智能伙伴</text>
    </view>

    <!-- 按钮区 + 协议一起出现（协议紧跟按钮下方） -->
    <view class="options-with-protocol">
      <button class="option-btn wechat" hover-class="btn-hover" @tap="loginWechat">
        <text>微信一键登录</text>
        <text class="tag">推荐</text>
      </button>

      <button class="option-btn phone" hover-class="btn-hover" @tap="loginPhone">
        <text>手机号验证码登录</text>
      </button>

      <!-- 协议跟按钮一起出现，在按钮正下方 -->
      <view class="protocol">
        <checkbox size="24" :checked="agree" @change="toggleAgree" color="#3b82f6" />
        <text>我已阅读并同意</text>
        <text class="link" @tap="openUserProtocol">《用户协议》</text>
        <text>和</text>
        <text class="link" @tap="openPrivacy">《隐私政策》</text>
      </view>
    </view>
  </view>
</template>

<script>
export default {
  data() {
    return {
      agree: true  // 默认勾选
    };
  },

  methods: {
    toggleAgree(e) {
      this.agree = e.detail.value;
    },

    openUserProtocol() {
      uni.navigateTo({ url: '/pages/protocol/user' });
    },

    openPrivacy() {
      uni.navigateTo({ url: '/pages/protocol/privacy' });
    },

    loginWechat() {
      if (!this.agree) return uni.showToast({ title: '请同意协议', icon: 'none' });
      uni.login({ /* 你的微信登录逻辑 */ });
    },

    loginPhone() {
      if (!this.agree) return uni.showToast({ title: '请同意协议', icon: 'none' });
      uni.navigateTo({ url: '/pages/login/login' });
    }
  }
};
</script>

<style>
.choose-login {
  height: 100vh;
  background: #0a0e1a;
  padding-top: 140rpx;  /* logo 上方留白（上浮后偏上） */
  padding-bottom: 80rpx;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
}

.brand {
  text-align: center;
  opacity: 0;
  transform: translateY(180rpx);  /* 从启动页位置向下偏移180rpx开始上浮 */
  animation: brandFloat 1.2s ease-out forwards;
}

.logo {
  width: 220rpx;
  height: 220rpx;
  margin-bottom: 0;
}

.title {
  font-size: 64rpx;
  font-weight: bold;
  color: #3b82f6;
  margin: 0;
}

.slogan {
  font-size: 32rpx;
  color: #64748b;
  margin: 0;
}

/* 按钮 + 协议一起出现 */
.options-with-protocol {
  width: 80%;
  margin-top: 240rpx;  /* 整体往下沉，上面留白多 */
  opacity: 0;
  animation: fadeIn 0.8s ease-out 1.2s forwards;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.option-btn {
  width: 100%;
  height: 88rpx;
  line-height: 88rpx;
  border-radius: 44rpx;
  font-size: 32rpx;
  margin-bottom: 40rpx;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 6rpx 24rpx rgba(0,0,0,0.4);
  transition: all 0.25s;
  position: relative;
}

.option-btn:hover {
  transform: scale(1.04);
}

.wechat {
  background: linear-gradient(90deg, #3b82f6, #2563eb);
  color: white;
}

.phone {
  background: linear-gradient(90deg, #60a5fa, #3b82f6);
  color: white;
}

.tag {
  position: absolute;
  top: 6rpx;
  right: 16rpx;
  background: #60a5fa;
  color: white;
  font-size: 20rpx;
  padding: 2rpx 12rpx;
  border-radius: 20rpx;
}

.protocol {
  margin-top: 40rpx;  /* 紧跟按钮下方，跟按钮一起出现 */
  font-size: 24rpx;
  color: #94a3b8;
  text-align: center;
  width: 100%;
}

/* 上浮动画 */
@keyframes brandFloat {
  0% { opacity: 0; transform: translateY(180rpx); }
  100% { opacity: 1; transform: translateY(0); }
}

/* 按钮 + 协议一起淡入 */
@keyframes fadeIn {
  0% { opacity: 0; transform: translateY(40rpx); }
  100% { opacity: 1; transform: translateY(0); }
}
</style>