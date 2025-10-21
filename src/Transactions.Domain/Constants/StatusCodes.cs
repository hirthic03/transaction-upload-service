namespace Transactions.Domain.Constants;

public static class StatusCodes
{
    public const string Approved = "A";
    public const string Rejected = "R";
    public const string Done = "D";

    public static readonly IReadOnlyDictionary<string, string> StatusMappings = new Dictionary<string, string>
    {
        ["Approved"] = Approved,
        ["Failed"] = Rejected,
        ["Rejected"] = Rejected,
        ["Finished"] = Done,
        ["Done"] = Done
    };
}