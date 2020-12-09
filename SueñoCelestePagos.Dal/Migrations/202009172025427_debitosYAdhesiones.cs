namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class debitosYAdhesiones : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Debitoes",
                c => new
                    {
                        id = c.Int(nullable: false),
                        type = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        first_due_date = c.DateTime(nullable: false),
                        first_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SecondDueDate = c.DateTime(nullable: false),
                        SecondTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        description = c.String(),
                        AdhesionId = c.Int(nullable: false),
                        CuotaDebitoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Adhesions", t => t.AdhesionId, cascadeDelete: true)
                .ForeignKey("dbo.CuotasDebitoes", t => t.CuotaDebitoId, cascadeDelete: true)
                .Index(t => t.AdhesionId)
                .Index(t => t.CuotaDebitoId);
            
            AddColumn("dbo.tblCartonesVendidos", "CantCuotas", c => c.Int());
            AddColumn("dbo.CuotasDebitoes", "FechaPago", c => c.DateTime());
            AddColumn("dbo.CuotasDebitoes", "AdhesionID", c => c.Int(nullable: false));
            DropColumn("dbo.CuotasDebitoes", "PagoID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CuotasDebitoes", "PagoID", c => c.Int(nullable: false));
            DropForeignKey("dbo.Debitoes", "CuotaDebitoId", "dbo.CuotasDebitoes");
            DropForeignKey("dbo.Debitoes", "AdhesionId", "dbo.Adhesions");
            DropIndex("dbo.Debitoes", new[] { "CuotaDebitoId" });
            DropIndex("dbo.Debitoes", new[] { "AdhesionId" });
            DropColumn("dbo.CuotasDebitoes", "AdhesionID");
            DropColumn("dbo.CuotasDebitoes", "FechaPago");
            DropColumn("dbo.tblCartonesVendidos", "CantCuotas");
            DropTable("dbo.Debitoes");
        }
    }
}
