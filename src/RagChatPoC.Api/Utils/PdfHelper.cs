using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RagChatPoC.Api.Utils;

public static class PdfHelper
{
    public static string ExtractTextFromPdf(Stream pdfStream)
    {
        using var pdf = PdfDocument.Open(pdfStream);
        var sb = new StringBuilder();

        foreach (Page page in pdf.GetPages())
        {
            // foreach (var word in page.GetWords())
            // {
            //     sb.Append(word);
            // }
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }
}