<script setup lang="ts">
import { onLaunch, onShow, onHide } from '@dcloudio/uni-app';
onLaunch(() => {
  console.log('App Launch');
});
onShow(() => {
  console.log('App Show');
});
onHide(() => {
  console.log('App Hide');
});
// 这里写全局逻辑
</script>

<template>
  <view>
    <!-- 你的根组件内容，通常是 <navigator> 或 <tab-bar> 容器 -->
    <slot />
  </view>
</template>