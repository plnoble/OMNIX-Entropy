# OMNIX-Entropy 发布流程

自用 GitHub 分发请先阅读 [GitHub 自用安装与更新通道](personal-github-updates.zh-CN.md)。该通道可以使用本机个人签名，但不会取消 App 与 Elevated Worker 的同签名校验。

正式发布按下面的顺序进行。每一步都必须成功，不能把“代码测试通过”当成“软件已经可以发布”。

1. 阅读[正式签名准备指南](signing-prerequisites.zh-CN.md)，准备 Windows SDK `signtool.exe` 和经过明确确认的 RSA 代码签名证书。
2. 在仓库根目录运行只读检查：

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\inspect-release-signing-prerequisites.ps1 -AsJson
   ```

3. 只有检查结果包含 `CanCreateSignedCandidate=true`，才使用指南中的命令生成签名候选包。
4. 把完整候选包复制到带还原检查点的一次性 Windows 环境，并运行 `verify-signed-release-candidate.ps1` 复核文件、签名和时间戳。
5. 严格按照[一次性 Windows 行为验收](disposable-windows-acceptance.zh-CN.md)完成十项测试。
6. 只有最终只读验证器返回 `BehavioralAcceptanceComplete=true`，才可以把候选包视为通过发布验收。

## 安全边界

- 不要在日常使用的主电脑上执行行为验收。
- 不要用自签名证书冒充正式发布证书。
- 不要关闭或绕过 Windows、安全软件、Smart App Control 或证书信任检查。
- 不要自动操作 UAC；接受和取消都由测试人员手动完成。
- 不要把证书密码、令牌 PIN、私钥或恢复信息写入仓库、命令记录或截图。
