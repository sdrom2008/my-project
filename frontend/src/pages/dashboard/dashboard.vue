<template>
    <view class="dashboard-page">
        <!-- 顶部欢迎栏 -->
        <view class="welcome-bar">
            <view class="greeting">
                <text class="hello">你好，</text>
                <text class="nickname">{{ profile.nickname || '商户' }}</text>
            </view>
            <view class="date">{{ currentDate }}</view>
        </view>

        <!-- 额度卡片 -->
        <view class="quota-card">
            <view class="card-header">
                <text class="title">剩余免费额度</text>
                <text class="icon">🔥</text>
            </view>
            <view class="quota-value">{{ profile.freeQuota || 0 }} <text class="unit">条</text></view>
            <view class="progress-bar">
                <view class="progress" :style="{ width: progress + '%' }"></view>
            </view>
            <view class="quota-tip">本月已用 {{ usedQuota }} 条，还剩 {{ profile.freeQuota || 0 }} 条</view>
        </view>

        <!-- 订阅状态 -->
        <view class="subscription-card">
            <view class="card-header">
                <text class="title">订阅状态</text>
            </view>
            <view class="status-row">
                <text class="label">当前等级：</text>
                <text class="value" :class="profile.subscriptionLevel === 'Pro' ? 'pro' : 'free'">
                    {{ profile.subscriptionLevel || '免费版' }}
                </text>
            </view>
            <view class="status-row">
                <text class="label">到期时间：</text>
                <text class="value">{{ profile.subscriptionEnd ? formatDate(profile.subscriptionEnd) : '未订阅' }}</text>
            </view>
            <button class="upgrade-btn" @tap="toSubscribe" v-if="!profile.subscriptionEnd || profile.subscriptionLevel === '免费版'">
                立即升级订阅
            </button>
        </view>

        <!-- 快捷入口 -->
        <view class="quick-actions">
            <view class="action-item" @tap="toChat">
                <view class="icon-wrapper chat-icon">💬</view>
                <text class="label">智能客服</text>
            </view>
            <view class="action-item" @tap="toProducts">
                <view class="icon-wrapper product-icon">🛒</view>
                <text class="label">商品管理</text>
            </view>
            <view class="action-item" @tap="toProfile">
                <view class="icon-wrapper profile-icon">👤</view>
                <text class="label">我的设置</text>
            </view>
            <view class="action-item" @tap="toMarketing">
                <view class="icon-wrapper marketing-icon">📈</view>
                <text class="label">营销方案</text>
            </view>
        </view>

        <!-- 底部提示 -->
        <view class="tip">
            额度不足？升级订阅享无限使用 + 更多高级功能
        </view>
    </view>
</template>

<script>
    const testbase = 'http://192.168.1.254:7092';

    export default {
        data() {
            return {
                profile: {
                    nickname: '',
                    freeQuota: 0,
                    subscriptionLevel: '免费版',
                    subscriptionEnd: null,
                    avatarUrl: ''
                },
                usedQuota: 0,           // 本月已用（示例，可后端返回）
                progress: 0,            // 进度条百分比
                currentDate: ''
            };
        },

        onLoad() {
            this.updateDate();
            this.loadProfile();
            // 每分钟更新一次日期（可选）
            setInterval(this.updateDate, 60000);
        },

        methods: {
            updateDate() {
                const now = new Date();
                this.currentDate = now.toLocaleDateString('zh-CN', { weekday: 'long', month: 'long', day: 'numeric' });
            },

            async loadProfile() {
                const token = uni.getStorageSync('token');
                if (!token) {
                    uni.navigateTo({ url: '/pages/login/login' });
                    return;
                }

                const res = await uni.request({
                    url: `${testbase}/api/seller/profile`,
                    header: { Authorization: `Bearer ${token}` }
                });

                if (res.statusCode === 200) {
                    this.profile = res.data;
                    // 示例：计算已用额度（实际应后端返回）
                    this.usedQuota = 100 - (this.profile.freeQuota || 0);
                    this.progress = this.usedQuota > 0 ? Math.min(100, (this.usedQuota / 100) * 100) : 0;
                } else {
                    uni.showToast({ title: '加载信息失败', icon: 'none' });
                }
            },

            formatDate(dateStr) {
                if (!dateStr) return '未订阅';
                const date = new Date(dateStr);
                return date.toLocaleDateString('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' });
            },

            toChat() {
                uni.switchTab({ url: '/pages/conversations/conversations' });
            },

            toProducts() {
                uni.navigateTo({ url: '/pages/products/products' });  // 后续实现
            },

            toProfile() {
                uni.navigateTo({ url: '/pages/profile/profile' });
            },

            toMarketing() {
                uni.navigateTo({ url: '/pages/marketing/marketing' });  // 后续实现
            },

            toSubscribe() {
                uni.navigateTo({ url: '/pages/pay/subscribe' });  // 后续支付页
            }
        }
    };
