namespace Messenger.Constants
{
    public static class AttachmentInputConstraints
    {
        // In bytes. Max == 100 MBs
        public static readonly int MinFileSize = 1;
        public static readonly int MaxFileSize = 104857600;
        public static readonly int MinFileTypeLenght = 1;
        public static readonly int MaxFileTypeLength = 25;
    }
}
