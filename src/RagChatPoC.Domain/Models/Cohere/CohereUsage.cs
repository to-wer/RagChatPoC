namespace RagChatPoC.Domain.Models.Cohere;

public class CohereUsage
{
    public CohereBilledUnits BilledUnits { get; set; } = null!;
    public CohereTokens Tokens { get; set; } = null!;
}