using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyEars.Spam
{
    public enum SpamType
    {
        OverLimit, // Over the limit (default >=800).
        Linesplit, // Linesplitting (default => 5 linesplits in a message).
        Overflow   // Sending messages too quickly (default => 3 messages in a second).
    }
}
