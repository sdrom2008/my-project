<template>
  <view class="profile-container">
    <view class="header">
      <view class="title">我的设置</view>
    </view>

    <view class="section">
      <view class="section-title">店铺信息</view>
      <view class="form-item">
        <text>店铺名称</text>
        <input v-model="form.shopName" placeholder="请输入店铺名称" />
      </view>
      <view class="form-item">
        <text>店铺LOGO</text>
        <input v-model="form.shopLogo" placeholder="LOGO URL 或上传" />
      </view>
      <view class="form-item">
        <text>主营类目</text>
        <input v-model="form.mainCategory" placeholder="如：女装,数码" />
      </view>
      <view class="form-item">
        <text>目标客户描述</text>
        <textarea v-model="form.targetCustomerDesc" placeholder="描述你的目标客户群体" />
      </view>
    </view>

    <view class="section">
      <view class="section-title">AI 偏好</view>
      <view class="form-item">
        <text>回复语气</text>
        <picker @change="toneChange" :value="toneIndex" :range="toneOptions">
          <view class="picker">{{ toneOptions[toneIndex] }}</view>
        </picker>
      </view>
      <view class="form-item">
        <text>偏好语言</text>
        <picker @change="langChange" :value="langIndex" :range="langOptions">
          <view class="picker">{{ langOptions[langIndex] }}</view>
        </picker>
      </view>
      <view class="form-item switch">
        <text>开启主动营销提醒</text>
        <switch :checked="form.enableAutoMarketingReminder" @change="reminderChange" />
      </view>
      <view class="form-item">
        <text>记忆保留天数（0=永久）</text>
        <input type="number" v-model="form.memoryRetentionDays" placeholder="180" />
      </view>
    </view>

    <button class="save-btn" @tap="save">保存设置</button>
  </view>
</template>

<script>
const testbase = 'http://192.168.1.254:7092';

export default {
  data() {
    return {
      form: {
        shopName: '',
        shopLogo: '',
        mainCategory: '',
        targetCustomerDesc: '',
        defaultReplyTone: 'professional',
        preferredLanguage: 'zh',
        enableAutoMarketingReminder: true,
        memoryRetentionDays: 180
      },
      toneOptions: ['professional', 'friendly', 'humorous'],
      toneIndex: 0,
      langOptions: ['中文 (zh)', '英文 (en)', '双语 (bilingual)'],
      langIndex: 0
    };
  },

  onLoad() {
    this.loadProfile();
  },

  methods: {
    async loadProfile() {
      const token = uni.getStorageSync('token');
      if (!token) {
        uni.showToast({ title: '请先登录', icon: 'none' });
        return;
      }

      const res = await uni.request({
        url: `${testbase}/api/seller/profile`,
        method: 'GET',
        header: { Authorization: `Bearer ${token}` }
      });

      if (res.statusCode === 200) {
        const data = res.data;
        this.form = {
          shopName: data.config?.shopName || '',
          shopLogo: data.config?.shopLogo || '',
          mainCategory: data.config?.mainCategory || '',
          targetCustomerDesc: data.config?.targetCustomerDesc || '',
          defaultReplyTone: data.config?.defaultReplyTone || 'professional',
          preferredLanguage: data.config?.preferredLanguage || 'zh',
          enableAutoMarketingReminder: data.config?.enableAutoMarketingReminder ?? true,
          memoryRetentionDays: data.config?.memoryRetentionDays ?? 180
        };

        this.toneIndex = this.toneOptions.indexOf(this.form.defaultReplyTone);
        this.langIndex = this.langOptions.findIndex(l => l.includes(this.form.preferredLanguage));
      }
    },

    toneChange(e) {
      this.toneIndex = e.detail.value;
      this.form.defaultReplyTone = this.toneOptions[this.toneIndex];
    },

    langChange(e) {
      this.langIndex = e.detail.value;
      const langMap = { 0: 'zh', 1: 'en', 2: 'bilingual' };
      this.form.preferredLanguage = langMap[this.langIndex];
    },

    reminderChange(e) {
      this.form.enableAutoMarketingReminder = e.detail.value;
    },

    async save() {
      const token = uni.getStorageSync('token');
      const res = await uni.request({
        url: `${testbase}/api/seller/config`,
        method: 'PUT',
        header: {
          'content-type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        data: this.form
      });

      if (res.statusCode === 200) {
        uni.showToast({ title: '保存成功', icon: 'success' });
      } else {
        uni.showToast({ title: res.data?.msg || '保存失败', icon: 'none' });
      }
    }
  }
};
</script>

<style>
.profile-container { padding: 30rpx; }
.header { text-align: center; margin-bottom: 40rpx; }
.title { font-size: 44rpx; font-weight: bold; }
.section { background: #fff; padding: 30rpx; border-radius: 16rpx; margin-bottom: 30rpx; box-shadow: 0 4rpx 12rpx rgba(0,0,0,0.05); }
.section-title { font-size: 34rpx; font-weight: bold; margin-bottom: 30rpx; }
.form-item { margin-bottom: 40rpx; }
.form-item text { display: block; font-size: 30rpx; margin-bottom: 16rpx; color: #333; }
input, textarea { width: 100%; padding: 20rpx; border: 1rpx solid #eee; border-radius: 12rpx; font-size: 30rpx; }
.picker { padding: 20rpx; background: #f8f8f8; border-radius: 12rpx; }
.switch { display: flex; justify-content: space-between; align-items: center; font-size: 30rpx; }
.save-btn { background: #007aff; color: white; border-radius: 50rpx; margin-top: 60rpx; }
</style>