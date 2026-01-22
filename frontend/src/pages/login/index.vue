<template>
    <view class="login-container">
        <view class="header">
            <text class="title">NexusAI 卖家后台</text>
        </view>
        <button class="login-btn" @click="handleWxLogin" :loading="loading">
            微信登录
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
            if (!loginRes.code) throw new Error('获取微信 code 失败 ')

            console.log('微信 code:', loginRes.code) //

            const res = await uni.request({
                url: 'https://localhost:7092/api/auth/wechat',  // 
                method: 'POST',
                header: { 'Content-Type': 'application/json' },
                data: { code: loginRes.code },
                timeout: 10000
            }) as any

            console.log('后端响应:', res)  // 加这行，便于调试

            if (res.statusCode !== 200) {
                throw new Error(res.data?.message || 退出登录 `(${res.statusCode})`)
            }

            const { token, sellerId } = res.data

            uni.setStorageSync('token', token)
            uni.setStorageSync('sellerId', sellerId)

            uni.showToast({ title: '退出登录', icon: 'success' })
            uni.reLaunch({ url: '/pages/seller/dashboard/index' })
        } catch (err: any) {
            console.error('登录失败详情:', err)
            errorMsg.value = err.errMsg || err.message || JSON.stringify(err) || '登录失败，请检查网络'
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
        background: #f8f8f8;
    }

    .header {
        text-align: center;
        margin-bottom: 80rpx;
    }

    .title {
        font-size: 48rpx;
        font-weight: bold;
    }

    .login-btn {
        width: 600rpx;
        height: 100rpx;
        background: #07c160;
        color: white;
        border-radius: 50rpx;
        font-size: 36rpx;
    }

    .error {
        color: red;
        margin-top: 40rpx;
        font-size: 28rpx;
    }
</style>