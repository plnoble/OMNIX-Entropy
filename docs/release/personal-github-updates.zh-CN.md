# GitHub 自用安装与更新通道

这个通道面向 OMNIX-Entropy 作者自己的 Windows 电脑。它不要求购买商业证书，但不会取消程序内部的同签名校验。

## 信任模型

- GitHub Releases 负责保存版本、安装包、更新清单和哈希。
- GitHub Actions 只执行还原、测试和源完整性检查，不接触证书私钥。
- App、Elevated Worker 和后续安装程序必须由同一个本机个人代码签名证书签署。
- 个人证书的私钥只保存在作者电脑，不能提交到 Git、GitHub Secrets、日志或截图。
- Windows SmartScreen 可能继续提示；用户可以手动确认，但程序不能关闭或绕过 Windows 安全功能。
- GitHub 下载成功不等于可信。更新前仍须核对固定仓库、文件长度、SHA-256 和实际 Authenticode 签名者。

## 发布顺序

1. 提交并推送通过测试的源代码。
2. 运行 `.github/workflows/ci.yml`，确认 Release 测试和源完整性检查通过。
3. 在本机生成 portable package，再使用已有签名脚本签署 App 和 Elevated Worker。
4. 使用已有只读验证器检查签名候选包。
5. 运行下面的命令生成 GitHub 发布资产：

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\prepare-personal-github-release.ps1 `
     -InstallerDirectory .artifacts\OMNIX-Entropy-installer-v0.1.0 `
     -Version 0.1.0
   ```

6. 检查新目录中的 ZIP、`omnix-release.json` 和 `SHA256SUMS.txt`。
7. 只有确认内容正确后，才增加 `-PublishDraft`。脚本只创建草稿 Release，不会直接公开发布。
8. 在 GitHub 页面再次检查资产和说明，然后由用户手动发布草稿。

## 当前限制

- 当前脚本不会生成、导入或信任个人证书。
- 当前应用内更新客户端尚未取得安装权限；Release 通道完成后再接入下载、验证、明确确认和回退。
- 没有有效同签名包时，高权限清理、迁移和卸载仍必须保持阻止状态。
