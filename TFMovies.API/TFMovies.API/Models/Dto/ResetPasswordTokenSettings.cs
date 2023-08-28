using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public class ResetPasswordTokenSettings : ISecretTokenSettings
{
    public int LifeTimeDuration { get; set; }
    public TimeUnitEnum LifeTimeUnit { get; set; }
}
