<template>
    <view class="login-container">
        <view class="header">
            <text class="title">NexusAI 卖家后台</text>
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
            if (!loginRes.code) throw new Error('获取 code 失败')

            console.log('code:', loginRes.code) // 调试

            const res = await uni.request({
                url: 'http://localhost:7092/api/auth/wechat',  // 改成你的后端端口
                method: 'POST',
                header: { 'Content-Type': 'application/json' },
                data: { code: loginRes.code },
                timeout: 10000
            }) as any

            if (res.statusCode !== 200) {
                throw new Error(res.data?.message || `登录失败 (${res.statusCode})`)
            }

            const { token, sellerId } = res.data

            uni.setStorageSync('token', token)
            uni.setStorageSync('sellerId', sellerId)

            uni.showToast({ title: '登录成功', icon: 'success' })
            uni.reLaunch({ url: '/pages/seller/dashboard/index' })
        } catch (err: any) {
            errorMsg.value = err.message || '登录出错，请检查网络'
            console.error('登录失败:', err)
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