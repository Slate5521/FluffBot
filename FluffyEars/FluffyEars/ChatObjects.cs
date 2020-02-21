// ChatObjects.cs
// Various objects to be sent to chat.

namespace FluffyEars
{
    public static class ChatObjects
    {
        const string ERROR_PREFIX = @":x: **/(u︵u)\\**";
        const string SUCCESS_PREFIX = @":white_check_mark: **/(^‿^)\\**";
        const string NEUTRAL_PREFIX = @":speech_left: **/('▿')\\**";

        public const string URL_SPEECH_BUBBLE =    @"https://i.imgur.com/NAHI4h3.png";
        public const string URL_FILTER_ADD =       @"https://i.imgur.com/cVeWPNz.png";
        public const string URL_FILTER_SUB =       @"https://i.imgur.com/kNdFBoK.png";
        public const string URL_FILTER_SPAM =      @"https://i.imgur.com/jh741I7.png";
        public const string URL_FILTER_BUBBLE =    @"https://i.imgur.com/TAaRDgI.png";
        public const string URL_FILTER_GENERIC =   @"https://i.imgur.com/58C8Noh.png";
        public const string URL_REMINDER_GENERIC = @"https://i.imgur.com/Ya1Lu6e.png";
        public const string URL_REMINDER_EXCLAIM = @"https://i.imgur.com/NUCrbSl.png";
        public const string URL_REMINDER_DELETED = @"https://i.imgur.com/OnTaJdd.png";


        public static string GetSuccessMessage(string message)
            => SUCCESS_PREFIX + ' ' + message;
        public static string GetErrMessage(string message)
            => ERROR_PREFIX + ' ' + message;
        public static string GetNeutralMessage(string message)
            => NEUTRAL_PREFIX + ' ' + message;
    }
}
