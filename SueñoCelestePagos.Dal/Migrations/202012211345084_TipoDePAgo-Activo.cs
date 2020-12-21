namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TipoDePAgoActivo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tblTiposDePagos", "Activo", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tblTiposDePagos", "Activo");
        }
    }
}
