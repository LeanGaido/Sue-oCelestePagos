namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompraOffline : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblCartonesReservados",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Dni = c.String(),
                        Sexo = c.String(),
                        CartonID = c.Int(nullable: false),
                        FechaReserva = c.DateTime(nullable: false),
                        FechaExpiracionReserva = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblCartones", t => t.CartonID, cascadeDelete: true)
                .Index(t => t.CartonID);
            
            CreateTable(
                "dbo.tblCompradoresSinCuenta",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        dni = c.String(),
                        Año = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblCartonesReservados", "CartonID", "dbo.tblCartones");
            DropIndex("dbo.tblCartonesReservados", new[] { "CartonID" });
            DropTable("dbo.tblCompradoresSinCuenta");
            DropTable("dbo.tblCartonesReservados");
        }
    }
}
