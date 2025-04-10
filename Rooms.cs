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

namespace Chronicle.Facilities.Rooms
{
    public partial class Rooms : Form
    {
        public Rooms()
        {
            InitializeComponent();

            saveToolStripMenuItem.Image = IconChar.Save.ToBitmap(48, 48, Color.Black);
            newToolStripMenuItem.Image = IconChar.File.ToBitmap(48, 48, Color.Black);
            listView1.Groups.AddRange(getBuildingNames().ToArray());

        }

        public List<ListViewGroup> getBuildingNames()
        {
            List<ListViewGroup> buildings = new List<ListViewGroup>();

            using (MySqlConnection conn = new(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM BUILDINGS";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ListViewGroup group = new();
                    group.Header = reader.GetString("buildingName");
                    buildings.Add(group);
                }

                return buildings;
            }
        }
    }
}
