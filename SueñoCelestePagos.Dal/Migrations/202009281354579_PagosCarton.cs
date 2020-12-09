namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PagosCarton : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PagoCartonVendidoes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CartonVendidoID = c.Int(nullable: false),
                        TipoDePagoID = c.Int(nullable: false),
                        PagoID = c.Int(nullable: false),
                        Pago = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Pagado = c.Boolean(nullable: false),
                        FechaDePago = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblCartonesVendidos", t => t.CartonVendidoID, cascadeDelete: true)
                .ForeignKey("dbo.tblTiposDePagos", t => t.TipoDePagoID, cascadeDelete: false)
                .Index(t => t.CartonVendidoID)
                .Index(t => t.TipoDePagoID);
            
            AddColumn("dbo.tblCartonesVendidos", "Pagos", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PagoCartonVendidoes", "TipoDePagoID", "dbo.tblTiposDePagos");
            DropForeignKey("dbo.PagoCartonVendidoes", "CartonVendidoID", "dbo.tblCartonesVendidos");
            DropIndex("dbo.PagoCartonVendidoes", new[] { "TipoDePagoID" });
            DropIndex("dbo.PagoCartonVendidoes", new[] { "CartonVendidoID" });
            DropColumn("dbo.tblCartonesVendidos", "Pagos");
            DropTable("dbo.PagoCartonVendidoes");
        }
    }
}
