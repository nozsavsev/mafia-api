namespace _Mafia_API
{
    public static class HttpContextExtensions
    {
        public static string? MafiaUser(this HttpContext context)
        {
            return context.Items?["UserID"] as string;

        }
    }
}
