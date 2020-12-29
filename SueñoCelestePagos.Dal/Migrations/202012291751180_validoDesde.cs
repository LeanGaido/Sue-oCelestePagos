namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class validoDesde : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCartones", "ValidoDesde", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCartones", "ValidoDesde");
        }
    }
}
