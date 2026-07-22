namespace Css.Core.Apps;

public sealed class AppDrawerTechnicalDetailsState
{
    public required bool IsVisible { get; init; }
    public required string ButtonText { get; init; }
    public required string StatusText { get; init; }
}

public static class AppDrawerTechnicalDetailsPresenter
{
    public static AppDrawerTechnicalDetailsState Collapsed() =>
        new()
        {
            IsVisible = false,
            ButtonText = "\u67e5\u770b\u6280\u672f\u8be6\u60c5",
            StatusText = string.Empty
        };

    public static AppDrawerTechnicalDetailsState Toggle(bool isCurrentlyVisible) =>
        isCurrentlyVisible
            ? new AppDrawerTechnicalDetailsState
            {
                IsVisible = false,
                ButtonText = "\u67e5\u770b\u6280\u672f\u8be6\u60c5",
                StatusText = "\u5df2\u9690\u85cf\u6280\u672f\u8be6\u60c5\uff0c\u4e3b\u754c\u9762\u7ee7\u7eed\u53ea\u663e\u793a\u7ed3\u8bba\u548c\u5efa\u8bae\u3002"
            }
            : new AppDrawerTechnicalDetailsState
            {
                IsVisible = true,
                ButtonText = "\u9690\u85cf\u6280\u672f\u8be6\u60c5",
                StatusText = "\u5df2\u5c55\u5f00\u6280\u672f\u8be6\u60c5\uff1b\u8fd9\u4e9b\u53ea\u662f\u8bc1\u636e\uff0c\u4e0d\u4f1a\u76f4\u63a5\u4fee\u6539\u7cfb\u7edf\u3002"
            };
}
