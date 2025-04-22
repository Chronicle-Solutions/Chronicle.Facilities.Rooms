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

namespace Chronicle.Facilities.Rooms
{
    public partial class Rooms : Form
    {
        ToolStripMenuItem selectedItem;

        public Rooms()
        {
            InitializeComponent();
            selectedItem = allToolStripMenuItem;
            Form1.populateMenu(menuToolStripMenuItem.DropDownItems, "/");
            getBuildingNames();
            populateRooms();
        }


        public void populateRooms()
        {
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT A.*, B.buildingCode, B.active as 'buildingActive' FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID;";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ListViewItem itm = listView1.Items.Add(reader.GetInt32("roomID").ToString());
                    itm.SubItems.Add(reader["buildingCode"] as string ?? "");
                    itm.SubItems.Add(reader["roomCode"] as string ?? "");
                    itm.SubItems.Add(reader["roomName"] as string ?? "");

                    bool active = reader.GetBoolean("active") && reader.GetBoolean("buildingActive");
                    if (!active)
                    {
                        itm.ForeColor = Color.Red;
                        itm.Font = new Font(itm.Font.FontFamily, itm.Font.Size, FontStyle.Italic);
                    }

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


        private void onFilterClick(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem itm) return;
            selectedItem.Checked = false;
            selectedItem = itm;
            selectedItem.Checked = true;
            string buildingName = itm.Text ?? "(all)";
            listView1.Items.Clear();
            if (buildingName == "(all)")
            {
                populateRooms();
            }
            else
            {

                using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT A.*, B.buildingCode, B.active as 'buildingActive' FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND B.buildingName = @bName;";
                    cmd.Parameters.AddWithValue("@bName", buildingName);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ListViewItem i = listView1.Items.Add(reader.GetInt32("roomID").ToString());
                        i.SubItems.Add(reader["buildingCode"] as string ?? "");
                        i.SubItems.Add(reader["roomCode"] as string ?? "");
                        i.SubItems.Add(reader["roomName"] as string ?? "");

                        bool active = reader.GetBoolean("active") && reader.GetBoolean("buildingActive");
                        if (!active)
                        {
                            i.ForeColor = Color.Red;
                            i.Font = new Font(i.Font.FontFamily, i.Font.Size, FontStyle.Italic);
                        }
                    }



                }
            }

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
                cmd.CommandText = "UPDATE ROOMS SET buildingID=(SELECT buildingID from BUILDINGS WHERE buildingName = @buildingName), roomCode=@roomCode, roomName=@roomName, updateBy=@oprID, updateDt=current_timestamp WHERE roomID=@roomID";
                cmd.Parameters.AddWithValue("@buildingName", r.BuildingName);
                cmd.Parameters.AddWithValue("@roomCode", r.RoomCode);
                cmd.Parameters.AddWithValue("@roomName", r.RoomName);
                cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
                cmd.Parameters.AddWithValue("@roomID", r.roomID);
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
            }
        }

        private void insertRoom(Room r)
        {
            using(MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO ROOMS (buildingID, roomCode, roomName, addedBy, addedDt, updateBy, updateDt) VALUES ((SELECT buildingID from BUILDINGS WHERE buildingName = @buildingName), @roomCode, @roomName, @oprID, current_timestamp, @oprID, current_timestamp)";
                cmd.Parameters.AddWithValue("@buildingName", r.BuildingName);
                cmd.Parameters.AddWithValue("@roomCode", r.RoomCode);
                cmd.Parameters.AddWithValue("@roomName", r.RoomName);
                cmd.Parameters.AddWithValue("@oprID", Globals.OperatorID);
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
            }
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
                    propertyGrid1.SelectedObject = room;
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
