namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechaPagoNulleable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tblCartonesVendidos", "FechaPago", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tblCartonesVendidos", "FechaPago", c => c.DateTime(nullable: false));
        }
    }
}
