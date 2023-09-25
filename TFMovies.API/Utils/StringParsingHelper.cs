namespace TFMovies.API.Utils;

public class StringParsingHelper
{
    public static IEnumerable<string> ParseDelimitedValues(string input)
    {
        var result = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrEmpty(value));

        return result;
    }
}
