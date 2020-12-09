namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DebitoId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CuotasDebitoes", "DebitoID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CuotasDebitoes", "DebitoID");
        }
    }
}
