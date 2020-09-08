﻿// Reminder.cs
// Contains a struct that defines a Reminder.
//
// EMIKO

using System;

namespace BigSister.Reminders
{
    /// <summary>
    /// A struct containing everything necessary for a reminder: the User, the notification message (Text), and when to notify (Time).
    /// </summary>
    public struct Reminder
    {
        /// <summary>User who scheduled the reminder.</summary>
        public ulong User;
        /// <summary>Text to be sent when the reminder is called.</summary>
        public string Text;
        /// <summary>Time of the reminder.</summary>
        public long Time;
        /// <summary>Channel the reminder was originally set in.</summary>
        public ulong Channel;
        /// <summary>A list of users to notify.</summary>
        public string[] UsersToNotify;

        /// <summary>
        /// This is a command that (tries) to generate a unique identifier for the Reminder, which can be used to cancel the reminder at a later date.
        /// </summary>
        /// <returns>A string representing the User's ID and the Time's Unix EPOCH.</returns>
        public string GetIdentifier() => User.ToString() + '@' + Time.ToString();

        // Yummy overrides!
        public override bool Equals(object obj)
        {
            return obj is Reminder reminder &&
                   User == reminder.User &&
                   Text == reminder.Text &&
                   Time == reminder.Time &&
                   Channel == reminder.Channel;
        }

        #region bunny

        //   /\\=//\-"""-.        
        //  / /6 6\ \     \        
        //   =\_Y_/=  (_  ;{}     
        //     /^//_/-/__/  jgs    
        //     "" ""  """       

        #endregion bunny

        public override int GetHashCode()
        {
            return HashCode.Combine(User, Text, Time, Channel);
        }
    }
}