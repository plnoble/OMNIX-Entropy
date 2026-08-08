namespace Css.Core.Agent;

public enum AgentSymptomKind
{
    NetworkConnection,
    Sound,
    BluetoothOrPeripheral,
    Display,
    DriverOrDevice,
    BlueScreenOrRestart
}

public sealed class AgentSymptomPrompt
{
    public required string Id { get; init; }
    public required AgentSymptomKind Kind { get; init; }
    public required string Label { get; init; }
    public required string Question { get; init; }
}

public sealed class AgentSymptomTriage
{
    public required AgentSymptomKind Kind { get; init; }
    public required string Headline { get; init; }
    public required string Summary { get; init; }
    public required string CheckedSummary { get; init; }
    public required string UnknownSummary { get; init; }
    public required string UrgencySummary { get; init; }
    public required string PrimaryNextStep { get; init; }
    public required AgentShortcutKind ShortcutKind { get; init; }
    public required string ShortcutId { get; init; }
    public required string NavigationLabel { get; init; }
}

public static class AgentSymptomPromptCatalog
{
    private static readonly IReadOnlyList<AgentSymptomPrompt> DefaultPrompts =
    [
        new()
        {
            Id = "network",
            Kind = AgentSymptomKind.NetworkConnection,
            Label = "网络连不上",
            Question = "电脑连不上 Wi-Fi"
        },
        new()
        {
            Id = "sound",
            Kind = AgentSymptomKind.Sound,
            Label = "没有声音",
            Question = "电脑突然没有声音"
        },
        new()
        {
            Id = "bluetooth",
            Kind = AgentSymptomKind.BluetoothOrPeripheral,
            Label = "蓝牙或外设",
            Question = "蓝牙耳机一直连不上"
        },
        new()
        {
            Id = "display",
            Kind = AgentSymptomKind.Display,
            Label = "屏幕异常",
            Question = "显示器一直闪屏"
        },
        new()
        {
            Id = "driver",
            Kind = AgentSymptomKind.DriverOrDevice,
            Label = "驱动或设备",
            Question = "设备驱动显示异常"
        },
        new()
        {
            Id = "blue-screen",
            Kind = AgentSymptomKind.BlueScreenOrRestart,
            Label = "蓝屏或重启",
            Question = "电脑刚才蓝屏并自动重启"
        }
    ];

    public static IReadOnlyList<AgentSymptomPrompt> CreateDefault() => DefaultPrompts;
}

public static class AgentSymptomTriagePresenter
{
    private static readonly string[] ExplicitNavigationActions =
    [
        "打开", "进入", "查看", "带我去"
    ];
    private static readonly string[] ExplicitNavigationDestinations =
    [
        "网络设置", "wi-fi 设置", "wifi 设置", "蓝牙设置", "声音设置", "显示设置",
        "电源设置", "存储设置", "已安装应用", "启动应用", "任务管理器", "设备管理器",
        "事件查看器", "磁盘管理", "安全中心", "注册表编辑器", "回收站"
    ];
    private static readonly string[] ExplicitSettingsQuestions =
    [
        "在哪里设置", "在哪设置", "怎么设置", "设置在哪"
    ];
    private static readonly string[] ExplicitSettingsSubjects =
    [
        "wi-fi", "wifi", "网络", "蓝牙", "声音", "麦克风", "扬声器", "显示", "分辨率",
        "电源", "睡眠", "休眠", "存储", "应用", "启动"
    ];
    private static readonly string[] DriverSymptomTerms =
    [
        "驱动异常", "设备驱动", "黄色感叹号", "设备异常", "驱动报错"
    ];
    private static readonly string[] DisplaySymptomTerms =
    [
        "闪屏", "屏幕闪烁", "显示器闪烁", "显示器没信号", "黑屏", "画面异常", "显示异常"
    ];
    private static readonly string[] SoundSymptomTerms =
    [
        "没有声音", "没声音", "无声音", "麦克风没声", "声音断断续续", "扬声器不响"
    ];
    private static readonly string[] NetworkSymptomTerms =
    [
        "网络连不上", "连不上 wi-fi", "连不上wifi", "wi-fi 连不上", "wifi连不上",
        "wifi断开", "wi-fi断开", "断网", "上不了网", "网速很慢"
    ];
    private static readonly string[] PeripheralSubjects =
    [
        "蓝牙", "外设", "耳机", "鼠标", "键盘", "摄像头", "手柄"
    ];
    private static readonly string[] PeripheralFailureTerms =
    [
        "连不上", "连接失败", "找不到", "断开", "失灵", "不能用", "没反应", "异常"
    ];
    private static readonly string[] ContrastSeparators =
    [
        "但是", "不过", "只是", "而是", "却", "但"
    ];
    private static readonly char[] ClauseSeparators =
        [',', '，', '。', ';', '；', '!', '！', '?', '？'];

