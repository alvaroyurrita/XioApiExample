namespace XioApiExample.Services;

public sealed record AppOptions(string ApiKey, string AccountId)
{
    // Never expose the API key when the record is printed.
    public override string ToString() => $"AppOptions {{ ApiKey = ****, AccountId = {AccountId} }}";
}

public sealed record ArgumentParseResult(AppOptions? Options, string? Error)
{
    public bool Success => Options is not null;
}

public static class ArgumentParser
{
    public const string Usage =
        "Usage: XioApiExample --api-key <API_KEY> --account-id <ACCOUNT_ID>";

    public static ArgumentParseResult Parse(IReadOnlyList<string> args)
    {
        string? apiKey = null;
        string? accountId = null;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            string name = arg;
            string? value = null;

            var eq = arg.IndexOf('=');
            if (arg.StartsWith("--", StringComparison.Ordinal) && eq > 0)
            {
                name = arg[..eq];
                value = arg[(eq + 1)..];
            }

            switch (name.ToLowerInvariant())
            {
                case "--api-key":
                case "--account-id":
                    if (value is null)
                    {
                        if (i + 1 >= args.Count || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                        {
                            return Fail($"Missing value for {name}.");
                        }
                        value = args[++i];
                    }
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return Fail($"Missing value for {name}.");
                    }
                    if (name.Equals("--api-key", StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = value.Trim();
                    }
                    else
                    {
                        accountId = value.Trim();
                    }
                    break;
                default:
                    return Fail($"Unknown argument '{name}'.");
            }
        }

        if (apiKey is null)
        {
            return Fail("--api-key is required.");
        }
        if (accountId is null)
        {
            return Fail("--account-id is required.");
        }

        return new ArgumentParseResult(new AppOptions(apiKey, accountId), null);
    }

    private static ArgumentParseResult Fail(string error) => new(null, error);
}
