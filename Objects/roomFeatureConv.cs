using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using static System.ComponentModel.TypeConverter;

namespace Chronicle.Facilities.Rooms.Objects
{
    internal class roomFeatureConv : StringConverter
    {
        public static string[] getFeatures()
        {
            List<string> features = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(Globals.ConnectionString))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT roomFeatureText FROM ROOM_FEATURES;";
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    features.Add(reader["roomFeatureText"] as string ?? "");
                }
            }

            return features.ToArray();
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false; // Forces dropdown only
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(getFeatures());
        }
    }
}
