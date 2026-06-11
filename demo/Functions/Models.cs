namespace Functions;

public record LocationCreationResult(
    string Location,
    string ResourceName,
    string Status,
    DateTimeOffset CreatedAt);

public record ApprovalDecisionRequest(string? Decision);

public record ApprovalDecision(string Decision);

public record CreationApprovalStatus(
    string Status,
    string WaitingForEvent,
    LocationCreationResult[] Results);

public record CreationWorkflowResult(
    string Status,
    string Message,
    LocationCreationResult[] Results);

public static class ApprovalDecisions
{
    public const string Ok = "OK";
    public const string Ng = "NG";

    public static bool TryNormalize(string? decision, out string normalized)
    {
        normalized = decision?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalized is Ok or Ng;
    }
}
