using System;
using System.Collections.Generic;
using System.Linq;
using FluffyEars;
using FluffyEars.Reminders;

namespace tester
{
    class Program
    {
        public static class ctx
        {
            public static string RawArgumentString = "`20 d 40 hrs 1 mins` `love`";
        }

        static void Main(string[] args)
        {
            object lockObj = new object();
            List<Reminder> reminders = new List<Reminder>();

            //for(int i = 0; i < 20; i++)
            //    reminders.Add(new Reminder { Text = i.ToString(), Time = ((i * 50000 + 2) * 10) / 200, User = (ulong)(i + 800 + 20 / 40) });
            //    reminders.Add(new Reminder { Text = i.ToString(), Time = ((i * 40000 + 2) * 10) / 200, User = (ulong)(i + 700 + 20 / 40) });

            SaveFile sf = new SaveFile("test");

            //sf.Save<List<Reminder>>(reminders, lockObj);
            sf.Load<List<Reminder>>(lockObj);

        }
    }
}
