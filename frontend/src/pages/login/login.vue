<template>
  <view class="phone-login">
    <view class="header">
      <image src="/static/logo.png" mode="widthFix" class="logo" /><br/>
      <text class="title">手机号登录</text><br/>
      <text class="back" @tap="backToChoose">返回</text>
    </view>

    <view class="form">
      <input 
        v-model="phone" 
        placeholder="+86 请输入手机号" 
        type="tel" 
        maxlength="11" 
        class="input" 
      />

      <view class="code-row">
        <input 
          v-model="code" 
          placeholder="验证码" 
          type="number" 
          maxlength="6" 
          class="input" 
        />
        <button 
          class="code-btn" 
          :disabled="countdown > 0 || sendCodeLoading" 
          @tap="sendCode"
          :loading="sendCodeLoading"
        >
          {{ countdown > 0 ? countdown + '秒' : '获取验证码' }}
        </button>
      </view>

      <button 
        class="login-btn" 
        @tap="handlePhoneLogin" 
        :loading="loginLoading"
        :disabled="loginLoading || !agree"
      >
        登录 / 注册
      </button>
    </view>

    <view class="protocol">
      <checkbox size="22" :checked="agree" @change="toggleAgree" color="#22c55e" />
      <text>同意《用户协议》和《隐私政策》</text>
    </view>
  </view>
</template>

<script>
const testbase = 'http://192.168.1.254:7092';

export default {
  data() {
    return {
      phone: '',
      code: '',
      countdown: 0,
      sendCodeLoading: false,
      loginLoading: false,
      agree: true
    };
  },

  methods: {
    toggleAgree(e) {
      this.agree = e.detail.value;
    },

    backToChoose() {
      uni.navigateBack();
    },

    async sendCode() {
      if (!this.agree) {
        uni.showToast({ title: '请先同意协议', icon: 'none' });
        return;
      }

      if (!this.phone || this.phone.length !== 11) {
        uni.showToast({ title: '手机号格式错误', icon: 'none' });
        return;
      }

      this.sendCodeLoading = true;

      try {
        const res = await uni.request({
          url: `${testbase}/api/auth/send-code`,
          method: 'POST',
          data: { phone: this.phone }
        });

        if (res.statusCode === 200) {
          uni.showToast({ title: '验证码已发送', icon: 'success' });
          this.countdown = 60;
          const timer = setInterval(() => {
            this.countdown--;
            if (this.countdown <= 0) clearInterval(timer);
          }, 1000);
        } else {
          uni.showToast({ title: res.data?.message || '发送失败', icon: 'none' });
        }
      } catch (err) {
        uni.showToast({ title: '网络错误', icon: 'none' });
      } finally {
        this.sendCodeLoading = false;
      }
    },

    async handlePhoneLogin() {
      if (!this.agree) {
        uni.showToast({ title: '请先同意协议', icon: 'none' });
        return;
      }

      if (!this.phone || !this.code) {
        uni.showToast({ title: '请输入完整信息', icon: 'none' });
        return;
      }

      this.loginLoading = true;

      try {
        const res = await uni.request({
          url: `${testbase}/api/auth/phone-login`,
          method: 'POST',
          data: { phone: this.phone, code: this.code }
        });

        if (res.statusCode === 200 && res.data?.token) {
          uni.setStorageSync('token', res.data.token);
          uni.setStorageSync('sellerId', res.data.sellerId);
          uni.setStorageSync('openId', res.data.openId);
          uni.showToast({ title: '登录成功', icon: 'success' });
          uni.switchTab({ url: '/pages/dashboard/dashboard' });
        } else {
          uni.showToast({ title: res.data?.message || '登录失败', icon: 'none' });
        }
      } catch (err) {
        uni.showToast({ title: '网络错误', icon: 'none' });
      } finally {
        this.loginLoading = false;
      }
    }
  }
};
</script>

<style>
.phone-login {
  height: 100vh;
  background: #0a0e1a;
  padding: 120rpx 40rpx;
}

.header {
  text-align: center;
  margin-bottom: 80rpx;
  position: relative;
}

.logo {
  width: 160rpx;
  height: 160rpx;
}

.title {
  font-size: 64rpx;
  font-weight: bold;
  color: #22c55e;
  margin-top: 40rpx;
}

.back {
  position: absolute;
  top: 40rpx;
  left: 40rpx;
  font-size: 32rpx;
  color: #60a5fa;
}

.form {
  margin-top: 60rpx;
}

.input {
  background: #1e293b;
  border-radius: 24rpx;
  padding: 0 32rpx;
  height: 100rpx;
  line-height: 100rpx;
  font-size: 36rpx;
  color: #e2e8f0;
  margin-bottom: 32rpx;
}

.code-row {
  display: flex;
  align-items: center;
  margin-bottom: 40rpx;
}

.code-input {
  flex: 1;
  background: #1e293b;
  border-radius: 24rpx;
  padding: 0 32rpx;
  height: 100rpx;
  line-height: 100rpx;
  font-size: 36rpx;
  color: #e2e8f0;
}

.code-btn {
  width: 280rpx;
  height: 100rpx;
  line-height: 100rpx;
  background: #3b82f6;
  color: white;
  border-radius: 50rpx;
  font-size: 30rpx;
  margin-left: 20rpx;
}

.login-btn {
  width: 100%;
  height: 100rpx;
  line-height: 100rpx;
  background: linear-gradient(90deg, #3b82f6, #2563eb);
  color: white;
  border-radius: 50rpx;
  font-size: 36rpx;
  box-shadow: 0 8rpx 32rpx rgba(59, 130, 246, 0.3);
}

.protocol {
  margin-top: 40rpx;
  font-size: 24rpx;
  color: #94a3b8;
  text-align: center;
}
</style>