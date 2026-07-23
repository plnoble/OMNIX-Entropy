# D 盘优先的个人安装器

OMNIX-Entropy 使用 Inno Setup 生成单个 Windows 安装程序。安装向导默认选择 `D:\Software\OMNIX-Entropy\Install`，目录选择页始终显示；用户可以手动改到其他位置。安装器拒绝 `/SILENT` 和 `/VERYSILENT`，应用也不会在后台静默运行它。

## 安全边界

- 安装器只能从已经通过 `verify-signed-release-candidate.ps1` 的候选包构建。
- App、Elevated Worker、安装器和卸载器必须使用同一个本机代码签名证书。
- 构建脚本只接受显式的 `ISCC.exe`、`signtool.exe`、证书指纹和 HTTPS RFC3161 时间戳地址。
- 脚本不会生成、导入、导出或信任证书，也不会关闭 Windows 或安全软件的检查。
- 构建完成不等于可以发布；安装器还必须通过独立的只读验证器。

Inno Setup 的 `SignTool` 会签署 Setup；启用 `SignedUninstaller=yes` 后也会签署卸载器。架构使用 `x64compatible`，允许在 x64 Windows 和支持 x64 模拟的 Arm64 Windows 11 上运行。参考：[Inno Setup SignTool](https://jrsoftware.org/ishelp/topic_setup_signtool.htm)、[SignedUninstaller](https://jrsoftware.org/ishelp/topic_setup_signeduninstaller.htm)、[ArchitecturesAllowed](https://jrsoftware.org/ishelp/topic_setup_architecturesallowed.htm)。

## 1. 准备工具

需要：

- 当前版 Inno Setup（推荐 7.x）的 `ISCC.exe`。
- Windows SDK 的 `signtool.exe`。
- `Cert:\CurrentUser\My` 中明确选择的 RSA 代码签名证书，具备私钥和代码签名 EKU。
- 已通过验证的 `SignedReleaseCandidate` 目录。

本机 2026-07-23 的只读检查结果：SignTool 已找到；Inno 编译器未找到；可用代码签名证书为 0。因此当前不能诚实生成可发布安装器。

## 2. 构建安装器

所有路径都必须使用本机真实的绝对路径：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\build-personal-installer.ps1 `
  -PackageDirectory "D:\Agent\Project\OMNIX-Entropy\.artifacts\OMNIX-Entropy-release-YYYYMMDD-HHMMSS" `
  -Version 0.1.0 `
  -InnoCompilerPath "D:\Development\Inno Setup 7\ISCC.exe" `
  -SignToolPath "D:\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" `
  -CertificateThumbprint "40_HEX_CHARACTER_THUMBPRINT" `
  -TimestampUrl "https://YOUR_RFC3161_TIMESTAMP_ENDPOINT"
```

成功后输出目录只包含：

- `OMNIX-Entropy-0.1.0-win-x64-setup.exe`
- `installer-manifest.json`

## 3. 独立验证

把输出复制到另一固定本地路径后运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-personal-installer.ps1 `
  -InstallerDirectory "D:\OMNIX-Installer-0.1.0"
```

只有输出 `CanStageGitHubRelease=true` 才能进入 GitHub 暂存。验证器会重新检查路径、额外文件、默认目录策略、静默安装策略、长度、SHA-256、Authenticode、时间戳和同签名者；它不会运行安装器。

## 4. 生成 GitHub 草稿发布

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\prepare-personal-github-release.ps1 `
  -InstallerDirectory ".artifacts\OMNIX-Entropy-installer-v0.1.0" `
  -Version 0.1.0
```

先检查本地 `GitHub-v0.1.0` 暂存目录。确认 setup、`installer-manifest.json`、`omnix-release.json` 和 `SHA256SUMS.txt` 后，才可以再次运行并显式加入 `-PublishDraft`。脚本只创建 GitHub 草稿，不会公开发布或安装软件。
