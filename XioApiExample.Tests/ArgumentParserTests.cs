using XioApiExample.Services;

namespace XioApiExample.Tests;

public class ArgumentParserTests
{
    [Test]
    public void Parse_SeparateValues_Succeeds()
    {
        var result = ArgumentParser.Parse(["--api-key", "k1", "--account-id", "a1"]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Options!.ApiKey, Is.EqualTo("k1"));
        Assert.That(result.Options.AccountId, Is.EqualTo("a1"));
    }

    [Test]
    public void Parse_EqualsSyntax_Succeeds()
    {
        var result = ArgumentParser.Parse(["--api-key=k1", "--account-id=a1"]);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Options!.AccountId, Is.EqualTo("a1"));
    }

    [Test]
    public void Parse_NamesAreCaseInsensitive()
    {
        var result = ArgumentParser.Parse(["--API-KEY", "k1", "--Account-Id", "a1"]);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void Parse_MissingApiKey_Fails()
    {
        var result = ArgumentParser.Parse(["--account-id", "a1"]);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("--api-key"));
    }

    [Test]
    public void Parse_MissingAccountId_Fails()
    {
        var result = ArgumentParser.Parse(["--api-key", "k1"]);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("--account-id"));
    }

    [Test]
    public void Parse_MissingValue_Fails()
    {
        var result = ArgumentParser.Parse(["--api-key", "--account-id", "a1"]);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("--api-key"));
    }

    [Test]
    public void Parse_UnknownArgument_Fails()
    {
        var result = ArgumentParser.Parse(["--api-key", "k1", "--account-id", "a1", "--bogus"]);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("--bogus"));
    }

    [Test]
    public void Options_ToString_DoesNotExposeApiKey()
    {
        var options = new AppOptions("super-secret", "a1");

        Assert.That(options.ToString(), Does.Not.Contain("super-secret"));
    }
}
