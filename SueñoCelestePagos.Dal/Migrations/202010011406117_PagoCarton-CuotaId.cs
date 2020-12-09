namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PagoCartonCuotaId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PagoCartonVendidoes", "CuotaPlanID", c => c.Int(nullable: false));
            AddColumn("dbo.PagoCartonVendidoes", "CuotaDebitoID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PagoCartonVendidoes", "CuotaDebitoID");
            DropColumn("dbo.PagoCartonVendidoes", "CuotaPlanID");
        }
    }
}
