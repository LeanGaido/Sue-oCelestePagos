namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cbu_holder_id_numberlong : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Adhesions", "cbu_holder_id_number", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Adhesions", "cbu_holder_id_number", c => c.Int(nullable: false));
        }
    }
}
