namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Instituciones : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FechaLimiteDebitoes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Mes = c.Int(nullable: false),
                        Año = c.Int(nullable: false),
                        FechaLimite = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Institucions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Cuit = c.String(),
                        Calle = c.String(),
                        Altura = c.Int(nullable: false),
                        Ciudad = c.String(),
                        Provincia = c.String(),
                        AreaTelefono = c.String(),
                        NumeroTelefono = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.tblCartonesVendidos", "EntidadID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCartonesVendidos", "EntidadID");
            DropTable("dbo.Institucions");
            DropTable("dbo.FechaLimiteDebitoes");
        }
    }
}
