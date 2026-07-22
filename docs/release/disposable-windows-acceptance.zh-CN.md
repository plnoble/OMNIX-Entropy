# OMNIX-Entropy 一次性 Windows 行为验收

这份协议只用于已签名、已通过传输复核的发布候选包。它不会自动运行 OMNIX-Entropy，不会自动操作 UAC，也不会把“创建了测试清单”写成“已经验收通过”。

## 环境要求

- 只使用 Windows Sandbox、可恢复快照的虚拟机，或专门的可重置测试机。
- 不得使用日常工作的主电脑。
- 不得使用个人文件、真实软件账号、真实聊天记录或唯一副本；所有操作对象都必须是可重建的测试夹具。
- 测试前记录快照或重置点编号。测试结束并导出证据后，重置一次性环境。
- UAC 必须由测试人员人工确认 UAC 或人工取消 UAC，禁止脚本代点。

## 准备专用夹具包

夹具工具不会进入用户发布包，只用于一次性 Windows 验收。它会创建测试软件、测试缓存、测试临时文件和当前用户测试启动项，因此不能在主电脑运行。

先在开发工作站发布独立夹具包，不要运行其中的程序：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\publish-acceptance-fixture-kit.ps1
```

把生成的 `OMNIX-Acceptance-Fixtures-*` 目录复制到一次性环境的固定本地磁盘，然后只读复核：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-acceptance-fixture-kit.ps1 `
  -FixtureKitDirectory "D:\OMNIX-Acceptance-Fixtures"
```

复核成功只说明夹具包内容和安全声明没有变化，不会自动创建夹具。

## 创建会话

先在一次性环境中把签名候选和已复核夹具包放到两个不同的固定本地目录，再运行：

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

脚本先复核候选包。复核失败时，不创建会话目录。复核成功后只创建：

- `session-manifest.json`：候选包、签名者、环境声明和必测场景。
- `acceptance-receipt.template.json`：尚未执行的空白收据模板。
- `evidence`：保存每个场景的独立证据。

记下命令输出中的 `SessionId`。只在一次性环境中使用同一个 SessionId 创建测试对象：

```powershell
& "D:\OMNIX-Acceptance-Fixtures\Css.AcceptanceFixtures.exe" provision `
  --session-id "<session-id>" `
  --attestation "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
```

创建后先读取状态并保存为夹具证据：

```powershell
& "D:\OMNIX-Acceptance-Fixtures\Css.AcceptanceFixtures.exe" status `
  --session-id "<session-id>"
```

## 十个必测场景

每个场景都使用独立夹具，记录开始和结束 UTC 时间、简短的人话结论，并至少保存一个证据文件。截图应同时显示动作结果和当前状态；必要时补充应用自身生成的非隐私日志。

1. `package-preflight`：记录签名候选复核成功，且状态仍为等待行为验收。
2. `uac-cancel-official-uninstall`：对测试软件发起官方卸载，在 UAC 中取消；确认软件仍在、OMNIX 不声称卸载成功，也不扫描或处理残留。
3. `uac-cancel-migration`：对可迁移测试软件发起迁移，在 UAC 中取消；确认源位置、目标位置和闭环状态没有被误报为成功。
4. `cleanup-quarantine-restore`：对测试垃圾执行清理并还原；确认先进入隔离区，后悔药中心可还原，内容哈希不变。
5. `app-cache-quarantine-restore`：对测试应用缓存执行清理并还原；确认不触碰主程序和用户文档。
6. `startup-disable-restore`：关闭启动项并还原；确认只改变指定测试启动项，恢复后原值和状态一致。
7. `official-uninstall-residue-review`：人工确认 UAC，运行测试软件的官方卸载；确认卸载后才扫描残留，低风险项只在再次确认后进入隔离区，高风险项不自动处理。
8. `migration-complete-closure-monitor`：人工确认 UAC，完成测试软件迁移；确认新位置可用，并监控原 C 盘位置是否继续写入。
9. `migration-failure-rollback`：使用故意失败的测试迁移夹具，确认迁移失败并回滚，原位置仍可用，目标半成品不会被当成成功。
10. `undo-center-restore`：从后悔药中心还原一条测试操作；确认时间线状态、文件内容和目标位置都与收据一致。

第 9 项在迁移最终确认前，人工启动受限文件锁；它最多保持 600 秒，只能锁定该会话派生的 `failure-lock.bin`：

```powershell
& "D:\OMNIX-Acceptance-Fixtures\Css.AcceptanceFixtures.exe" lock `
  --session-id "<session-id>" `
  --duration-seconds "120" `
  --attestation "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
```

## 填写收据

1. 将模板另存为会话目录内的 `acceptance-receipt.json`，不要改动 `session-manifest.json` 或候选包。
2. 每个 `Cases` 项的 `Outcome` 必须来自实际结果。只有实际满足预期时才能填写 `Pass`；失败、跳过或证据不足都不能填写为通过。
3. 每个证据路径相对于 `evidence` 目录，填写文件字节数和 SHA-256。一个证据文件不能重复用于多个场景。
4. 全部场景结束后先导出会话目录。夹具重置命令只会清除所有权标记与会话完全匹配的测试目录和 HKCU 记录：

```powershell
& "D:\OMNIX-Acceptance-Fixtures\Css.AcceptanceFixtures.exe" reset `
  --session-id "<session-id>" `
  --attestation "I CONFIRM THIS IS A DISPOSABLE WINDOWS TEST ENVIRONMENT"
```

5. 再重置整个一次性 Windows 环境，然后填写 `ResetCompleted=true` 和实际 `ResetCompletedUtc`。
6. `FinalVerdict` 只有在全部场景通过后才能填写 `Pass`。已知剩余风险写入 `KnownResidualRisks`，不要隐瞒。

## 最终只读复核

在干净的发布工作站上准备同一份签名候选和导出的会话目录，然后运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\verify-disposable-acceptance-receipt.ps1 `
  -PackageDirectory "D:\OMNIX-Candidate" `
  -FixtureKitDirectory "D:\OMNIX-Acceptance-Fixtures" `
  -SessionDirectory "D:\OMNIX-Acceptance\session-001" `
  -ReceiptPath "acceptance-receipt.json"
```

校验器只读复核候选签名、候选清单哈希、会话清单、环境声明、十个场景、时间顺序、证据长度与 SHA-256。任一项缺失、失败、跳过或被改动，均不会输出 `BehavioralAcceptanceComplete=true`。

行为验收通过只证明这一个候选包在这一个一次性环境和这组测试夹具中完成了规定流程。它不替代后续安装包信誉、杀毒软件兼容性、不同 Windows 版本和真实用户可用性验收。
