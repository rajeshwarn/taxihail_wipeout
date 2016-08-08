namespace apcurium.MK.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_Mears_Aug3_2016 : DbMigration
    {
        public override void Up()
        {
            DropColumn("Config.ServiceTypeSettings", "SupportPhoneNumber");
        }
        
        public override void Down()
        {
            AddColumn("Config.ServiceTypeSettings", "SupportPhoneNumber", c => c.String());
        }
    }
}
