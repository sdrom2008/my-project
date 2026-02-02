<template>
  <view class="profile-page">
    <!-- 顶部导航 -->
    <view class="navbar">
      <view class="title">我的</view>
      <view class="right-menu" @tap="showMenu">
        <text class="icon-dot">•••</text>
      </view>
    </view>

    <!-- 个人信息卡片 -->
    <view class="user-card">
      <view class="avatar-wrapper" @tap="chooseAvatar">
        <image :src="form.avatarUrl || '/static/default-avatar.png'" mode="aspectFill" class="avatar" />
        <view class="edit-icon">✎</view>
      </view>
      <view class="user-info">
        <view class="nickname">{{ form.nickname || '未设置昵称' }}</view>
        <view class="level">
          <text class="badge">{{ profile.subscriptionLevel || '免费版' }}</text>
          <text v-if="profile.freeQuota" class="quota">剩余 {{ profile.freeQuota }} 条</text>
        </view>
        <view class="phone">{{ form.phone || '未绑定手机号' }}</view>
      </view>
    </view>

    <!-- 店铺信息 -->
    <view class="section">
      <view class="section-title">店铺信息</view>
      <view class="form-group">
        <view class="label">店铺名称</view>
        <input v-model="form.shopName" placeholder="请输入店铺名称" class="input" />
      </view>
      <view class="form-group">
        <view class="label">店铺LOGO</view>
        <view class="logo-preview" v-if="form.shopLogo">
          <image :src="form.shopLogo" mode="aspectFill" class="logo-img" />
        </view>
        <input v-model="form.shopLogo" placeholder="LOGO URL 或上传" class="input" />
        <button class="upload-btn" @tap="chooseLogo">上传图片</button>
      </view>
      <view class="form-group">
        <view class="label">主营类目</view>
        <input v-model="form.mainCategory" placeholder="如：女装,数码（最多5个，用逗号分隔）" class="input" />
      </view>
      <view class="form-group">
        <view class="label">目标客户描述</view>
        <textarea v-model="form.targetCustomerDesc" placeholder="描述你的目标客户群体，帮助 AI 更精准回复" class="textarea" maxlength="200" show-confirm-bar />
      </view>
    </view>

    <!-- AI 偏好 -->
    <view class="section">
      <view class="section-title">AI 偏好设置</view>
      <view class="form-group">
        <view class="label">回复语气</view>
        <view class="segmented">
          <view v-for="(item, index) in toneOptions" :key="index" 
                :class="['segment', toneIndex === index ? 'active' : '']" 
                @tap="toneIndex = index; form.defaultReplyTone = item.value">
            {{ item.label }}
          </view>
        </view>
      </view>
      <view class="form-group">
        <view class="label">偏好语言</view>
        <view class="segmented">
          <view v-for="(item, index) in langOptions" :key="index" 
                :class="['segment', langIndex === index ? 'active' : '']" 
                @tap="langIndex = index; form.preferredLanguage = item.value">
            {{ item.label }}
          </view>
        </view>
      </view>
      <view class="form-group switch">
        <view class="label">开启主动营销提醒</view>
        <switch :checked="form.enableAutoMarketingReminder" @change="form.enableAutoMarketingReminder = $event.detail.value" />
      </view>
    </view>

    <!-- 账号安全 -->
    <view class="section">
      <view class="section-title">账号与安全</view>
      <view class="menu-item" @tap="logout">
        <text>退出登录</text>
        <text class="arrow">></text>
      </view>
      <view class="menu-item">
        <text>联系客服</text>
        <text class="arrow">></text>
      </view>
      <view class="version">版本 v1.0.0 | Synerixis © 2026</view>
    </view>

    <button class="save-btn" :loading="saving" @tap="saveConfig">保存设置</button>
  </view>
</template>

<script>
const testbase = 'http://192.168.1.254:7092';

