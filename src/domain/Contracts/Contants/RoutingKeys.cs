namespace domain.Contracts.Contants;

public static class RoutingKeys
{
    public const string AWS_FILECHANGED = "aws.fileupdated";
    public const string AWS_FILEDELETED = "aws.filedeleted";

    public const string COMMANDS_ROUTE_KEY = "domain.command";
}
