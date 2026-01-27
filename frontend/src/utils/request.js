// src/utils/request.js
import { BASE_URL } from './config';  // 如果你有 config.js

export const request = (options = {}) => {
  // 从 storage 取 token（全局统一）
  const token = uni.getStorageSync('token') || '';

  return new Promise((resolve, reject) => {
    uni.request({
      ...options,
      url: BASE_URL + (options.url.startsWith('/') ? options.url : '/' + options.url),
      header: {
        'Authorization': token ? `Bearer ${token}` : '',  // 全局自动加 token
        'Content-Type': 'application/json',
        ...options.header  // 允许覆盖
      },
      success: (res) => {
        if (res.statusCode >= 200 && res.statusCode < 300) {
          resolve(res.data);
        } else {
          // 统一处理错误（可选加登录失效跳转）
          if (res.statusCode === 401 || res.statusCode === 403) {
            uni.showToast({ title: '登录失效，请重新登录', icon: 'none' });
            uni.removeStorageSync('token');
            uni.navigateTo({ url: '/pages/login/login' });
          } else {
            uni.showToast({ title: res.data?.message || '请求失败', icon: 'none' });
          }
          reject(res);
        }
      },
      fail: (err) => {
        uni.showToast({ title: '网络错误，请检查网络', icon: 'none' });
        reject(err);
      }
    });
  });
};