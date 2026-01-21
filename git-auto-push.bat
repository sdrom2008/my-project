@echo off
setlocal enabledelayedexpansion

echo.
echo ===============================================
echo        自动提交并推送至 GitHub (main 分支)
echo ===============================================
echo.

:: 获取当前日期（格式：2026-01-21）
set "commit_date=%date:~0,4%-%date:~5,2%-%date:~8,2%"

:: 获取当前时间（格式：14:35:22）
set "commit_time=%time:~0,2%:%time:~3,2%:%time:~6,2%"
set "commit_time=!commit_time: =0!"

:: 组合提交信息
set "commit_msg=Update %commit_date% %commit_time%"

echo 提交信息: %commit_msg%
echo.

:: 检查是否有改动
git status --porcelain | findstr "." >nul
if errorlevel 1 (
    echo 当前没有改动，无需提交。
    goto :end
)

:: 添加所有改动
echo 添加所有改动...
git add .
if errorlevel 1 (
    echo 添加失败！
    pause
    exit /b
)

:: 提交
echo 提交中...
git commit -m "%commit_msg%"
if errorlevel 1 (
    echo 提交失败！
    pause
    exit /b
)

:: 推送
echo 推送至 main 分支...
git push origin main
if errorlevel 1 (
    echo 推送失败！请检查网络或权限。
    pause
    exit /b
)

echo.
echo ===============================================
echo           提交并推送成功！
echo 提交信息: %commit_msg%
echo ===============================================
echo.

pause
:end