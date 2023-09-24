using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Extensions.Attributes;

public class MaxItemCountAttribute : ValidationAttribute
{
    private readonly int _maxCount;

    public MaxItemCountAttribute(int maxCount)
    {
        _maxCount = maxCount;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is ICollection collection && collection.Count > _maxCount)
        {
            var formattedErrorMessage = string.Format(ErrorMessageString, _maxCount);
            return new ValidationResult(formattedErrorMessage);
        }
        return ValidationResult.Success;
    }
}
