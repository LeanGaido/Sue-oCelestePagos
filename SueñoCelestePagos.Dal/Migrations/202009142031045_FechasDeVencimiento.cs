namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FechasDeVencimiento : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FechaDeVencimientoes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Mes = c.Int(nullable: false),
                        Año = c.Int(nullable: false),
                        PrimerVencimiento = c.DateTime(nullable: false),
                        SegundoVencimiento = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FechaDeVencimientoes");
        }
    }
}