    public static AgentSymptomTriage? TryCreate(string? question)
    {
        var normalized = Normalize(question);
        if (normalized.Length == 0 || IsExplicitNavigationRequest(normalized))
            return null;

        if (HasAffirmedBlueScreen(normalized) || HasAffirmedRestart(normalized))
        {
            return Triage(
                AgentSymptomKind.BlueScreenOrRestart,
                "蓝屏或意外重启需要先保留错误线索",
                "你报告的是可能中断工作并造成未保存内容丢失的问题。现在还不能只凭描述判断根因是驱动、硬件还是 Windows 组件。",
                "已识别到蓝屏或意外重启现象；本次没有执行修复。",
                "还不知道停止代码、发生时间和 Windows 错误记录，因此不能判断根因。",
                "较紧急：先保存正在编辑的内容，避免反复重启。",
                "打开“事件查看器”，先查看故障时间附近是否有严重错误；不要删除日志。",
                AgentShortcutKind.SystemTool,
                "event-viewer",
                "打开事件查看器");
        }

        if (HasAffirmedSymptom(normalized, DriverSymptomTerms))
        {
            return Triage(
                AgentSymptomKind.DriverOrDevice,
                "驱动或设备异常先确认 Windows 是否识别它",
                "当前只有故障描述，还不能判断根因是驱动缺失、设备被禁用、连接问题还是硬件故障。",
                "已识别到驱动或设备异常描述；没有卸载、更新或禁用任何驱动。",
                "还不知道具体设备和 Windows 报告的状态。",
                "需要尽快查看：先确认设备状态，暂时不要批量更新驱动。",
                "打开“设备管理器”，找到有提示的设备，只查看设备状态和错误代码。",
                AgentShortcutKind.SystemTool,
                "device-manager",
                "打开设备管理器");
        }

        if (HasAffirmedSymptom(normalized, DisplaySymptomTerms))
        {
            return Triage(
                AgentSymptomKind.Display,
                "屏幕异常先确认 Windows 当前显示状态",
                "闪屏、黑屏或无信号可能来自连接、显示设置或驱动；仅凭描述还不能确定原因。",
                "已识别到显示异常描述；没有更改分辨率、缩放或多屏布局。",
                "还不知道 Windows 是否识别显示器，也没有读取线缆、刷新率或驱动状态。",
                "需要尽快查看：如果画面仍可操作，先保留当前设置。",
                "打开“显示”设置，先确认 Windows 是否识别了正确数量的显示器。",
                AgentShortcutKind.WindowsSettings,
                "display",
                "打开显示设置");
        }

        if (HasAffirmedSymptom(normalized, SoundSymptomTerms))
        {
            return Triage(
                AgentSymptomKind.Sound,
                "没有声音先确认当前输入输出设备",
                "声音问题常见于输出设备选错、静音或设备未被 Windows 识别，但当前没有证据证明是哪一种。",
                "已识别到声音异常描述；没有更改音量、麦克风或扬声器。",
                "还不知道当前默认输出设备、音量和设备状态。",
                "建议先检查：这通常不需要立即修改驱动。",
                "打开“声音”设置，先确认输出设备名称和音量是否正确。",
                AgentShortcutKind.WindowsSettings,
                "sound",
                "打开声音设置");
        }

        if (HasPeripheralFailure(normalized))
        {
            return Triage(
                AgentSymptomKind.BluetoothOrPeripheral,
                "蓝牙或外设问题先确认设备是否被 Windows 看到",
                "当前不能判断是未配对、已断开、电量不足、驱动异常还是设备自身问题。",
                "已识别到蓝牙或外设连接问题；没有配对、删除或关闭任何设备。",
                "还不知道设备是否出现在 Windows 列表、是否已连接或是否有状态提示。",
                "建议先检查：先看连接状态，不要删除设备后反复重装。",
                "打开“蓝牙 / 设备”设置，先确认目标设备是否出现以及当前连接状态。",
                AgentShortcutKind.WindowsSettings,
                "bluetooth",
                "打开蓝牙 / 设备设置");
        }

        if (HasAffirmedSymptom(normalized, NetworkSymptomTerms))
        {
            return Triage(
                AgentSymptomKind.NetworkConnection,
                "网络问题先确认 Windows 看到的连接状态",
                "当前还不能判断是 Wi-Fi、网线、路由器、账号还是网络配置导致，先从不会改变配置的状态页开始。",
                "已识别到网络连接异常描述；没有切换网络、重置适配器或修改配置。",
                "还不知道 Windows 是否显示已连接，也没有读取路由器或网络账号状态。",
                "建议先检查：如果只有这台电脑断网，先看本机状态。",
                "打开“网络 / Wi-Fi”设置，先确认页面顶部是否显示已连接。",
                AgentShortcutKind.WindowsSettings,
                "network",
                "打开网络 / Wi-Fi 设置");
        }

        return null;
    }

