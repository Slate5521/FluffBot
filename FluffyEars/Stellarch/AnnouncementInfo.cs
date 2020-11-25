using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BigSister
{
    public class AnnouncementInfo
    {
        [JsonIgnore] const string FORMAT_RECEIVE = @"yyyy-MM-dd HH:mm";
        [JsonIgnore] const string FORMAT_BASICTIME = @"h tt";
        [JsonIgnore] const int PST_OFFSET = -8;
        [JsonIgnore] const int EST_OFFSET = -5;

        public string time;
        public string end;
        public string username;
        public string @override;

        [JsonIgnore]
        public DateTimeOffset PostTime
        {
            get => DateTimeOffset.ParseExact(time, FORMAT_RECEIVE, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        [JsonIgnore]
        public DateTimeOffset EndTime
        {
            get => DateTimeOffset.ParseExact(end, FORMAT_RECEIVE, CultureInfo.InvariantCulture.DateTimeFormat);
        }

        string GetTwitchLink()
            => $"{@"https://twitch.tv/"}{username}";

        string GetTimeEnd()
        {
            var endTime = EndTime;
            var stringBuilder = new StringBuilder();

            // PST
            stringBuilder.Append(endTime.ToOffset(TimeSpan.FromHours(PST_OFFSET)).ToString(FORMAT_BASICTIME));
            stringBuilder.Append('/');

            // EST
            stringBuilder.Append(endTime.ToOffset(TimeSpan.FromHours(EST_OFFSET)).ToString(FORMAT_BASICTIME));
            stringBuilder.Append('/');

            // EST
            stringBuilder.Append(endTime.ToString(FORMAT_BASICTIME));
            stringBuilder.Append(@" (PST/EST/UTC)");

            return stringBuilder.ToString();
        }

        public string GetContent(string roleMention)
        {
            string @return;

            if (!(@override is null) && @override.Length > 0)
                @return = $"{roleMention} {@override} {GetTwitchLink()}";
            else
                @return = $"{roleMention}\n:fire::potato: The potato has been passed! :potato::fire: **{username}** is now carrying the potato until {GetTimeEnd()}! Catch the drama at {GetTwitchLink()}";

            return @return;
        }

        public override bool Equals(object obj)
        {
            return obj is AnnouncementInfo info &&
                   time.Equals(info.time) &&
                   end.Equals(info.end) &&
                   username == info.username;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(time, end, username);
        }
    }
}
