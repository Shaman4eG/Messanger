namespace Messenger.DataLayer.Sql
{
    static class InputRestrictionsAndDefaultValues
    {
        // User
        public static readonly int MinUserNameLength = 1;
        public static readonly int MaxUserNameLength = 25;
        public static readonly int MinUserLastNameLength = 1;
        public static readonly int MaxUserLastNameLength = 25;
        public static readonly int MinUserEmailLength = 1;
        public static readonly int MaxUserEmailLength = 254;
        public static readonly int MinUserPasswordLength = 1;
        public static readonly int MaxUserPasswordLength = 25;
        public static readonly byte[] DefaultUserAvatar = new byte[0];

        // Chat
        public static readonly string DefaultChatName = "New Chat";
        public static readonly byte[] DefaultChatAvatar = new byte[0];
        public static readonly int MinChatNameLength = 1;
        public static readonly int MaxChatNameLength = 50;
        public static readonly int MinNumberOfChatMembers = 1;
        public static readonly int MaxNumberOfChatMembers = 10;

        // Message
        public static readonly int MinMessageTextLength = 1;
        public static readonly int MaxMessageTextLength = 7500;
    }
}
