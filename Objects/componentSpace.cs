using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chronicle.Facilities.Rooms.Objects
{
    public class componentSpace
    {
        private bool lockObj = false;

        public componentSpace()
        {
            showDialog();
        }

        private void showDialog()
        {
            RoomPicker f = new RoomPicker();

            DialogResult res = f.ShowDialog();
            if (res != DialogResult.OK)
            {
                markForDelete = true;
                lockObj = true;
                buildingCode = roomCode = roomName = "(none)";
                
                
                return;
            }
            ListViewItem itm = f.listView1.SelectedItems[0];
            roomCode = itm.Text;
            roomName = itm.SubItems[1].Text;
            buildingCode = itm.SubItems[2].Text;

        }

        public componentSpace(bool showForm)
        {
            if (showForm)
            {
                showDialog();
            }
        }

        #region Audit
        [ReadOnly(true)]
        [Category("Audit")]
        [Description("Unique Internal Object Identifier")]
        [DisplayName("Object ID")]
        public int componentSpaceID { get; set; }

        [Category("Audit")]
        [DisplayName("Created By")]
        [ReadOnly(true)]
        public string? CreatedBy { get; set; }

        [Category("Audit")]
        [DisplayName("Created Date")]
        [ReadOnly(true)]
        public DateTime CreatedDate { get; set; }

        [Category("Audit")]
        [DisplayName("Updated By")]
        [ReadOnly(true)]
        public string? UpdatedBy { get; set; }

        [Category("Audit")]
        [DisplayName("Updated Date")]
        [ReadOnly(true)]
        public DateTime UpdatedDate { get; set; }

        private bool _delete;

        [Category("Audit")]
        [DisplayName("Remove Component")]
        public bool markForDelete { get => _delete; set {
                _delete = value & !lockObj;
            } }
        #endregion

        #region Component Space Information
        [Category("Component Data")]
        [DisplayName("Building Code")]
        [ReadOnly(true)]
        public string? buildingCode {get; set;}

        [Category("Component Data")]
        [DisplayName("Room Code")]
        [ReadOnly(true)]
        public string? roomCode { get; set; }

        [Category("Component Data")]
        [DisplayName("Room Name")]
        [ReadOnly(true)]
        public string? roomName { get; set; }

        #endregion
    }
}
