<template>
  <view class="subscribe-page">
    <view class="header">
      <text class="title">升级订阅，畅享无限使用</text>
    </view>

    <view class="benefit-list">
      <view class="benefit-item">+1000 条额外额度</view>
      <view class="benefit-item">30 天无限智能客服 & 商品优化</view>
      <view class="benefit-item">优先使用高级营销模板</view>
      <view class="benefit-item">专属 AI 偏好记忆</view>
    </view>

    <view class="price-card">
      <view class="price">¥99</view>
      <view class="period">/月</view>
      <view class="original">原价 ¥199</view>
    </view>

    <!-- 新增：支付方式选择 -->
    <view class="payment-method">
      <text class="method-title">支付方式</text>
      <view class="method-list">
        <view 
          class="method-item" 
          :class="{ active: selectedChannel === 'wechat' }" 
          @tap="selectChannel('wechat')"
        >
          <image src="/static/wechat-pay.png" class="method-icon" mode="aspectFit"></image>
          <text>微信支付</text>
        </view>
        <view 
          class="method-item" 
          :class="{ active: selectedChannel === 'alipay' }" 
          @tap="selectChannel('alipay')"
        >
          <image src="/static/alipay.png" class="method-icon" mode="aspectFit"></image>
          <text>支付宝支付</text>
        </view>
      </view>
    </view>

    <button 
      class="pay-btn" 
      :loading="paying" 
      :disabled="paying"
      @tap="createOrder"
    >
      立即支付 99 元
    </button>

    <view class="tip">
      支付后立即生效，支持微信/支付宝支付\n额度可累积，随时查看
    </view>
  </view>
</template>

<script>
const testbase = 'http://192.168.1.254:7092';

export default {
  data() {
    return {
      paying: false,
      selectedChannel: 'wechat'  // 默认微信
    };
  },

  methods: {
    selectChannel(channel) {
      this.selectedChannel = channel;
    },

    async createOrder() {
      try {
        this.paying = true;
        const token = uni.getStorageSync('token');
        if (!token) {
          uni.showToast({ title: '请先登录', icon: 'none' });
          return;
        }

        const res = await uni.request({
          url: `${testbase}/api/pay/create`,  // 注意：已改为正确的 /create（之前是 create-order 导致 404）
          method: 'POST',
          header: { 
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          data: {
            channel: this.selectedChannel,  // 'wechat' 或 'alipay'
            amount: 99,
            description: '升级订阅 99元/月'
            // 如果需要 openId，可从 storage 取或后端从 seller 取
          }
        });

        this.paying = false;

        if (res.statusCode !== 200 || !res.data.success) {
          uni.showToast({ title: res.data?.message || '创建订单失败', icon: 'none' });
          console.error('创建订单失败', res);
          return;
        }

        const payData = res.data;

        // 根据 channel 调用对应支付
        let provider = this.selectedChannel === 'wechat' ? 'wxpay' : 'alipay';

        uni.requestPayment({
          provider: provider,
          orderInfo: payData.orderInfo || payData,  // 支付宝用 orderInfo，微信用 timeStamp 等
          timeStamp: payData.timeStamp,
          nonceStr: payData.nonceStr,
          package: payData.package,
          signType: payData.signType,
          paySign: payData.paySign,
          success: async () => {
            uni.showToast({ title: '支付成功！权益已到账', icon: 'success' });
            setTimeout(() => {
              uni.navigateTo({ url: '/pages/profile/profile' });  // 或 dashboard
            }, 1500);
          },
          fail: err => {
            uni.showToast({ title: '支付取消或失败', icon: 'none' });
            console.error('[支付失败]', err);
          }
        });

      } catch (error) {
        this.paying = false;
        console.error('[ERROR]', error);
        uni.showToast({ title: '网络错误，请重试', icon: 'none' });
      }
    }
  }
};
</script>

<style>
.payment-method {
  background: white;
  border-radius: 24rpx;
  padding: 40rpx 30rpx;
  margin-bottom: 60rpx;
  box-shadow: 0 8rpx 32rpx rgba(0,0,0,0.08);
}

.method-title {
  font-size: 36rpx;
  font-weight: bold;
  color: #333;
  margin-bottom: 30rpx;
}

.method-list {
  display: flex;
  justify-content: space-around;
}

.method-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 20rpx;
  border-radius: 16rpx;
  border: 2rpx solid #eee;
  width: 280rpx;
  transition: all 0.3s;
}

.method-item.active {
  border-color: #ff4d4f;
  background: #fff1f0;
}

.method-icon {
  width: 80rpx;
  height: 80rpx;
  margin-bottom: 20rpx;
}

.method-item text {
  font-size: 32rpx;
  color: #333;
}

.pay-btn {
  /* 原有样式 */
  margin-top: 20rpx;
}
.subscribe-page { background: #f5f5f5; min-height: 100vh; padding: 60rpx 30rpx; }
.header { text-align: center; margin-bottom: 60rpx; }
.title { font-size: 48rpx; font-weight: bold; color: #333; }
.benefit-list { background: white; border-radius: 24rpx; padding: 40rpx; margin-bottom: 40rpx; box-shadow: 0 8rpx 32rpx rgba(0,0,0,0.08); }
.benefit-item { font-size: 32rpx; margin: 20rpx 0; padding-left: 40rpx; position: relative; }
.benefit-item::before { content: '✓'; color: #52c41a; position: absolute; left: 0; font-size: 36rpx; }
.price-card { text-align: center; background: white; border-radius: 24rpx; padding: 60rpx 30rpx; margin-bottom: 60rpx; box-shadow: 0 8rpx 32rpx rgba(0,0,0,0.08); }
.price { font-size: 96rpx; font-weight: bold; color: #ff4d4f; }
.period { font-size: 48rpx; color: #666; }
.original { font-size: 32rpx; color: #999; text-decoration: line-through; margin-top: 16rpx; }
.pay-btn { background: #ff4d4f; color: white; border-radius: 50rpx; height: 120rpx; line-height: 120rpx; font-size: 40rpx; box-shadow: 0 12rpx 32rpx rgba(255,77,79,0.3); }
.tip { text-align: center; font-size: 28rpx; color: #666; margin-top: 40rpx; line-height: 48rpx; }
</style>