using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using static BigSister.Program;
using System.Threading.Tasks;

namespace BigSister
{
    /// <remarks>Singleton-type class for thread safety.</remarks>
    public class Announcements
    {
        readonly SemaphoreSlim semaphoreSlim;

        /// <summary>All announcements.</summary>
        List<AnnouncementInfo> announcements;
        /// <summary>The next time to post an announcement.</summary>
        DateTimeOffset nextPost;

        bool loaded;

        // Singleton implementation
        /// <summary>Get class instance.</summary>
        public static Announcements Instance { get; } = new Announcements();

        Announcements()
        {
            semaphoreSlim = new SemaphoreSlim(1, 1);
            loaded = false;
        }

        ~Announcements()
        {
            semaphoreSlim.Dispose();
        }

        /// <summary>Load the announcements file.</summary>
        public bool Load()
        {
            bool @return;

            if (!loaded)
            {
                if (File.Exists(Files.AnnouncementsFile))
                {
                    string announcementsFileContents;

                    // Read contents
                    using (StreamReader sr = new StreamReader(Files.AnnouncementsFile))
                    {
                        announcementsFileContents = sr.ReadToEnd();
                    }

                    // Deserialize the object.
                    announcements = JsonConvert.DeserializeObject<List<AnnouncementInfo>>(announcementsFileContents);

                    // Set our tracker and return variable.
                    loaded = @return = true;
                }
                else
                {
                    throw new FileNotFoundException("Unable to find announcements.json.");
                } // end else
            } // end if
            else
            {
                throw new Exception("Attempting to load announcements twice.");
            }

            // Finally, set the next post.
            SetNextPost();

            return @return;
        }

        async Task Save()
        {
            semaphoreSlim.Wait();

            try
            {
#pragma warning disable IDE0063 // Use simple 'using' statement
                // Open SW on announcements file with intention to overwrite.
                using (StreamWriter sw = new StreamWriter(Files.AnnouncementsFile, false))
                {
                    // Dump that shit out!
                    await sw.WriteAsync(JsonConvert.SerializeObject(announcements));
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>Set nextPost variable to the next most recent post time.</summary>
        void SetNextPost()
        {
            // Check if there's actually announcements.
            if (announcements.Count > 0)
            {   // Yes, there are! So let's set the next post.
                nextPost = announcements.Min(a => a.PostTime);
            }
            else
            {   // There are not, so let's set the next post to a time value that is unfeasible to reach so I have... /adequate/ time to get home 
                // from work and turn the bot off.
                nextPost = DateTimeOffset.MaxValue;
            }

        }

        /// <summary>Check if there are any announcements currently pending.</summary>
        /// <returns>True if any announcements are pending posting.</returns>
        public bool IsPending()
            => DateTimeOffset.UtcNow >= nextPost;

        /// <summary>Get the announcements pending posting.</summary>
        public List<AnnouncementInfo> GetPendingAnnouncements()
        {
            // Set the list to an initial value.
            List<AnnouncementInfo> @return = new List<AnnouncementInfo>();

            // Check if any are pending.
            if(IsPending())
            {   // There are some pending.

                var dtoNow = DateTimeOffset.UtcNow;

                // Only gather the announcements that need to be posted.
                @return.AddRange(announcements.Where(a => dtoNow >= a.PostTime));
            }

            return @return;
        }

        /// <summary>Pops a single announcement.</summary>
        public void PopAnnouncement(AnnouncementInfo announcement)
            // Listen, I know this is terrible practice, but I don't care :D
            => PopAnnouncementsRange(new AnnouncementInfo[] { announcement });

        /// <summary>Pops a series of announcements.</summary>
        public void PopAnnouncementsRange(IEnumerable<AnnouncementInfo> announcementsToPop)
        {
            // Remove only the announcements from memory that are found in the announcements pop list.
            announcements.RemoveAll(a => announcementsToPop.Contains(a));

            // Set the next post.
            SetNextPost();

            // Finally, we want to save.
            Save().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
