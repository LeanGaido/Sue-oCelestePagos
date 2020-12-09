namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MesyAño_CuotaPlanDePago : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCuotasPlanDePago", "MesCuota", c => c.Int(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "AñoCuota", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCuotasPlanDePago", "AñoCuota");
            DropColumn("dbo.tblCuotasPlanDePago", "MesCuota");
        }
    }
}