</script>

<style>
    .dashboard-page {
        background: #f5f5f5;
        min-height: 100vh;
        padding: 30rpx;
        padding-top: 120rpx;
    }

    .welcome-bar {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        height: 120rpx;
        background: linear-gradient(135deg, #667eea, #764ba2);
        color: white;
        padding: 0 30rpx;
        display: flex;
        flex-direction: column;
        justify-content: center;
        z-index: 99;
    }

    .hello {
        font-size: 32rpx;
    }

    .nickname {
        font-size: 40rpx;
        font-weight: bold;
    }

    .date {
        font-size: 28rpx;
        opacity: 0.9;
        margin-top: 8rpx;
    }

    .quota-card {
        background: white;
        border-radius: 24rpx;
        padding: 40rpx;
        margin-bottom: 30rpx;
        box-shadow: 0 8rpx 32rpx rgba(0,0,0,0.08);
    }

    .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30rpx;
    }

    .title {
        font-size: 36rpx;
        font-weight: bold;
    }

    .icon {
        font-size: 40rpx;
    }

    .quota-value {
        font-size: 80rpx;
        font-weight: bold;
        text-align: center;
        margin: 20rpx 0;
    }

    .unit {
        font-size: 40rpx;
        color: #666;
    }

    .progress-bar {
        height: 16rpx;
        background: #eee;
        border-radius: 8rpx;
        overflow: hidden;
        margin: 20rpx 0;
    }

    .progress {
        height: 100%;
        background: linear-gradient(to right, #667eea, #764ba2);
        transition: width 0.5s;
    }

    .quota-tip {
        text-align: center;
        font-size: 28rpx;
        color: #666;
    }

    .subscription-card {
        background: white;
        border-radius: 24rpx;
        padding: 40rpx;
        margin-bottom: 30rpx;
        box-shadow: 0 8rpx 32rpx rgba(0,0,0,0.08);
    }

    .status-row {
        display: flex;
        justify-content: space-between;
        margin: 20rpx 0;
        font-size: 32rpx;
    }

    .label {
        color: #666;
    }

    .value {
        font-weight: bold;
    }

    .pro {
        color: #007aff;
    }

    .free {
        color: #999;
    }

    .upgrade-btn {
        background: #007aff;
        color: white;
        border-radius: 50rpx;
        margin-top: 40rpx;
        height: 96rpx;
        line-height: 96rpx;
        text-align: center;
        font-size: 32rpx;
    }

    .quick-actions {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        margin-bottom: 40rpx;
    }

    .action-item {
        width: 45%;
        background: white;
        border-radius: 16rpx;
        padding: 40rpx 20rpx;
        text-align: center;
        margin-bottom: 30rpx;
        box-shadow: 0 4rpx 16rpx rgba(0,0,0,0.06);
    }

    .icon-wrapper {
        width: 120rpx;
        height: 120rpx;
        background: #f0f4ff;
        border-radius: 50%;
        margin: 0 auto 20rpx;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 60rpx;
    }

    .chat-icon {
        background: #e6f7ff;
    }

    .product-icon {
        background: #fff7e6;
    }

    .profile-icon {
        background: #f0f5ff;
    }

    .marketing-icon {
        background: #fff1f0;
    }

    .label {
        font-size: 30rpx;
        color: #333;
    }

    .tip {
        text-align: center;
        font-size: 28rpx;
        color: #999;
    }
</style>