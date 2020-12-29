namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CartonValidoHasta : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCartones", "ValidoHasta", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCartones", "ValidoHasta");
        }
    }
}
