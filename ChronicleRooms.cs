using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronicle.Plugins.Core;
using System.Windows.Forms;

namespace Chronicle.Facilities.Rooms
{
    internal class ChronicleRooms : IPlugable
    {
        public string PluginName => "Manage Rooms";
        public string PluginDescription => "";

        public Version Version => new Version("0.0.0.1");

        public int Execute()
        {
            Rooms r = new();
            r.Show();
            return 0;
        }
    }
}
