using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyEars.Spam
{
    public struct TimeoutInformation
    {
        public long TimeoutEndMilliseconds;
        public ulong UserId;

        public override bool Equals(object obj)
        {
            return obj is TimeoutInformation information &&
                   UserId == information.UserId;
        }
    }
}
