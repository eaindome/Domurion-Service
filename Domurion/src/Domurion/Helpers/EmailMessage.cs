namespace Domurion.Helpers
{
    public record EmailMessage(string To, string Subject, string Body, bool IsHtml = true);
}