    private static bool IsExplicitNavigationRequest(string question) =>
        Clauses(question).Any(clause =>
            IsExplicitSettingsQuestion(clause)
            || ExplicitNavigationActions.Any(action =>
                ExplicitNavigationDestinations.Any(destination =>
                    HasNearbyOrderedTerms(clause, action, destination))));

    private static bool IsExplicitSettingsQuestion(string clause) =>
        !ContainsAny(clause, ["没怎么设置", "没有怎么设置", "并没怎么设置"])
        && ContainsAny(clause, ExplicitSettingsQuestions)
        && ContainsAny(clause, ExplicitSettingsSubjects);

    private static bool HasNearbyOrderedTerms(
        string question,
        string first,
        string second)
    {
        var searchFrom = 0;
        while (searchFrom < question.Length)
        {
            var firstIndex = question.IndexOf(first, searchFrom, StringComparison.OrdinalIgnoreCase);
            if (firstIndex < 0)
                return false;

            var secondIndex = question.IndexOf(
                second,
                firstIndex + first.Length,
                StringComparison.OrdinalIgnoreCase);
            if (secondIndex >= 0 && secondIndex - (firstIndex + first.Length) <= 4)
                return true;

            searchFrom = firstIndex + first.Length;
        }

        return false;
    }

    private static bool HasPeripheralFailure(string question) =>
        Clauses(question).Any(clause =>
            ContainsAny(clause, PeripheralSubjects)
            && HasAffirmedSymptom(clause, PeripheralFailureTerms));

    private static bool HasAffirmedBlueScreen(string question) =>
        HasAffirmedSymptom(question, ["蓝屏"]);

    private static bool HasAffirmedRestart(string question) =>
        HasAffirmedSymptom(question, ["自动重启", "突然重启", "反复重启"]);

    private static bool HasAffirmedSymptom(
        string question,
        IEnumerable<string> symptomTerms) =>
        Clauses(question).Any(clause =>
            symptomTerms.Any(term => HasAffirmedTerm(clause, term)));

    private static bool HasAffirmedTerm(string clause, string term)
    {
        var searchFrom = 0;
        while (searchFrom < clause.Length)
        {
            var index = clause.IndexOf(term, searchFrom, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;

            var prefixStart = Math.Max(0, index - 6);
            var prefix = clause[prefixStart..index].TrimEnd();
            var suffixStart = index + term.Length;
            var suffixLength = Math.Min(8, clause.Length - suffixStart);
            var suffix = clause.Substring(suffixStart, suffixLength).TrimStart();
            var negatedBefore = EndsWithAny(
                prefix,
                ["不是", "并不是", "并非", "没有出现", "没出现", "未出现", "没有发生", "没发生", "未发生", "没有", "没"]);
            var negatedAfter = StartsWithAny(
                suffix,
                ["倒是没有", "倒没有", "并没有", "也没有", "没有", "并不存在", "不存在"]);
            if (!negatedBefore && !negatedAfter)
                return true;

            searchFrom = index + term.Length;
        }

        return false;
    }

    private static IEnumerable<string> Clauses(string question)
    {
        var separated = question;
        foreach (var separator in ContrastSeparators)
            separated = separated.Replace(separator, "，", StringComparison.OrdinalIgnoreCase);

        return separated.Split(
            ClauseSeparators,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool EndsWithAny(string value, IEnumerable<string> candidates) =>
        candidates.Any(candidate => value.EndsWith(candidate, StringComparison.OrdinalIgnoreCase));

    private static bool StartsWithAny(string value, IEnumerable<string> candidates) =>
        candidates.Any(candidate => value.StartsWith(candidate, StringComparison.OrdinalIgnoreCase));

    private static AgentSymptomTriage Triage(
        AgentSymptomKind kind,
        string headline,
        string summary,
        string checkedSummary,
        string unknownSummary,
        string urgencySummary,
        string primaryNextStep,
        AgentShortcutKind shortcutKind,
        string shortcutId,
        string navigationLabel) =>
        new()
        {
            Kind = kind,
            Headline = headline,
            Summary = summary,
            CheckedSummary = checkedSummary,
            UnknownSummary = unknownSummary,
            UrgencySummary = urgencySummary,
            PrimaryNextStep = primaryNextStep,
            ShortcutKind = shortcutKind,
            ShortcutId = shortcutId,
            NavigationLabel = navigationLabel
        };

    private static bool ContainsAny(string value, IEnumerable<string> candidates) =>
        candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
}
