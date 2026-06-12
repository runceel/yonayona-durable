namespace Functions;

public record LocationCreationResult(
    string Location,
    string ResourceName,
    string Status,
    DateTimeOffset CreatedAt);

public record ApprovalDecisionRequest(bool? IsApproved);

public record ApprovalDecision(bool IsApproved);

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
    public static bool TryNormalize(bool? decision, out bool normalized)
    {
        if (decision == null)
        {
            normalized = false;
            return false;
        }

        normalized = decision.Value;
        return true;
    }
       
}
