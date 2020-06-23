using System;
using System.Collections.Generic;
using System.Text;

namespace Stellarch
{
    static class AuditLogger
    {
        public static bool Enabled
        {
            get => Settings.SuppressAudit;
            set => Settings.SuppressAudit = value;
        }
    }
}
