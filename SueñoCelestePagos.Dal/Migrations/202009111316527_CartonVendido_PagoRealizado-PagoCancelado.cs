namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CartonVendido_PagoRealizadoPagoCancelado : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCartonesVendidos", "PagoRealizdo", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblCartonesVendidos", "PagoCancelado", c => c.Boolean(nullable: false));
            AddColumn("dbo.tblCartonesVendidos", "FechaPago", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCartonesVendidos", "FechaPago");
            DropColumn("dbo.tblCartonesVendidos", "PagoCancelado");
            DropColumn("dbo.tblCartonesVendidos", "PagoRealizdo");
        }
    }
}
