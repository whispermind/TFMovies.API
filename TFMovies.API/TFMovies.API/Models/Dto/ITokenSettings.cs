using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public interface ITokenSettings
{
    int LifeTimeDuration { get; }
    TimeUnitEnum LifeTimeUnit { get; }
}
