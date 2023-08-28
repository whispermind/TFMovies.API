using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public class ConfirmEmailTokenSettings : ISecretTokenSettings
{
    public int LifeTimeDuration { get; set; }
    public TimeUnitEnum LifeTimeUnit { get; set; }
}
