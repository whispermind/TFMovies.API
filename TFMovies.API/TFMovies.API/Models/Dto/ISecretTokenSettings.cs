using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public interface ISecretTokenSettings
{
    int LifeTimeDuration { get; }
    TimeUnitEnum LifeTimeUnit { get; }
}
