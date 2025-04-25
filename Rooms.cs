using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;
using FontAwesome.Sharp;
using Chronicle.Facilities.Rooms.Objects;
using Chronicle.Utils;

namespace Chronicle.Facilities.Rooms
{
    public partial class Rooms : Form
    {
        ToolStripMenuItem selectedItem;
        ListViewColumnSorter lvwColumnSorter;
        public Rooms()
        {
            InitializeComponent();
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
            selectedItem = allToolStripMenuItem;
            Form1.populateMenu(menuToolStripMenuItem.DropDownItems, "/");
            getBuildingNames();
            populateRooms("(all)");
        }


        public void populateRooms(string buildingName)
        {
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                listView1.Items.Clear();
                MySqlCommand cmd = conn.CreateCommand();
                if (buildingName == "(all)")
                {
                    cmd.CommandText = "SELECT A.*, B.buildingCode, B.active as 'buildingActive' FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID;";
                } else
                {
                    cmd.CommandText = "SELECT A.*, B.buildingCode, B.active as 'buildingActive' FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND B.buildingName = @bName ORDER BY A.roomCode;";
                    cmd.Parameters.AddWithValue("@bName", buildingName);
                }
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem itm = new ListViewItem();
                    itm.Text = reader.GetInt32("roomID").ToString();
                    itm.SubItems.Add(reader["buildingCode"] as string ?? "");
                    itm.SubItems.Add(reader["roomCode"] as string ?? "");
                    itm.SubItems.Add(reader["roomName"] as string ?? "");

                    bool active = reader.GetBoolean("active") && reader.GetBoolean("buildingActive");
                    if (!active)
                    {
                        itm.ForeColor = Color.Red;
                        itm.Font = new Font(itm.Font.FontFamily, itm.Font.Size, FontStyle.Italic);
                    }
                    listView1.Items.Add(itm);

                }
            }
        }

        public void getBuildingNames()
        {
            List<ToolStripMenuItem> categories = new List<ToolStripMenuItem>();
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT buildingName, active from BUILDINGS ORDER BY buildingName;";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ToolStripMenuItem item = categories.FirstOrDefault(item => item.Text[0] == reader.GetString("buildingName")[0]);
                    if (item is null)
                    {
                        item = new ToolStripMenuItem();
                        item.Text = reader.GetString("buildingName")[0].ToString();
                        ToolStripItem subitm = item.DropDownItems.Add(reader.GetString("buildingName"));
                        if (!reader.GetBoolean("active"))
                            subitm.ForeColor = Color.Red;
                        subitm.Click += onFilterClick;
                        categories.Add(item);
                    }
                    else
                    {
                        ToolStripItem subitm = item.DropDownItems.Add(reader.GetString("buildingName"));
                        if (!reader.GetBoolean("active"))
                            subitm.ForeColor = Color.Red;
                        categories.Add(item);
                        subitm.Click += onFilterClick;
                    }
                }

            }
            filterToolStripMenuItem.DropDownItems.AddRange(categories.ToArray());
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }


        private void onFilterClick(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem itm) return;
            selectedItem.Checked = false;
            selectedItem = itm;
            selectedItem.Checked = true;
            string buildingName = itm.Text ?? "(all)";
            listView1.Items.Clear();
            populateRooms(buildingName);

        }

        private void newToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = new Room();
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (propertyGrid1.SelectedObject is not Room currentRoom) return;

            if(currentRoom.roomID == 0)
            {
                // Insert
                insertRoom(currentRoom);
            } else
            {
                // Update
                updateRoom(currentRoom);
            }
        }

        private void updateRoom(Room r)
        {
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE ROOMS SET buildingID=(SELECT buildingID from BUILDINGS WHERE buildingName = @buildingName), roomCode=@roomCode, roomName=@roomName, active=@active, updateBy=@oprID, updateDt=current_timestamp WHERE roomID=@roomID";
                cmd.Parameters.AddWithValue("@buildingName", r.BuildingName);
                cmd.Parameters.AddWithValue("@roomCode", r.RoomCode);
                cmd.Parameters.AddWithValue("@roomName", r.RoomName);
                cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                cmd.Parameters.AddWithValue("@roomID", r.roomID);
                cmd.Parameters.AddWithValue("@active", r.Active);
                List<roomNotes> newNotes = new List<roomNotes>();
                foreach (roomNotes note in r.Notes)
                {
                    if (note.noteID == 0 && note.markForDelete) continue;
                    if (note.markForDelete)
                    {
                        cmd.CommandText = "DELETE FROM ROOM_NOTES WHERE roomID = @roomID";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@roomID", r.roomID);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        newNotes.Add(note);
                        cmd.CommandText = "UPDATE ROOM_NOTES SET roomID = @roomID, noteText = @noteText, noteProtectionLevel = @nPL, updateBy = @oprID, updateDt = current_timestamp WHERE roomNoteID = @noteID";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@roomID", r.roomID);
                        cmd.Parameters.AddWithValue("@noteText", note.noteText);
                        cmd.Parameters.AddWithValue("@nPL", note.accessLevel.ToString());
                        cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                        cmd.Parameters.AddWithValue("@noteID", note.noteID);
                        cmd.ExecuteNonQuery();
                    }
                }
                r.Notes = newNotes;
                if (r.RoomType == roomType.Combination)
                {
                    // Insert Combination Rooms
                    foreach (componentSpace space in r.componentSpaces)
                    {
                        if (space.markForDelete && space.componentSpaceID == 0)
                            continue;
                        if (space.markForDelete)
                        { 
                            deleteSpace(space, cmd);
                            continue;
                        }
                        if (space.componentSpaceID == 0) {
                            insertComboSpace(space, r, cmd);
                            continue;
                        }
                        updateComboSpace(space, r, cmd);
                    }
                }
            }
        }

        private void deleteSpace(componentSpace space, MySqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "DELETE FROM COMBINATION_ROOM_LINK WHERE combinationRoomID = @crID";
            cmd.Parameters.AddWithValue("@crID", space.componentSpaceID);
            cmd.ExecuteNonQuery();
        }

        private void updateComboSpace(componentSpace space, Room r, MySqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "UPDATE COMBINATION_ROOM_LINK SET parentRoomID = @prID, componentRoomID = (SELECT A.roomID FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND A.roomName = @rName AND A.roomCode = @rCode AND B.buildingCode = @bCode), updateBy = @oprID, updateDt = current_timestamp ;";
            cmd.Parameters.AddWithValue("@prID", r.roomID);
            cmd.Parameters.AddWithValue("@rName", space.roomName);
            cmd.Parameters.AddWithValue("@rCode", space.roomCode);
            cmd.Parameters.AddWithValue("@bCode", space.buildingCode);
            cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
            cmd.ExecuteNonQuery();
        }

        private void insertRoom(Room r)
        {
            using(MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO ROOMS (buildingID, roomCode, roomName, roomType, addedBy, addedDt, updateBy, updateDt) VALUES ((SELECT buildingID from BUILDINGS WHERE buildingName = @buildingName), @roomCode, @roomName, @rType, @oprID, current_timestamp, @oprID, current_timestamp)";
                cmd.Parameters.AddWithValue("@buildingName", r.BuildingName);
                cmd.Parameters.AddWithValue("@roomCode", r.RoomCode);
                cmd.Parameters.AddWithValue("@roomName", r.RoomName);
                cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                cmd.Parameters.AddWithValue("@rType", r.RoomType.ToString());
                cmd.ExecuteNonQuery();
                long roomID = cmd.LastInsertedId;
                r.roomID = (int)roomID;
                foreach (roomNotes note in r.Notes)
                {
                    if (note.noteID == 0 && note.markForDelete) continue;
                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO ROOM_NOTES (roomID, noteText, noteProtectionLevel, addedBy, addedDt, updateBy, updateDt) VALUES (@roomID, @noteText, @nPL, @oprID, current_timestamp, @oprID, current_timestamp)";
                    cmd.Parameters.AddWithValue("@roomID", roomID);
                    cmd.Parameters.AddWithValue("@noteText", note.noteText);
                    cmd.Parameters.AddWithValue("@nPL", note.accessLevel.ToString());
                    cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                    cmd.ExecuteNonQuery();
                    note.noteID = (int)cmd.LastInsertedId;
                }
                if(r.RoomType == roomType.Combination)
                {
                    // Insert Combination Rooms
                    foreach (componentSpace space in r.componentSpaces)
                    {
                        if (space.markForDelete) continue;
                        insertComboSpace(space, r, cmd);
                    }
                }
            }
        }

        private void insertComboSpace(componentSpace space, Room r, MySqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "INSERT INTO COMBINATION_ROOM_LINK (parentRoomID, componentRoomID, addedBy, addedDt, updateBy, updateDt) VALUES (@roomID, (SELECT A.roomID FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND A.roomName = @rName AND A.roomCode = @rCode AND B.buildingCode = @bCode), @oprID, current_timestamp, @oprID, current_timestamp)";
            cmd.Parameters.AddWithValue("@roomID", r.roomID);
            cmd.Parameters.AddWithValue("@rName", space.roomName);
            cmd.Parameters.AddWithValue("@rCode", space.roomCode);
            cmd.Parameters.AddWithValue("@bCode", space.buildingCode);
            cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
            cmd.ExecuteNonQuery();
            space.componentSpaceID = (int)cmd.LastInsertedId;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 0)
            {
                propertyGrid1.SelectedObject = null;
                return;
            }
            string roomID = listView1.SelectedItems[0].Text;

            using(MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT A.*, B.buildingName FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND roomID = @roomID";
                cmd.Parameters.AddWithValue("@roomID", roomID);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Room room = new Room();
                    room.Active = reader.GetBoolean("active");
                    room.roomID = reader.GetInt32("roomID");
                    room.CreatedBy = reader["addedBy"] as string ?? "";
                    room.UpdatedBy = reader["updateBy"] as string ?? "";
                    room.CreatedDate = reader["addedDt"] as DateTime? ?? new DateTime();
                    room.UpdatedDate = reader["updateDt"] as DateTime? ?? new DateTime();
                    room.UpdatedBy = reader["updateBy"] as string ?? "";
                    room.RoomName = reader["roomName"] as string ?? "";
                    room.RoomCode = reader["roomCode"] as string ?? "";
                    room.AllowAlternateNames = reader.GetBoolean("allowAlternateName");
                    room.IsAcademic = reader.GetBoolean("isAcademic");
                    room.BuildingName = reader["buildingName"] as string ?? "";
                    room.Notes.AddRange(getRoomNotes(roomID));
                    switch(reader["roomType"] as string ?? "")
                    {
                        default: // Default should be Standard.
                        case "Standard":
                            room.RoomType = roomType.Standard;
                            break;
                        case "Combination":
                            room.RoomType = roomType.Combination;
                            getComponentSpaces(room);
                            break;

                    }
                    propertyGrid1.SelectedObject = room;
                }
            }
        }

        public void getComponentSpaces(Room room)
        {
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT A.addedBy, A.updateBy, A.addedDt, A.updateDt, A.combinationRoomID, B.roomCode, B.roomName, C.buildingCode FROM COMBINATION_ROOM_LINK A, ROOMS B, BUILDINGS C WHERE A.componentRoomID = B.roomID AND B.buildingID = C.buildingID AND A.parentRoomID = @prID";
                cmd.Parameters.AddWithValue("@prID", room.roomID);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    componentSpace s = new componentSpace(false)
                    {
                        roomName = reader["roomName"] as string ?? "",
                        roomCode = reader["roomCode"] as string ?? "",
                        buildingCode = reader["buildingCode"] as string ?? "",
                        CreatedBy = reader["addedBy"] as string ?? "",
                        UpdatedBy = reader["updateBy"] as string ?? "",
                        CreatedDate = reader["addedDt"] as DateTime? ?? new DateTime(),
                        UpdatedDate = reader["updateDt"] as DateTime? ?? new DateTime(),
                        componentSpaceID = reader["combinationRoomID"] as int? ?? 0
                    };
                    room.componentSpaces.Add(s);
                }

            }
        }

        public IEnumerable<roomNotes> getRoomNotes(string roomID)
        {
            List<roomNotes> notes = new List<roomNotes>();
            using(MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT A.* FROM ROOM_NOTES A, OPERATOR_CLASS_OVERRIDES B WHERE"+
                                  " A.roomID = @roomID AND (A.noteProtectionLevel = 'Public' OR (A.noteProtectionLevel = "+
                                  "'Private' AND (A.addedBy = @oprID OR (B.operatorID = @oprID AND "+
                                  "B.operatorClassOverrideTypeID = (SELECT operatorClassOverrideTypeID "+
                                  "FROM OPERATOR_CLASS_OVERRIDE_TYPES WHERE overrideDescription = 'Administrator'))))"+
                                  " OR (A.noteProtectionLevel = 'Internal' AND "+
                                  "(EXISTS((SELECT * FROM INTERNAL_DEPARTMENTS A, OPERATORS C WHERE "+
                                  "A.departmentID = C.departmentID AND C.operatorID = @oprID)))));";
                cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                cmd.Parameters.AddWithValue("@roomID", roomID);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    roomNotes note = new roomNotes();
                    note.noteText = reader["noteText"] as string ?? "";
                    note.addedDt = reader["addedDt"] as DateTime? ?? new DateTime();
                    note.addedBy = reader["addedBy"] as string ?? "";
                    note.updatedDt = reader["updateDt"] as DateTime? ?? new DateTime();
                    note.updatedBy = reader["updateBy"] as string ?? "";
                    note.noteID = reader.GetInt32("roomNoteID");
                    switch (reader["noteProtectionLevel"] as string ?? "")
                    {
                        case "Public":
                            note.accessLevel = NoteProtectionLevel.Public;
                            break;
                        case "Private":
                            note.accessLevel = NoteProtectionLevel.Private;
                            break;
                        case "Internal":
                            note.accessLevel = NoteProtectionLevel.Internal;
                            break;
                    }
                    notes.Add(note);
                }
            }


            return notes.AsEnumerable<roomNotes>();
        }
    }
}
