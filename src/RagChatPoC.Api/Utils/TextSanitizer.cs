using System.Text;

namespace RagChatPoC.Api.Utils;

public static class TextSanitizer
{
    public static string CleanTextForPostgres(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            // Ausschluss von ungültigen Steuerzeichen, insbesondere 0x00
            if (char.IsControl(ch) && ch != '\n' && ch != '\r' && ch != '\t')
                continue;

            // Ausschluss explizit von Null-Byte (char = 0)
            if (ch == '\0')
                continue;

            builder.Append(ch);
        }

        return builder.ToString();
    }
}