namespace PriorAuth.AuthEngine.Models;

public static class FailureReasons
{
    public const string MissingField = "Required field is missing from clinical data.";

    public static string BooleanRequirementNotMet(string field) =>
        $"{field} is required but was not confirmed.";

    public static string ThresholdNotMet(string field, object required, object actual) =>
        $"{field} does not meet the minimum requirement of {required} (submitted: {actual}).";

    public static string OrderedThresholdNotMet(string field, string required, string actual) =>
        $"{field} must be at least '{required}' (submitted: '{actual}').";

    public static string ValueRequired(string field) =>
        $"{field} must have a value selected.";
}
