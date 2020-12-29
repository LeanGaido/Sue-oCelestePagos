namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechasYCambios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Institucions", "LocalidadID", c => c.Int(nullable: false));
            AddColumn("dbo.tblFechasLimiteVentaCartones", "FechaDesde", c => c.DateTime(nullable: false));
            AddColumn("dbo.tblFechasLimiteVentaCartones", "FechaHasta", c => c.DateTime(nullable: false));
            CreateIndex("dbo.Institucions", "LocalidadID");
            AddForeignKey("dbo.Institucions", "LocalidadID", "dbo.tblLocalidad", "ID", cascadeDelete: true);
            DropColumn("dbo.tblFechasLimiteVentaCartones", "Fecha");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblFechasLimiteVentaCartones", "Fecha", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.Institucions", "LocalidadID", "dbo.tblLocalidad");
            DropIndex("dbo.Institucions", new[] { "LocalidadID" });
            DropColumn("dbo.tblFechasLimiteVentaCartones", "FechaHasta");
            DropColumn("dbo.tblFechasLimiteVentaCartones", "FechaDesde");
            DropColumn("dbo.Institucions", "LocalidadID");
        }
    }
}
