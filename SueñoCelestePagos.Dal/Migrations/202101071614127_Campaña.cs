namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Campaña : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblCampañas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Año = c.Int(nullable: false),
                        FechaInicio = c.DateTime(nullable: false),
                        FechaFin = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tblCampañas");
        }
    }
}
