// utils/config.js
export const BASE_URL = 'https://127.0.0.1:7092';  // 改成你的后端地址
export const getToken = () => uni.getStorageSync('token') || '';