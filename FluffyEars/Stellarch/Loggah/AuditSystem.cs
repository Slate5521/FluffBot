using BigSister.ChatObjects;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BigSister.Loggah
{
    static class AuditSystem
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        internal static async Task Bot_CommandExecuted(CommandExecutionEventArgs e)
        {
            semaphore.Wait();

            try
            {
                using (StreamWriter sw = new StreamWriter(Program.Files.LogFile, true))
                {
                    DateTimeOffset dto = DateTimeOffset.UtcNow;
                    string timeStamp = dto.ToString(Generics.DateFormat);

                    await sw.WriteAsync(
                        value: String.Format("[{0}] '{1}' CALLED BY '{2}' ID '{3}' WITH ARGUMENTS '{4}'",
                            timeStamp,
                            e.Command.Name,
                            $"{e.Context.Member.Username}#{e.Context.Member.Discriminator}",
                            e.Context.Member.Id,
                            e.Context.RawArgumentString));
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
