using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chronicle.Facilities.Rooms.Objects
{
    public class roomNotes
    {
        [ReadOnly(true)]
        [Category("Audit")]
        [Description("Unique Internal Object Identifier")]
        [DisplayName("Object ID")]
        public int noteID {get;set;}
        [Category("Audit")]
        [DisplayName("Added By")]
        [Description("The OperatorID of the user who created this object")]
        [ReadOnly(true)]
        public string addedBy { get; set; }
        [Category("Audit")]
        [DisplayName("Last Updated By")]
        [Description("The OperatorID of the user who last updated this object")]
        [ReadOnly(true)]
        public string updatedBy { get; set; }

        [Category("Audit")]
        [DisplayName("Added DateTime")]
        [Description("The Date and Time of creation")]
        [ReadOnly(true)]
        public DateTime addedDt { get; set; } = new();

        [Category("Audit")]
        [DisplayName("Last Updated DateTime")]
        [Description("The Date and Time of the last update to this object")]
        [ReadOnly(true)]
        public DateTime updatedDt { get; set; } = new();

        [Category("General Information")]
        [DisplayName("Access Level")]
        [Description("Who can access this note")]
        public NoteProtectionLevel accessLevel { get; set; }

        [Category("General Information")]
        [DisplayName("Note Text")]
        [Description("Content of the room note")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string noteText { get; set; }
        [Category("Audit")]
        [DisplayName("Delete Note")]
        public bool markForDelete { get; set; }
    }
}
