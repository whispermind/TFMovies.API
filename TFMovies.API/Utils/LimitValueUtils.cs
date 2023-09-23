namespace TFMovies.API.Utils;

public static class LimitValueUtils
{
    public static int CheckLimitValue(int limit, int defaultValue)
    {
        if (limit <= 0)
        {
            limit = defaultValue;
        }

        return limit;
    }
}
