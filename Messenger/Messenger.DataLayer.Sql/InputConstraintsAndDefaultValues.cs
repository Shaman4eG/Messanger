namespace Messenger.DataLayer.Sql
{
    public static class InputConstraintsAndDefaultValues
    {
        // DB connection string
        public static readonly string ConnectionString = "Server=DANIEL;Database=Messenger;Trusted_Connection=true";

        // Image size in bytes (2 MBs)
        public static readonly int MaxImageSize = 2097152;

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

        // Attachment
        // In bytes. Max == 100 MBs
        public static readonly int MinFileSize = 1;
        public static readonly int MaxFileSize = 104857600;
        public static readonly int MinFileTypeLenght = 1;
        public static readonly int MaxFileTypeLength = 25;
    }
}
