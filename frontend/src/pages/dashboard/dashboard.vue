<template>
  <view class="dashboard">
    <view>欢迎，{{ nickname || '商户' }}</view>
    <view>剩余免费额度：{{ quota.freeQuota }} 条</view>
    <view>订阅状态：{{ quota.subscriptionEnd ? '有效至 ' + formatDate(quota.subscriptionEnd) : '未订阅' }}</view>
    <button v-if="!quota.subscriptionEnd" @tap="toSubscribe">升级订阅</button>
    <button @tap="toChat">去聊天</button>
  </view>
</template>

<script>
export default {
  data() {
    return {
      quota: { freeQuota: 0, subscriptionEnd: null },
      nickname: ''
    }
  },
  onLoad() {
    this.loadProfile();
  },
  methods: {
    async loadProfile() {
      const res = await uni.request({
        url: `${testbase}/api/seller/info`,
        header: { Authorization: 'Bearer ' + uni.getStorageSync('token') }
      });
      if (res.statusCode === 200) {
        this.quota = res.data.quota;
        this.nickname = res.data.nickname;
      }
    },
    toSubscribe() {
      uni.navigateTo({ url: '/pages/pay/subscribe' });
    }
  }
}
</script>