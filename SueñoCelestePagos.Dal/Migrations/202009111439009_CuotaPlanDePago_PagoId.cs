namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CuotaPlanDePago_PagoId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tblCuotasPlanDePago", "PagoID", "dbo.tblPago");
            DropIndex("dbo.tblCuotasPlanDePago", new[] { "PagoID" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.tblCuotasPlanDePago", "PagoID");
            AddForeignKey("dbo.tblCuotasPlanDePago", "PagoID", "dbo.tblPago", "id", cascadeDelete: true);
        }
    }
}
