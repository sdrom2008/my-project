<template>
  <view class="login-container">
    <image src="/static/logo.png" mode="widthFix" class="logo" />
    <view class="title">Synerixis - AI 智能伙伴</view>
    <view class="desc">一键接入，AI 帮你 24h 客服 & 增长</view>

    <!-- 微信一键登录（先授权用户信息，再手机号） -->
		<button 
		  class="wechat-btn" 
		  open-type="getPhoneNumber" 
		  @getphonenumber="handleGetPhoneNumber"
		  :loading="wechatLoading"
		>
		  <text>微信一键登录（获取手机号）</text>
		</button>

    <view class="divider">或</view>

    <!-- 手机号登录 / 注册 -->
    <view class="phone-section">
      <input 
        v-model="phone" 
        placeholder="请输入手机号" 
        type="tel" 
        maxlength="11" 
        class="input" 
        focus 
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
        :disabled="loginLoading"
      >
        登录 / 注册
      </button>
    </view>

    <view class="tip">登录即同意《用户协议》和《隐私政策》</view>
  </view>
</template>
<script>
const testbase = 'http://localhost:7092';

export default {
  data() {
    return {
      phone: '',
      code: '',
      countdown: 0,
      wechatLoading: false,
      sendCodeLoading: false,
      loginLoading: false
    };
  },

  methods: {
    // 微信授权用户信息（第一步）
  async handleGetPhoneNumber(e) {
    this.wechatLoading = true;

    const detail = e.detail;
    console.log('getPhoneNumber 返回：', detail);  // 调试用

    if (detail.errMsg && detail.errMsg.includes('deny')) {
      uni.showToast({ title: '用户拒绝授权', icon: 'none' });
      this.wechatLoading = false;
      return;
    }

    if (!detail.encryptedData || !detail.iv) {
      uni.showToast({ title: '获取手机号失败，请重试', icon: 'none' });
      this.wechatLoading = false;
      return;
    }

    try {
      // 获取 code（可选，但推荐用新 code 校验）
      const loginRes = await uni.login({ provider: 'weixin' });
      const code = loginRes.code;  // 或用 detail.code（新版有）

      const res = await uni.request({
        url: `${testbase}/api/auth/decrypt-phone`,
        method: 'POST',
        data: {
          code,  // 可以传，也可以不传（后端可跳过 jscode2session 如果信任）
          encryptedData: detail.encryptedData,
          iv: detail.iv
        }
      });

      console.log('后端响应：', res);

      if (res.statusCode === 200 && res.data.token) {
        uni.setStorageSync('token', res.data.token);
        uni.setStorageSync('sellerId', res.data.sellerId);
        uni.showToast({ title: '登录成功', icon: 'success' });
        uni.switchTab({ url: '/pages/conversations/conversations' });
      } else {
        uni.showToast({ title: res.data?.message || '登录失败', icon: 'none' });
      }
    } catch (err) {
      console.error('登录异常：', err);
      uni.showToast({ title: '网络错误，请检查控制台', icon: 'none' });
    } finally {
      this.wechatLoading = false;
    }
  },

    // 发送验证码
    async sendCode() {
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

    // 手机号登录 / 注册
    async handlePhoneLogin() {
      if (!this.phone || !this.code) {
        uni.showToast({ title: '请输入完整信息', icon: 'none' });
        return;
      }

  console.log('授权成功，encryptedData:', e.detail.encryptedData);
      this.loginLoading = true;

      try {
        const res = await uni.request({
          url: `${testbase}/api/auth/phone-login`,
          method: 'POST',
          data: { phone: this.phone, code: this.code }
        });

    console.log('后端响应:', res);

        if (res.statusCode === 200 && res.data.token) {
          uni.setStorageSync('token', res.data.token);
          uni.setStorageSync('sellerId', res.data.sellerId);

          uni.showToast({ title: '登录成功', icon: 'success' });
          uni.switchTab({ url: '/pages/conversations/conversations' });
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
.login-container { height: 100vh; display: flex; flex-direction: column; align-items: center; justify-content: center; background: linear-gradient(to bottom, #f0f4ff, #fff); padding: 0 60rpx; }
.logo { width: 300rpx; margin-bottom: 40rpx; }
.title { font-size: 52rpx; font-weight: bold; margin-bottom: 20rpx; }
.desc { font-size: 32rpx; color: #666; margin-bottom: 80rpx; text-align: center; }
.wechat-btn { width: 100%; height: 100rpx; line-height: 100rpx; background: #07c160; color: white; border-radius: 50rpx; font-size: 36rpx; margin: 40rpx 0; }
.divider { color: #999; margin: 40rpx 0; font-size: 28rpx; }
.phone-section { width: 100%; }
.input { width: 100%; height: 100rpx; padding: 0 30rpx; background: white; border-radius: 50rpx; margin: 20rpx 0; border: 1rpx solid #eee; box-sizing: border-box; font-size: 32rpx; }
.code-row { display: flex; align-items: center; margin: 20rpx 0; }
.code-btn { width: 280rpx; height: 100rpx; line-height: 100rpx; background: #007aff; color: white; border-radius: 50rpx; font-size: 32rpx; margin-left: 20rpx; }
.login-btn { width: 100%; height: 100rpx; line-height: 100rpx; background: #007aff; color: white; border-radius: 50rpx; font-size: 36rpx; margin-top: 40rpx; }
.tip { font-size: 28rpx; color: #999; margin-top: 40rpx; text-align: center; }
</style>