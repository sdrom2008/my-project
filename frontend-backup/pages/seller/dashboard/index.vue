<template>
    <view class="dashboard">
        <text class="welcome">»¶Ó­À´µ½ NexusAI ÒÇ±íÅÌ£¡</text>
        <button @click="logout">ÍË³öµÇÂ¼</button>
    </view>
</template>

<script setup lang="ts">
    const logout = () => {
        uni.removeStorageSync('token')
        uni.removeStorageSync('sellerId')
        uni.reLaunch({ url: '/pages/login/index' })
    }
</script>

<style>
    .dashboard {
        padding: 40rpx;
        text-align: center;
    }

    .welcome {
        font-size: 48rpx;
        font-weight: bold;
        margin-bottom: 80rpx;
    }

    button {
        width: 600rpx;
        margin: 40rpx auto;
        height: 100rpx;
        line-height: 100rpx;
        font-size: 36rpx;
    }
</style>