using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using static System.ComponentModel.TypeConverter;

namespace Chronicle.Facilities.Rooms.Objects
{
    public class BuildingConverter : StringConverter
    {

        public static string[] getBuildings()
        {
            List<string> buildings = new List<string>();
            using(MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT buildingName FROM BUILDINGS WHERE active=1;";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    buildings.Add(reader["buildingName"] as string ?? "");
                }
            }

            return buildings.ToArray();
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true; // Forces dropdown only
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(getBuildings());
        }
    }
}
