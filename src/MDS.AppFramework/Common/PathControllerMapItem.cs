namespace MDS.AppFramework.Common;

public record struct PathControllerMapItem(String Path, Type? ControllerType, string? Method)
{
    public static implicit operator (String, Type?, string?)(PathControllerMapItem value)
        => (value.Path, value.ControllerType, value.Method);

    public static implicit operator PathControllerMapItem((String, Type?, string?) value)
        => new(value.Item1, value.Item2, value.Item3);
}
