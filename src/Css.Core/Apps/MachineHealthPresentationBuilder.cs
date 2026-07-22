namespace Css.Core.Apps;

public static class MachineHealthPresentationBuilder
{
    public static IReadOnlyList<HealthDimensionResult> CreateDimensions(
        MachineHealthObservation? observation)
    {
        if (observation is null)
        {
            return
            [
                Unavailable("D 盘空间", "本次未读取到本地 D 盘状态"),
                Unavailable("内存占用", "本次未读取到内存状态"),
                Unavailable("电池状态", "本次未读取到电池状态")
            ];
        }

        return
        [
            SecondaryDrive(observation.SecondaryDrive),
            Memory(observation.Memory, observation.ProcessCount),
            Battery(observation.Battery)
        ];
    }

    public static bool HasAnyMachineStateValue(MachineHealthObservation observation) =>
        observation.SecondaryDrive.Availability != MachineMetricAvailability.Unavailable
        || observation.Memory.Availability != MachineMetricAvailability.Unavailable
        || observation.Battery.Availability != MachineMetricAvailability.Unavailable
        || observation.ProcessCount is not null;

    private static HealthDimensionResult SecondaryDrive(LocalDriveHealthObservation drive)
    {
        if (drive.Availability == MachineMetricAvailability.NotPresent)
            return Unavailable("D 盘空间", "未发现可用的本地固定 D 盘", "未配置");
        if (drive.Availability != MachineMetricAvailability.Available || drive.TotalBytes <= 0)
            return Unavailable("D 盘空间", "本次未读取到本地 D 盘状态");

        var freeBytes = Math.Clamp(drive.FreeBytes, 0, drive.TotalBytes);
        var usedPercent = (double)(drive.TotalBytes - freeBytes) / drive.TotalBytes * 100;
        return new HealthDimensionResult
        {
            Name = "D 盘空间",
            Result = $"已使用 {usedPercent:0.0}%，剩余 {FormatBytes(freeBytes)}",
            Rating = usedPercent >= 90 ? "需要关注" : usedPercent >= 80 ? "有优化空间" : "正常"
        };
    }

    private static HealthDimensionResult Memory(MemoryHealthObservation memory, int? processCount)
    {
        if (memory.Availability != MachineMetricAvailability.Available || memory.TotalBytes <= 0)
            return Unavailable("内存占用", "本次未读取到内存状态");

        var available = Math.Clamp(memory.AvailableBytes, 0, memory.TotalBytes);
        var used = memory.TotalBytes - available;
        var load = Math.Clamp(memory.LoadPercent, 0, 100);
        var processes = processCount is >= 0 ? $"，{processCount.Value} 个进程" : string.Empty;
        return new HealthDimensionResult
        {
            Name = "内存占用",
            Result = $"{FormatBytes(used)}/{FormatBytes(memory.TotalBytes)}（{load}%）{processes}",
            Rating = load >= 85 ? "本次偏高" : load >= 75 ? "建议观察" : "正常"
        };
    }

    private static HealthDimensionResult Battery(BatteryHealthObservation battery)
    {
        if (battery.Availability == MachineMetricAvailability.NotPresent)
            return Unavailable("电池状态", "这台电脑未检测到电池", "不适用");
        if (battery.Availability != MachineMetricAvailability.Available)
            return Unavailable("电池状态", "本次未读取到电池状态");

        var charge = battery.ChargePercent is { } percent
            ? $"电量 {Math.Clamp(percent, 0, 100)}%"
            : "电量未知";
        var power = battery.IsCharging == true
            ? "，正在充电"
            : battery.IsOnAcPower == true
                ? "，已接通电源"
                : battery.IsOnAcPower == false
                    ? "，正在使用电池"
                    : string.Empty;
        var low = battery.ChargePercent is <= 15 && battery.IsOnAcPower == false;
        return new HealthDimensionResult
        {
            Name = "电池状态",
            Result = charge + power,
            Rating = low ? "电量较低" : "正常"
        };
    }

    private static HealthDimensionResult Unavailable(
        string name,
        string result,
        string rating = "未检测") =>
        new() { Name = name, Result = result, Rating = rating };

    private static string FormatBytes(long bytes)
    {
        if (bytes <= 0)
            return "0 B";
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return unit == 0 ? $"{bytes} B" : $"{value:0.0} {units[unit]}";
    }
}
