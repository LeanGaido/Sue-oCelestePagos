namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Carton_Precio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblCartones", "Precio", c => c.Single(nullable: false));
            AddColumn("dbo.tblCuotasPlanDePago", "PrecioCuota", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblCuotasPlanDePago", "PrecioCuota");
            DropColumn("dbo.tblCartones", "Precio");
        }
    }
}
