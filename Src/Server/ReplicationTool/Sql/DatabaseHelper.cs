using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplicationTool.Sql
{
    public class DatabaseHelper
    {


        public void Backup(string connectionString, string databaseName, string pathBackup)
        {
            string file = string.Format("dbbackup{0}.bak", DateTime.Now.Ticks);
            //string pathBackup = Path.Combine(@"C:\temp\", file);
            string remoteNetworkPathBackup = Path.Combine(@"\\taxihail01\DBBackup", file);
            string remoteLocalPathBackup = Path.Combine(@"C:\DBBackup\", file);

            using (SqlConnection con = new SqlConnection(connectionString ))
            {
                ServerConnection srvConn = new ServerConnection(con);

                Server srvr = new Server(srvConn);

                if (srvr != null)
                {

                    try
                    {
                        var bkpDatabase = new Backup();
                        bkpDatabase.Action = BackupActionType.Database;
                        bkpDatabase.Database = "CabMateDemo";



                        var bkpDevice = new BackupDeviceItem(pathBackup, DeviceType.File);
                        bkpDatabase.Devices.Add(bkpDevice);
                        bkpDatabase.SqlBackup(srvr);


                        File.Copy(pathBackup, remoteNetworkPathBackup);



                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.ToString());
                    }
                }
            }

        }
    }
}
    



    
    //class Backup_Restore
    //{

    //    public string BackUpConString = @"Data Source=(local);Initial Catalog=CabMateDemo;Integrated Security=True";
    //    public string ReStoreConString = "Data Source=(local);Initial Catalog=master;Integrated Security=True";

    //    public void BackUpMyDB()
    //    {
    //        string file = string.Format("dbbackup{0}.bak", DateTime.Now.Ticks);
    //        string pathBackup = Path.Combine(@"C:\temp\", file);
    //        string remoteNetworkPathBackup = Path.Combine(@"\\taxihail01\DBBackup", file);
    //        string remoteLocalPathBackup = Path.Combine(@"C:\DBBackup\", file);
            
    //        using (SqlConnection con = new SqlConnection(BackUpConString))
    //        {
    //            ServerConnection srvConn = new ServerConnection(con);

    //            Server srvr = new Server(srvConn);
                
    //            if (srvr != null)
    //            {

    //                try
    //                {
    //                    var bkpDatabase = new Backup();
    //                    bkpDatabase.Action = BackupActionType.Database;
    //                    bkpDatabase.Database = "CabMateDemo";

                       

    //                    var bkpDevice = new BackupDeviceItem(pathBackup, DeviceType.File);
    //                    bkpDatabase.Devices.Add(bkpDevice);
    //                    bkpDatabase.SqlBackup(srvr);


    //                    File.Copy(pathBackup, remoteNetworkPathBackup);

                        
                        
    //                }
    //                catch (Exception e)
    //                {
    //                    //MessageBox.Show(e.ToString());
    //                }
    //            }
    //        }


    //        SqlConnection.ClearAllPools();
    //        using (SqlConnection con = new SqlConnection(ReStoreConString))
    //        {
    //            ServerConnection srvConn = new ServerConnection(con);

    //            Server srvr = new Server(srvConn);


    //            if (srvr != null)
    //            {
    //                try
    //                {

    //                    Restore rstDatabase = new Restore();
    //                    rstDatabase.Action = RestoreActionType.Database;
    //                    rstDatabase.Database = "CabMateDemo";
                        


    //                        BackupDeviceItem bkpDevice = new BackupDeviceItem(remoteLocalPathBackup, DeviceType.File);

    //                        rstDatabase.Devices.Add(bkpDevice);
    //                        rstDatabase.ReplaceDatabase = true;
    //                        rstDatabase.SqlRestore(srvr);
                            
                        
    //                }
    //                catch (Exception e)
    //                {
                        
    //                }
    //            }
    //        }
    //    }
    //    public void ReStorMyDB()
    //    {
    //        //if (MessageBox.Show("All Data Stored in the Database may change!!! \n If you agree, select \"Yes\".", "DataBase ReStore", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
    //        //{

    //        //    SqlConnection.ClearAllPools();
    //        //    using (SqlConnection con = new SqlConnection(BackUpConString))
    //        //    {
    //        //        ServerConnection srvConn = new ServerConnection(con);

    //        //        Server srvr = new Server(srvConn);


    //        //        if (srvr != null)
    //        //        {
    //        //            try
    //        //            {

    //        //                Restore rstDatabase = new Restore();
    //        //                rstDatabase.Action = RestoreActionType.Database;
    //        //                rstDatabase.Database = "taban";
    //        //                OpenFileDialog opfd = new OpenFileDialog();
    //        //                opfd.Filter = "BackUp File|*.taban";
    //        //                if (opfd.ShowDialog() == DialogResult.OK)
    //        //                {


    //        //                    BackupDeviceItem bkpDevice = new BackupDeviceItem(opfd.FileName, DeviceType.File);

    //        //                    rstDatabase.Devices.Add(bkpDevice);
    //        //                    rstDatabase.ReplaceDatabase = true;
    //        //                    rstDatabase.SqlRestore(srvr);
    //        //                    MessageBox.Show("Database succefully restored", "Server", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //        //                }
    //        //            }
    //        //            catch (Exception e)
    //        //            {
    //        //                MessageBox.Show("ERROR: An error ocurred while restoring the database", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    //        //            }
    //        //        }
    //        //    }

    //        //}
    //    }
    //}