using RagChatPoC.Api.Utils;

namespace RagChatPoC.Api.UnitTests.Utils;

public class TextSanitizerTests
{
    [Fact]
    public void CleanTextForPostgres_ShouldReturnEmptyString_WhenInputIsNull()
    {
        var result = TextSanitizer.CleanTextForPostgres(null);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void CleanTextForPostgres_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        var result = TextSanitizer.CleanTextForPostgres(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void CleanTextForPostgres_ShouldRemoveControlCharactersExceptAllowedOnes()
    {
        var input = "Valid\nText\rWith\tControl\u0001Characters";
        var expected = "Valid\nText\rWith\tControlCharacters";

        var result = TextSanitizer.CleanTextForPostgres(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CleanTextForPostgres_ShouldRemoveNullByteCharacters()
    {
        var input = "TextWith\0NullByte";
        var expected = "TextWithNullByte";

        var result = TextSanitizer.CleanTextForPostgres(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CleanTextForPostgres_ShouldReturnSameString_WhenNoInvalidCharactersExist()
    {
        var input = "This is a valid string.";
        var result = TextSanitizer.CleanTextForPostgres(input);

        Assert.Equal(input, result);
    }
}