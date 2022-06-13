using System.Reflection;

using MDS.AppFramework.Controls;

using Microsoft.Extensions.Primitives;

namespace MDS.AppFramework.Common
{
    public static class ViewModelExtensions
    {
        public static async Task<ControlViewModel?> GetViewModelAsync(this HttpContext context, IViewState viewState, Type viewModelType)
        {
            if (viewModelType is null)
            {
                throw new ArgumentNullException(nameof(viewModelType));
            }

            dynamic? viewModel = 
                viewState.ViewState.TryGetValue(nameof(IAppView.ViewModel), out LazyContainer? vm) 
                ? await vm.GetLazyDataAsync<ControlViewModel>() 
                : Activator.CreateInstance(viewModelType);

            if (viewModel is not null)
            {
                PropertyInfo[]? properties = viewModelType.GetProperties();

                foreach (KeyValuePair<string, StringValues> item in context.Request.Form)
                {
                    PropertyInfo? pi = properties.FirstOrDefault(pi => pi.Name.Equals(item.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (pi != null)
                    {
                        SetProperty(pi, item.Value);
                    }
                }
            }

            Func<ControlViewModel?> func = () => viewModel;
            LazyContainer lazy = await LazyContainer.CreateLazyContainerAsync(func, viewModel);
            viewState.ViewState.AddOrUpdate(nameof(IAppView.ViewModel), lazy, (_,_) => lazy);

            return viewModel as ControlViewModel;

            void SetProperty(PropertyInfo pi, StringValues value)
            {
                object? toSet = pi.PropertyType.Name switch
                {
                    nameof(String) => value.ToString(),
                    nameof(Int32) => int.Parse(value.First()),
                    nameof(Int16) => short.Parse(value.First()),
                    nameof(Int64) => long.Parse(value.First()),
                    nameof(UInt32) => uint.Parse(value.First()),
                    nameof(UInt16) => ushort.Parse(value.First()),
                    nameof(UInt64) => ulong.Parse(value.First()),
                    nameof(Byte) => byte.Parse(value.First()),
                    nameof(SByte) => sbyte.Parse(value.First()),
                    nameof(Single) => float.Parse(value.First()),
                    nameof(Double) => double.Parse(value.First()),
                    nameof(Decimal) => decimal.Parse(value.First()),
                    nameof(DateTime) => DateTime.Parse(value.First()),
                    nameof(DateTimeOffset) => DateTimeOffset.Parse(value.First()),
                    nameof(TimeSpan) => TimeSpan.Parse(value.First()),
                    nameof(Boolean) => bool.Parse(value.First()),
                    nameof(TimeOnly) => TimeOnly.Parse(value.First()),
                    nameof(DateOnly) => DateOnly.Parse(value.First()),
                    _ => pi.PropertyType.IsInstanceOfType(typeof(IConvertible)) switch
                    {
                        true => Convert.ChangeType(value, pi.PropertyType),
                        _ => (object?)null
                    }
                };

                if (toSet is null && toSet is object)
                {
                    viewModel.Set(pi.Name, value);
                }
                else
                {
                    pi.SetValue(viewModel, toSet);
                }
            }
        }
    }
}