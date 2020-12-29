namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechaLimiteVigente : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblFechasLimiteVentaCartones", "Vigente", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblFechasLimiteVentaCartones", "Vigente");
        }
    }
}
