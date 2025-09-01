using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
            return Task.CompletedTask;

        // заменяем запятую на точку, чтобы не зависеть от локали
        value = value.Replace(',', '.');

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            bindingContext.Result = ModelBindingResult.Success(parsedValue);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Значение '{value}' недопустимо для поля {bindingContext.ModelName}."
            );
        }

        return Task.CompletedTask;
    }
}
