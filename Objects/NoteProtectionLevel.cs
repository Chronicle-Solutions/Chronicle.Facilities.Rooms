using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle.Facilities.Rooms.Objects
{
    public enum NoteProtectionLevel
    {
        /// <summary>
        /// Viewable by Everyone
        /// </summary>
        Public,
        /// <summary>
        /// Viewable only by Events Employees
        /// </summary>
        Internal,
        /// <summary>
        /// Viewable only by the creator of the note and administrators.
        /// </summary>
        Private

    }
}
