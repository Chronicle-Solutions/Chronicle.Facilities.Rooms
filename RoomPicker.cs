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
using Chronicle.Controls;

namespace Chronicle.Facilities.Rooms
{
    public partial class RoomPicker : Form
    {

        public RoomPicker()
        {
            InitializeComponent();
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM BUILDINGS ORDER BY buildingName";
                MySqlDataReader reader = cmd.ExecuteReader();
                List<ListViewItem> buildingNames = new List<ListViewItem>();
                buildingNames.Add(new ListViewItem("(all)"));
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["buildingName"] as string ?? "");
                    bool active = reader.GetBoolean("active");
                    if (!active)
                    {
                        item.ForeColor = Color.Red;
                        item.Font = new Font(item.Font.FontFamily, item.Font.Size, FontStyle.Italic);
                    }
                    buildingNames.Add(item);
                }
                tabbedComboBox1.AddTab("Buildings", "Building Name", buildingNames.ToArray());
                tabbedComboBox1.SelectedValueChanged += new EventHandler(updateList);
            }

        }

        private void updateList(object? sender, EventArgs e)
        {

            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                listView1.Items.Clear();

                MySqlCommand cmd = conn.CreateCommand();
                if (tabbedComboBox1.SelectedItem == "(all)")
                {
                    cmd.CommandText = "SELECT A.*, B.buildingCode FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID ORDER BY A.roomCode;";
                }
                else
                {
                    cmd.CommandText = "SELECT A.*, B.buildingCode FROM ROOMS A, BUILDINGS B WHERE A.buildingID = B.buildingID AND B.buildingName = @bCode ORDER BY A.roomCode;";
                    cmd.Parameters.AddWithValue("@bCode", tabbedComboBox1.SelectedItem);
                }
                MySqlDataReader reader = cmd.ExecuteReader();
                List<ListViewItem> rooms = new List<ListViewItem>();
                while (reader.Read())
                {
                    ListViewItem itm = new ListViewItem();
                    itm.Text = reader["roomCode"] as string ?? "";
                    itm.SubItems.Add(reader["roomName"] as string ?? "");
                    itm.SubItems.Add(reader["buildingCode"] as string ?? "");
                    rooms.Add(itm);
                }
                listView1.Items.AddRange(rooms.ToArray());
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;

            DialogResult = DialogResult.OK;

        }

        private void RoomPicker_FormClosed(object sender, FormClosedEventArgs e)
        {
            tabbedComboBox1.dropDownForm.Close();
        }
    }
}