export default {
  data() {
    return {
      profile: {},
      form: {
        nickname: '',
        avatarUrl: '',
        phone: '',
        shopName: '',
        shopLogo: '',
        mainCategory: '',
        targetCustomerDesc: '',
        defaultReplyTone: 'professional',
        preferredLanguage: 'zh',
        enableAutoMarketingReminder: true
      },
      toneOptions: [
        { label: '专业', value: 'professional' },
        { label: '亲切', value: 'friendly' },
        { label: '幽默', value: 'humorous' }
      ],
      toneIndex: 0,
      langOptions: [
        { label: '中文', value: 'zh' },
        { label: '英文', value: 'en' },
        { label: '双语', value: 'bilingual' }
      ],
      langIndex: 0,
      saving: false
    };
  },

  onLoad() {
    this.loadProfile();
  },

  methods: {
    async loadProfile() {
      const token = uni.getStorageSync('token');
      if (!token) return uni.navigateTo({ url: '/pages/login/login' });

      const res = await uni.request({
        url: `${testbase}/api/seller/profile`,
        header: { Authorization: `Bearer ${token}` }
      });

      if (res.statusCode === 200) {
        this.profile = res.data;
        this.form = {
          nickname: res.data.nickname || '',
          avatarUrl: res.data.avatarUrl || '',
          phone: res.data.phone || '',
          shopName: res.data.config?.shopName || '',
          shopLogo: res.data.config?.shopLogo || '',
          mainCategory: res.data.config?.mainCategory || '',
          targetCustomerDesc: res.data.config?.targetCustomerDesc || '',
          defaultReplyTone: res.data.config?.defaultReplyTone || 'professional',
          preferredLanguage: res.data.config?.preferredLanguage || 'zh',
          enableAutoMarketingReminder: res.data.config?.enableAutoMarketingReminder ?? true
        };
        this.toneIndex = this.toneOptions.findIndex(t => t.value === this.form.defaultReplyTone);
        this.langIndex = this.langOptions.findIndex(l => l.value === this.form.preferredLanguage);
      }
    },

    toneChange(e) {
      this.form.defaultReplyTone = this.toneOptions[e.detail.value].value;
    },

    langChange(e) {
      this.form.preferredLanguage = this.langOptions[e.detail.value].value;
    },

    async chooseAvatar() {
      uni.chooseImage({
        count: 1,
        success: res => {
          const tempFile = res.tempFilePaths[0];
          uni.uploadFile({
            url: `${testbase}/api/upload/avatar`,
            filePath: tempFile,
            name: 'file',
            header: { Authorization: `Bearer ${uni.getStorageSync('token')}` },
            success: uploadRes => {
              const data = JSON.parse(uploadRes.data);
              if (data.url) {
                this.form.avatarUrl = data.url;
                uni.showToast({ title: '头像上传成功', icon: 'success' });
              }
            }
          });
        }
      });
    },

    async saveConfig() {
      this.saving = true;
      const token = uni.getStorageSync('token');
      const res = await uni.request({
        url: `${testbase}/api/seller/config`,
        method: 'PUT',
        header: { 'content-type': 'application/json', Authorization: `Bearer ${token}` },
        data: this.form
      });

      this.saving = false;
      if (res.statusCode === 200) {
        uni.showToast({ title: '保存成功', icon: 'success' });
      } else {
        uni.showToast({ title: res.data?.msg || '保存失败', icon: 'none' });
      }
    },

    logout() {
      uni.showModal({
        title: '提示',
        content: '确定退出登录吗？',
        success: res => {
          if (res.confirm) {
            uni.removeStorageSync('token');
            uni.removeStorageSync('sellerId');
            uni.switchTab({ url: '/pages/login/login' });
          }
        }
      });
    }
  }
};
</script>

<style>
.profile-page { background: #f5f5f5; min-height: 100vh; padding-bottom: 120rpx; }
.navbar { height: 88rpx; background: #000; color: white; display: flex; align-items: center; justify-content: space-between; padding: 0 30rpx; position: fixed; top: 0; left: 0; right: 0; z-index: 999; }
.title { font-size: 36rpx; font-weight: bold; }
.right-menu { font-size: 48rpx; }
.user-card { background: white; margin: 88rpx 30rpx 30rpx; padding: 40rpx; border-radius: 24rpx; display: flex; align-items: center; box-shadow: 0 8rpx 24rpx rgba(0,0,0,0.08); }
.avatar-wrapper { position: relative; width: 140rpx; height: 140rpx; }
.avatar { width: 140rpx; height: 140rpx; border-radius: 50%; border: 4rpx solid #fff; }
.edit-icon { position: absolute; bottom: 0; right: 0; width: 48rpx; height: 48rpx; background: #007aff; color: white; border-radius: 50%; text-align: center; line-height: 48rpx; font-size: 28rpx; }
.user-info { margin-left: 30rpx; }
.nickname { font-size: 40rpx; font-weight: bold; }
.level { margin: 12rpx 0; }
.badge { background: #007aff; color: white; padding: 4rpx 16rpx; border-radius: 20rpx; font-size: 24rpx; }
.quota { font-size: 28rpx; color: #666; margin-left: 20rpx; }
.phone { font-size: 28rpx; color: #666; }
.section { background: white; margin: 0 30rpx 30rpx; padding: 40rpx; border-radius: 24rpx; box-shadow: 0 8rpx 24rpx rgba(0,0,0,0.08); }
.section-title { font-size: 36rpx; font-weight: bold; margin-bottom: 40rpx; }
.form-item { margin-bottom: 48rpx; }
.label { font-size: 30rpx; color: #333; margin-bottom: 20rpx; display: block; }
.input, .textarea { width: 100%; padding: 24rpx 30rpx; background: #f8f8f8; border-radius: 16rpx; font-size: 30rpx; }
.textarea { height: 200rpx; }
.segmented { display: flex; background: #f8f8f8; border-radius: 16rpx; overflow: hidden; }
.segment { flex: 1; text-align: center; padding: 24rpx 0; font-size: 30rpx; color: #666; }
.segment.active { background: #007aff; color: white; }
.switch { display: flex; justify-content: space-between; align-items: center; font-size: 32rpx; }
.save-btn { position: fixed; bottom: 40rpx; left: 30rpx; right: 30rpx; background: #007aff; color: white; border-radius: 50rpx; height: 96rpx; line-height: 96rpx; font-size: 36rpx; text-align: center; box-shadow: 0 8rpx 24rpx rgba(0,122,255,0.3); }
</style>