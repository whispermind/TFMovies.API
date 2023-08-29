using TFMovies.API.Common.Enum;

namespace TFMovies.API.Utils;

public static class TimeUtility
{
    public static DateTime AddTime(this DateTime dateTime, TimeUnitEnum timeUnit, int timeDuration)
    {
        return timeUnit switch
        {
            TimeUnitEnum.Seconds => dateTime.AddSeconds(timeDuration),
            TimeUnitEnum.Minutes => dateTime.AddMinutes(timeDuration),
            TimeUnitEnum.Hours => dateTime.AddHours(timeDuration),
            TimeUnitEnum.Days => dateTime.AddDays(timeDuration),
            _ => dateTime.AddMinutes(timeDuration) //default
        };
    }
}
