OMNIX-Entropy 候选发布包

这是使用已有代码签名证书生成的候选发布包，不代表已经完成生产验收。

签名边界：
- 发布脚本不会导入证书，也不会生成或信任任何证书。
- 主程序和安全助手必须由当前用户证书库中的同一代码签名证书签名。
- 签名必须带 RFC3161 时间戳，并在生成清单前重新验证为有效。

下一步：
- 先查看 package-manifest.json，确认 ValidSameSigner 为 true。
- MutationReadiness 只能是 EligibleForDisposableMachineAcceptance。
- 必须先在一次性测试环境验证安装、清理、卸载、迁移、回滚和 UAC 取消流程。
- 一次性测试环境验收完成前，不要在主力电脑上执行真实修改。

运行要求：.NET 8 Desktop Runtime。
