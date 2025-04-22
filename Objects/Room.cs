using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Chronicle.Facilities.Rooms.Objects
{
    public class Room
    {

        [Category("Building Info")]
        [DisplayName("Building Name")]
        [TypeConverter(typeof(BuildingConverter))]
        public string BuildingName { get; set; }

        [ReadOnly(true)]
        [Category("Audit")]
        [Description("Unique Internal Object Identifier")]
        [DisplayName("Object ID")]
        public int roomID { get; set; }

        [Category("Room Info")]
        [DisplayName("Room Code")]
        public string RoomCode { get; set; }

        [Category("Room Info")]
        [DisplayName("Room Name")]
        public string RoomName { get; set; }

        [Category("Audit Info")]
        [DisplayName("Created By")]
        [ReadOnly(true)]
        public string CreatedBy { get; set; }

        [Category("Audit Info")]
        [DisplayName("Created Date")]
        [ReadOnly(true)]
        public DateTime CreatedDate { get; set; }

        [Category("Audit Info")]
        [DisplayName("Updated By")]
        [ReadOnly(true)]
        public string UpdatedBy { get; set; }

        [Category("Audit Info")]
        [DisplayName("Updated Date")]
        [ReadOnly(true)]
        public DateTime UpdatedDate { get; set; }

        [Category("Status")]
        [DisplayName("Active")]
        public bool Active { get; set; }

        [Category("Status")]
        [DisplayName("Is Academic")]
        public bool IsAcademic { get; set; }

        [Category("Status")]
        [DisplayName("Allow Alternate Names")]
        [Description("Allow users to specify a custom name in place of the room's default name?")]
        public bool AllowAlternateNames { get; set; }

        [Category("Room Info")]
        [DisplayName("Notes")]
       // [Editor(typeof(BuildingHoursCollectionEditor), typeof(UITypeEditor))]
        public List<roomNotes> Notes { get; set; } = new List<roomNotes>();

    }
}
