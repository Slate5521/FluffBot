// ReminderSystem.cs
// Contains everything required to need the reminder system.

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyEars.Reminders
{
    /// <summary>A static class containing just about everything needed to run the Reminder system on RimWorld.</summary>
    public static class ReminderSystem
    {
        public const string BaseFile = @"reminders";
        private static SaveFile saveFile = new SaveFile(BaseFile);
        private static readonly object lockObj = (object)
            @"
        .--,_
       / ,/ /
      / // /
     / // /
   .'  ' (
  /       \.-''' -._
 / a   ' .    '    `-.
(       .  '      '   `.
 `-.-'       '       '  ;
     `.'  '  .  .-'    ' ;
      : .     .'          ;
      `.   ' :     '   '  ;
 jgs    )  _.'. '     .  ';
      .'_.'   .'   '  __.,`.
     ''      ''''---'`    ''";

        private static List<Reminder> reminders_ = new List<Reminder>();    // All the reminders loaded into memory.
        // All of the reminders from reminders_ but sorted. This is initially set to an empty array, and for the sake of not having a clusterfuck,
        // I may change how this is initially defined or even switch it over to a Property wherein Linq is used to return the sorted array when
        // called upon.
        private static Reminder[] remindersSorted
        {
            get
            {
                Reminder[] returnVal;

                if (reminders_ is null || reminders_.Count == 0)
                    returnVal = Array.Empty<Reminder>();
                else
                    returnVal = reminders_.OrderBy(a => a.Time).ToArray();

                return returnVal;
            }
        }

        /// <summary>Call this to set the Reminder list to default.</summary>
        public static void Default()
        {
            reminders_ = new List<Reminder>();
        }
        public static void Save() => saveFile.Save<List<Reminder>>(reminders_, lockObj);       

        public static bool CanLoad() => saveFile.IsExistingSaveFile();

        public static void Load()
        {
            if (CanLoad())
                reminders_ = saveFile.Load<List<Reminder>>(lockObj);
            else Default();
        }

        /// <summary>Get a Reminder from its Id.</summary>
        public static Reminder GetReminderFromId(string reminderId) => reminders_.Single(a => a.GetIdentifier() == reminderId);

        /// <summary>Check if there are any Reminders in queue.</summary>
        /// <returns>True if there are notifications.</returns>
        public static bool HasNotification()
        {
            bool returnVal = false;

            // If the arrays are null, there are no reminders.
            if (remindersSorted is null || reminders_ is null)
                returnVal = false;
            else
            {   // If there's anything in the array, there's a reminder.
                if (remindersSorted.Length > 0)
                    returnVal = true;
            }

            return returnVal;
        }

        

        /// <summary>Get the soonest Reminder.</summary>
        /// <returns>The soonest Reminder.</returns>
        public static Reminder GetSoonestNotification()
        {
            Reminder returnVal;

            if (remindersSorted is null || remindersSorted.Length > 0)
                returnVal = remindersSorted[0];
            else
                throw new IndexOutOfRangeException();

            return returnVal;
        }
        
        /// <summary>Add a reminder to the list</summary>
        public static void AddReminder(Reminder reminder)
        {
            reminders_.Add(reminder);
        }

        /// <summary>Remove an existing reminder from the list.</summary>
        public static void RemoveReminder(Reminder reminder)
        {
            if (reminders_.Contains(reminder))
                reminders_.Remove(reminder);
        }
        
        /// <summary>Get a list of all the reminders.</summary>
        public static Reminder[] GetReminders() => reminders_.ToArray();
        
        /// <summary>Check if any Reminder with the specified ReminderID is in the list.</summary>
        /// <param name="reminderId">The Reminder Identifier</param>
        /// <returns>True if the Reminder exists.</returns>
        public static bool IsReminder(string reminderId) => 
            reminders_.Any(a => a.GetIdentifier() == reminderId);

        /// <summary>Check if the specified Reminder is in the list.</summary>
        /// <returns>True if the Reminder exists.</returns>
        public static bool IsReminder(Reminder reminder) => IsReminder(reminder.GetIdentifier());

        /// <summary>Called when the bot is heartbeated.</summary>
        internal static async Task BotClient_Heartbeated(HeartbeatEventArgs e)
        {
            Reminder curReminder;                   // The reminder currently being inspected within the loop.
            Reminder prevReminder = new Reminder(); // The reminder from the previous step of the loop. Defaults to a default Reminder object
                                                    // because a comparison will be done between curReminder and prevReminder, and theoretically
                                                    // there should never be a Reminder object stored in memory that is the default object.

            // Check if there's any notifications. If there are, set the curReminder to the most present reminder before entering the loop.
            if (ReminderSystem.HasNotification())
                curReminder = ReminderSystem.GetSoonestNotification();
            else return; // Note to self: I hate using return in the middle of a method due to optimization reasons. If I can change this later,
                         // I would love that.

            // Bit confusing what I'm doing here. So, basically...
            // 
            // Every time a reminder has notified, it is immediately popped from the list. Therefore, if a reminder from the previous 
            // step is equal to the reminder in the current step, that means the soonest reminder hasn't actually been notified. Therefore,
            // anything later doesn't need to be checked. On the contrary, if a reminder from the previous step is not equal to the 
            // reminder in the current step, that means we should continually check until we find a reminder that isn't ready to be
            // notified.
            while (ReminderSystem.HasNotification() && !curReminder.Equals(prevReminder))
            {
                DateTimeOffset reminderTime = DateTimeOffset.FromUnixTimeMilliseconds(curReminder.Time);

                DateTimeOffset utcNow = DateTimeOffset.UtcNow;

                // If the Reminder's time is NOW or PAST, enter this scope.
                if (utcNow.Ticks >= reminderTime.Ticks)
                {
                    StringBuilder sb = new StringBuilder();
                    // How late the notification is. Necessary for two reasons:
                    // 1 - The bot may, for unknown reasons in the future, have some sort of extended downtime, so we should keep track of lateness.
                    // 2 - Reminders are only checked every bot heartbeat, so a notification can be anywhere from 40-60 seconds late.
                    TimeSpan lateBy = utcNow.Subtract(reminderTime);

                    DiscordChannel chan = await Bot.BotClient.GetChannelAsync(curReminder.Channel);

                    // DEB!
                    DiscordEmbedBuilder deb = new DiscordEmbedBuilder();
                    deb.WithTitle("Notification");
                    deb.WithDescription(curReminder.Text);
                    deb.AddField(@"Late by", 
                        String.Format("{0}day {1}hr {2}min {3}sec", lateBy.Days, lateBy.Hours, lateBy.Minutes, lateBy.Seconds));
                    deb.WithThumbnail(ChatObjects.URL_REMINDER_EXCLAIM);

                    // Get all the people we need to remind.
                    sb.Append(String.Format("<@{0}> ", curReminder.User));
                    Array.ForEach(curReminder.UsersToNotify,            // For every user (a), append them to sb in mention format <@id>.
                        a => sb.Append(String.Format("<@{0}> ", a)));

                    await Bot.BotClient.SendMessageAsync(
                        channel: chan,
                        content: sb.ToString(),
                        embed: deb);

                    // Remove the reminder from the list.
                    ReminderSystem.RemoveReminder(curReminder);
                    ReminderSystem.Save();
                }

                // The current reminder will become the next step's previous reminder, and we should also grab the next reminder before heading into
                // the next step.
                prevReminder = curReminder;

                if (ReminderSystem.HasNotification())
                    curReminder = ReminderSystem.GetSoonestNotification();
                else break;
            }
        }
    }
}