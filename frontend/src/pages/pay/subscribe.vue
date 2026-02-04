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

    <button class="pay-btn" :loading="paying" @tap="createOrder">
      立即支付 99 元
    </button>

    <view class="tip">
      支付后立即生效，支持微信支付\n额度可累积，随时查看
    </view>
  </view>
</template>

<script>
const testbase = 'http://192.168.10.7:7092';

export default {
  data() {
    return {
      paying: false
    };
  },

  methods: {
    async createOrder() {
      this.paying = true;
      const token = uni.getStorageSync('token');

      const res = await uni.request({
        url: `${testbase}/api/pay/create-order`,
        method: 'POST',
        header: { Authorization: `Bearer ${token}` }
      });

      this.paying = false;

      if (res.statusCode !== 200 || !res.data.appId) {
        uni.showToast({ title: res.data?.msg || '创建订单失败', icon: 'none' });
        return;
      }

      const payData = res.data;

      uni.requestPayment({
        provider: 'wxpay',
        timeStamp: payData.timeStamp,
        nonceStr: payData.nonceStr,
        package: payData.package,
        signType: payData.signType,
        paySign: payData.paySign,
        success: () => {
          uni.showToast({ title: '支付成功！权益已到账', icon: 'success' });
          setTimeout(() => {
            uni.switchTab({ url: '/pages/dashboard/dashboard' });
          }, 1500);
        },
        fail: err => {
          uni.showToast({ title: '支付取消或失败', icon: 'none' });
          console.error('支付失败:', err);
        }
      });
    }
  }
};
</script>

<style>
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