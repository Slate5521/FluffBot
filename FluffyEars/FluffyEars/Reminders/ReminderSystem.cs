// ReminderSystem.cs
// Contains everything required to need the reminder system.

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
        /// <summary>
        /// The JSON File containing the reminders.
        /// </summary>
        /// <remarks>When I implement the anti-corrupt save system, let's remove this.</remarks>
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
        private static Reminder[] remindersSorted = reminders_.ToArray();  

        /// <summary>Call this whenever the list is altered.</summary>
        private static void ListAltered()
        {
            // So, if reminders_ is null, we're gonna wanna initiate it and its paired sorted array.
            if (reminders_ is null)
            {
                reminders_ = new List<Reminder>();
                remindersSorted = new Reminder[0];
            }

            // Great! So if they are not initiated, it is now, and we have the simple task of arranging the list to be in chronological order.
            if(reminders_.Count > 0)
                remindersSorted = reminders_.OrderBy(a => a.Time).ToArray();
            else
                remindersSorted = new Reminder[0];

        }

        /// <summary>Call this to set the Reminder list to default.</summary>
        public static void Default()
        {
            reminders_ = new List<Reminder>();
            remindersSorted = new Reminder[0];
        }
        public static void Save() => saveFile.Save<List<Reminder>>(reminders_, lockObj);       

        public static bool CanLoad() => saveFile.IsExistingSaveFile();

        public static void Load()
        {
            if (CanLoad())
                reminders_ = saveFile.Load<List<Reminder>>(lockObj);
            else Default();

            ListAltered();
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

            if (remindersSorted.Length > 0)
                returnVal = remindersSorted[0];
            else
                throw new IndexOutOfRangeException();

            return returnVal;
        }
        
        /// <summary>Add a reminder to the list</summary>
        public static void AddReminder(Reminder reminder)
        {
            reminders_.Add(reminder);
            ListAltered();
        }

        /// <summary>Remove an existing reminder from the list.</summary>
        public static void RemoveReminder(Reminder reminder)
        {
            if (reminders_.Contains(reminder))
                reminders_.Remove(reminder);
            ListAltered();
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
    }
}