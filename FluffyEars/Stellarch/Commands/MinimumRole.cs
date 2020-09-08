// MinimumRole.cs
// This is an attributes for commands to use to signify what role is required, minimum, in order to use the command.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace BigSister.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MinimumRole : Attribute
    {
        public Role MinRole;

        public MinimumRole(Role minimumRole)
            => MinRole = minimumRole;
    }
}
