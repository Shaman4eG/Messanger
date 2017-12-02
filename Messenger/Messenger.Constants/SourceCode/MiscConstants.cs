using System.Net;

namespace Messenger.Constants
{
    public static class MiscConstants
    {
        // DB connection string
        public static readonly string ConnectionString = "Server=DANIEL;Database=Messenger;Trusted_Connection=true";

        // Extra http status codes
        public static readonly HttpStatusCode UnprocessableEntity = (HttpStatusCode)422;
    }
}
