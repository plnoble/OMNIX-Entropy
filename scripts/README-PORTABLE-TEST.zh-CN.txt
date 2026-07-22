OMNIX-Entropy 便携测试包

启动方式：双击 Css.App.exe。
运行要求：此包为 FrameworkDependent 模式，需要安装 .NET 8 Desktop Runtime。

现在可以测试：
- 首页体检、C 盘分析、应用管理和 Computer Agent 等 read-only 只读能力。
- 查看建议、预览计划、确认页和后悔药中心的界面。

安全边界：
- 这不是正式生产安装包，不会替你安装或导入签名证书。
- 当前主程序和工作进程的签名状态、哈希及修改动作准备状态见 package-manifest.json。
- 卸载、迁移、服务、注册表和文件处理必须继续经过程序内安全管线。
- 在完成有效同签名校验和一次性测试机验收前，不要在主力电脑上测试真实修改动作。

只读测试发现问题时，请保留 package-manifest.json 和界面截图。
