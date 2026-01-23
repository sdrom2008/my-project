<template>
  <view class="login-container">
    <view class="header">
      <text class="title">NexusAI 智能店小二</text>
    </view>
    <button class="login-btn" @click="handleWxLogin" :loading="loading">
      微信一键登录
    </button>
    <view v-if="errorMsg" class="error">{{ errorMsg }}</view>
  </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const loading = ref(false)
const errorMsg = ref('')

const handleWxLogin = async () => {
  loading.value = true
  errorMsg.value = ''

  try {
    const loginRes = await uni.login({ provider: 'weixin' })
    if (!loginRes.code) {
      throw new Error('获取微信 code 失败')
    }

    console.log('微信 code:', loginRes.code)

    // ★★★ 用 ngrok 或内网IP（小程序不允许 localhost） ★★★
    const BASE_URL = 'https://127.0.0.1:7092'  // 或 http://192.168.x.x:7092

    const res = await uni.request({
      url: `${BASE_URL}/api/auth/wechat`,
      method: 'POST',
      header: { 'Content-Type': 'application/json' },
      data: { code: loginRes.code },
      timeout: 15000
    }) as any

    console.log('登录响应:', res)

    if (res.statusCode !== 200) {
      throw new Error(res.data?.message || `登录失败 (${res.statusCode})`)
    }

	const { token, sellerId, nickname, avatarUrl, subscriptionLevel } = res.data || {}
	if (!token || !sellerId) {
	  throw new Error('登录返回缺少 sellerId')
	}

	uni.setStorageSync('token', token)
	uni.setStorageSync('sellerId', sellerId)
	uni.setStorageSync('nickname', nickname || '商家')
	uni.setStorageSync('avatarUrl', avatarUrl)
	uni.setStorageSync('subscriptionLevel', subscriptionLevel || 'Free')

    uni.showToast({ title: '登录成功', icon: 'success' })

    // ★★★ 登录成功直接跳转聊天页 ★★★
	uni.reLaunch({
       url: '/pages/chat/index',
       success: () => console.log('跳转聊天页成功'),
       fail: (err) => console.error('跳转失败:', err)
     })
	} catch (err: any) {
		console.error('登录异常详情:', err)
		errorMsg.value = err.errMsg || err.message || '登录失败，请重试'
		uni.showToast({ title: errorMsg.value, icon: 'error' })
	} finally {
		loading.value = false
	}
	}
</script>

<style>
.login-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  background: linear-gradient(135deg, #f6f9ff, #e6f0ff);
}
.header {
  text-align: center;
  margin-bottom: 120rpx;
}
.title {
  font-size: 56rpx;
  font-weight: bold;
  color: #333;
}
.subtitle {
  font-size: 32rpx;
  color: #666;
  margin-top: 20rpx;
}
.login-btn {
  width: 640rpx;
  height: 100rpx;
  line-height: 100rpx;
  background: #07c160;
  color: white;
  border-radius: 50rpx;
  font-size: 36rpx;
}
.error {
  color: #ff4d4f;
  margin-top: 40rpx;
  font-size: 28rpx;
}
</style>