namespace MDS.AppFramework.Common;

public record LazyContainer
{
    private readonly Lazy<object?> _data;

    private LazyContainer(Func<object?> dataFactory)
    {
        _data = new Lazy<object?>(dataFactory);
    }

    public static Task<LazyContainer> CreateLazyContainerAsync<TData>(Func<TData> dataFactory, TData data)
    {
        LazyContainer? container = new LazyContainer(() => dataFactory());

        return Task.FromResult(container);
    }

    public Task<TData?> GetLazyDataAsync<TData>()
        => _data.Value is TData data
            ? Task.FromResult((TData?)data)
            : Task.FromResult(default(TData?));
}