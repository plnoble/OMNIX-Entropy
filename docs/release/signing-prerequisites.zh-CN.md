# OMNIX-Entropy 正式签名准备指南

这份指南面向不熟悉 Windows 发布流程的人。它只说明当前仓库已经支持的发布路线：使用本机 `CurrentUser\My` 证书库中的受信任 RSA 代码签名证书，通过 Windows SDK 的 `signtool.exe` 对主程序和权限进程签名。

仓库不会自动安装 Windows SDK，不会自动购买、申请或导入证书，不会修改受信任根证书库，也不会替你选择证书。证书来源、费用、身份验证、私钥设备和时间戳服务必须由你与证书提供方确认。

## 当前支持范围

当前脚本支持：

- Windows SDK `signtool.exe`。
- 位于 `Cert:\CurrentUser\My`、带可访问私钥的有效证书。
- 明确包含代码签名 EKU `1.3.6.1.5.5.7.3.3` 的 RSA 证书。
- 同一个证书签署 `Css.App.exe` 和 `Css.Elevated.exe`。
- SHA-256 文件摘要和 HTTPS RFC3161 时间戳。
- 签名后重新验证、传输后再次验证、一次性 Windows 行为验收。

当前脚本不支持 Microsoft Store 自动签名、Azure Artifact Signing、SignPath 或其他远程签名服务。它们是微软列出的可选发布路线，但需要另一套打包或远程签名集成，不能把它们的账号或密钥直接填入当前脚本。

微软官方说明：

- [Windows SDK 下载](https://learn.microsoft.com/en-us/windows/apps/windows-sdk/downloads)
- [SignTool 命令和摘要算法要求](https://learn.microsoft.com/en-us/dotnet/framework/tools/signtool-exe)
- [Windows 应用代码签名选项](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options)
- [Smart App Control 的代码签名要求](https://learn.microsoft.com/en-us/windows/apps/develop/smart-app-control/code-signing-for-smart-app-control)

微软说明 Smart App Control 当前不支持 ECC 签名，因此 OMNIX-Entropy 的本地发布管线只接受 RSA 代码签名证书。自签名证书不用于公开发布；它不能建立正常的 Windows 发布者信任，也不能代替真实发布验收。

## 第一步：准备 SignTool

从上面的微软官方 Windows SDK 页面选择仍受支持的稳定版 SDK。安装是系统级软件变更，必须由你明确确认并亲自完成；OMNIX-Entropy 仓库不会替你下载安装。

常见安装位置是：

```text
C:\Program Files (x86)\Windows Kits\10\bin\<SDK版本>\x64\signtool.exe
```

如果 SDK 安装在其他磁盘，检查器会只读查询两个标准 `Windows Kits\Installed Roots` 注册表位置的 `KitsRoot10`，再仅枚举该根目录下的 `bin\<版本>\x64|x86|arm64`。它不会递归扫描整块磁盘，也不会修改 SDK 或注册表。

不要下载来源不明的单独 `signtool.exe`。安装后在仓库根目录运行只读检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\inspect-release-signing-prerequisites.ps1 -AsJson
```

工具准备成功时应看到 `SignToolFound=true`，并记录输出中的完整 `SignToolPath`。

## 第二步：准备发布证书

当前路线要求受信任提供方签发的 RSA Authenticode 代码签名证书。购买或申请前，先向提供方确认：

- 用途包含 Windows Authenticode 代码签名。
- 公钥算法是 RSA，不是 ECC。
- 证书明确包含代码签名 EKU。
- 私钥能通过当前 Windows 用户访问；硬件令牌或云 HSM 的驱动和登录方式由提供方负责。
- 提供可用的 HTTPS RFC3161 时间戳地址。

证书安装或硬件令牌初始化可能改变本机状态，必须由你明确确认，并遵循提供方的官方步骤。不要把 PFX 密码、令牌 PIN、私钥或恢复信息写进仓库、命令历史、截图或聊天记录。

准备完成后再次运行检查器。只有同时满足以下条件才继续：

```text
SignToolFound=true
CertificateStoreReadable=true
CodeSigningCertificateCount>=1
RequiresExplicitCertificateSelection=true
CanCreateSignedCandidate=true
Readiness=ReadyForExplicitCertificateSelection
```

检查器只列出候选项，不会自动选择。要使用的 `CertificateThumbprint` 必须由你明确确认。

## 第三步：生成签名候选包

使用最近一次经过验证的便携测试包作为输入。所有路径都应换成当前机器上的完整路径：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-signed-release-package.ps1 `
  -SourcePackageDirectory "D:\Agent\Project\OMNIX-Entropy\.artifacts\OMNIX-Entropy-test-20260719-014731" `
  -SignToolPath "C:\Program Files (x86)\Windows Kits\10\bin\<SDK版本>\x64\signtool.exe" `
  -CertificateThumbprint "<你明确确认的40位证书指纹>" `
  -TimestampUrl "<证书提供方给出的HTTPS RFC3161地址>"
```

脚本会创建新的候选目录，不覆盖源包。它会：

1. 复核源包清单和生产命令面。
2. 再次检查证书有效期、私钥、代码签名 EKU 和 RSA 算法。
3. 分别签署主程序和权限进程。
4. 使用 SHA-256 和 RFC3161 时间戳。
5. 重新读取两个签名，确认状态、时间戳和签名者一致。
6. 生成新的哈希清单和 ZIP。

生成成功只代表“可以开始一次性环境验收”，不代表已经完成发布验收。

## 第四步：传输后复核

把完整候选目录复制到一次性 Windows 环境的固定本地磁盘，然后在那里运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-signed-release-candidate.ps1 `
  -PackageDirectory "D:\OMNIX-Candidate"
```

只有输出 `CanBeginDisposableAcceptance=true` 才能继续。复核器会再次检查所有文件哈希、额外文件、两个签名、时间戳、同一签名者、RSA 算法和生产命令面，但不会启动软件或修改系统。

## 第五步：完成一次性 Windows 验收

严格按照 [一次性 Windows 行为验收](disposable-windows-acceptance.zh-CN.md) 执行。创建会话的入口是：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\new-disposable-acceptance-session.ps1 `
  -PackageDirectory "D:\OMNIX-Candidate" `
  -FixtureKitDirectory "D:\OMNIX-Acceptance-Fixtures" `
  -SessionDirectory "D:\OMNIX-Acceptance\session-001" `
  -EnvironmentKind "VirtualMachine" `
  -PrimaryMachine false `
  -IsDisposableEnvironment true `
  -ResetCheckpointId "vm-checkpoint-before-omnix" `
  -OperatorAttestation "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
```

十项场景、证据哈希、环境重置和最终收据全部通过后，最终只读验证器才会返回 `BehavioralAcceptanceComplete=true`。

## 明确禁止

- 不在日常使用的主电脑运行验收夹具。
- 不用自签名证书冒充公开发布证书。
- 不把普通 `localhost`、HTTPS 或开发测试证书当成代码签名证书。
- 不导入来源不明的证书或私钥。
- 不为了运行候选包修改 Windows 受信任根、Smart App Control 或安全软件设置。
- 不自动点击 UAC；接受和取消都由测试人员人工完成并留证据。
- 不把“签名成功”写成“十项行为验收完成”。
