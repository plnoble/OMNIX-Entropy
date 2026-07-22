# OMNIX-Entropy

OMNIX-Entropy 是一个面向普通用户的 Windows 电脑维护助手。它用直观结论解释 C 盘占用、应用安装位置、后台常驻和可处理项目，并把清理、卸载、迁移等修改动作放在可审计、需确认、可回退的安全管线之后。

## 当前状态

- Windows WPF / .NET 8
- 首页体检、应用管理、C 盘分析、安装管控、后悔药中心和 Computer Agent 已有本地实现
- 真实系统修改要求 App 与 Elevated Worker 通过同签名验证
- GitHub 自用发布通道正在接入，不会在 CI 中保存签名私钥

## 本地验证

```powershell
dotnet restore ComputerSecuritySoftware.slnx
dotnet test ComputerSecuritySoftware.slnx --configuration Release --no-restore
powershell -NoProfile -ExecutionPolicy Bypass -File .omx\verify-source-integrity.ps1
```

发布和更新安全说明见 [GitHub 自用安装与更新通道](docs/release/personal-github-updates.zh-CN.md)。
