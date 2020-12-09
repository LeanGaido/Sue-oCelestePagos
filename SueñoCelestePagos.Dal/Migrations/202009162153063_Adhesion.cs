namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Adhesion : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblPago", "ApplicationID", "dbo.tblApplication");
            DropIndex("dbo.tblPago", new[] { "ApplicationID" });
            CreateTable(
                "dbo.Adhesions",
                c => new
                    {
                        id = c.Int(nullable: false),
                        external_reference = c.String(),
                        adhesion_holder_name = c.String(),
                        email = c.String(),
                        cbu_holder_name = c.String(),
                        cbu_holder_id_number = c.Int(nullable: false),
                        cbu_number = c.String(),
                        bank = c.String(),
                        description = c.String(),
                        short_description = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.CuotasDebitoes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CartonVendidoID = c.Int(nullable: false),
                        NroCuota = c.Int(nullable: false),
                        MesCuota = c.Int(nullable: false),
                        AñoCuota = c.Int(nullable: false),
                        PrimerVencimiento = c.DateTime(nullable: false),
                        PrimerPrecioCuota = c.Single(nullable: false),
                        SeguntoVencimiento = c.DateTime(nullable: false),
                        SeguntoPrecioCuota = c.Single(nullable: false),
                        CuotaPagada = c.Boolean(nullable: false),
                        PagoID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblCartonesVendidos", t => t.CartonVendidoID, cascadeDelete: true)
                .Index(t => t.CartonVendidoID);
            
            AddColumn("dbo.tblCuotasPlanDePago", "PrimerVencimiento", c => c.DateTime(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "PrimerPrecioCuota", c => c.Single(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "SeguntoVencimiento", c => c.DateTime(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "SeguntoPrecioCuota", c => c.Single(nullable: false));
            DropColumn("dbo.tblCartonesVendidos", "PagoID");
            DropColumn("dbo.tblCuotasPlanDePago", "PrecioCuota");
            DropColumn("dbo.tblPago", "ApplicationID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.tblPago", "ApplicationID", c => c.Int(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "PrecioCuota", c => c.Single(nullable: false));
            AddColumn("dbo.tblCartonesVendidos", "PagoID", c => c.Int());
            DropForeignKey("dbo.CuotasDebitoes", "CartonVendidoID", "dbo.tblCartonesVendidos");
            DropIndex("dbo.CuotasDebitoes", new[] { "CartonVendidoID" });
            DropColumn("dbo.tblCuotasPlanDePago", "SeguntoPrecioCuota");
            DropColumn("dbo.tblCuotasPlanDePago", "SeguntoVencimiento");
            DropColumn("dbo.tblCuotasPlanDePago", "PrimerPrecioCuota");
            DropColumn("dbo.tblCuotasPlanDePago", "PrimerVencimiento");
            DropTable("dbo.CuotasDebitoes");
            DropTable("dbo.Adhesions");
            CreateIndex("dbo.tblPago", "ApplicationID");
            AddForeignKey("dbo.tblPago", "ApplicationID", "dbo.tblApplication", "id", cascadeDelete: true);
        }
    }
}
