
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplicationTool
{
    class Program
    {
        static void Main(string[] args)
        {

            string file = string.Format("dbbackup{0}.bak", DateTime.Now.Ticks);
            string pathBackup = Path.Combine(@"C:\temp\", file);

            //Database.

            //new Backup_Restore().BackUpMyDB();



        }
    }

}
