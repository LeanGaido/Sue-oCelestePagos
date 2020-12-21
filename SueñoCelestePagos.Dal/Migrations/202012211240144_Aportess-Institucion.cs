namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AportessInstitucion : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblAportesIntitucion",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        InstitucionID = c.Int(nullable: false),
                        PorcAporte = c.Decimal(nullable: false, precision: 18, scale: 2),
                        FechaAlta = c.DateTime(nullable: false),
                        FechaBaja = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Institucions", t => t.InstitucionID, cascadeDelete: true)
                .Index(t => t.InstitucionID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblAportesIntitucion", "InstitucionID", "dbo.Institucions");
            DropIndex("dbo.tblAportesIntitucion", new[] { "InstitucionID" });
            DropTable("dbo.tblAportesIntitucion");
        }
    }
}
