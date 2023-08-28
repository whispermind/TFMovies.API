using TFMovies.API.Common.Enum;

namespace TFMovies.API.Models.Dto;

public class TokenDurationSettings
{
    public int Value { get; set; }
    public TimeUnitEnum Unit { get; set; }
}
