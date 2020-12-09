namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class application : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblApplication",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        descripcion = c.String(),
                        listenerPago360 = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.tblPago", "ApplicationID", c => c.Int(nullable: false));
            CreateIndex("dbo.tblPago", "ApplicationID");
            AddForeignKey("dbo.tblPago", "ApplicationID", "dbo.tblApplication", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblPago", "ApplicationID", "dbo.tblApplication");
            DropIndex("dbo.tblPago", new[] { "ApplicationID" });
            DropColumn("dbo.tblPago", "ApplicationID");
            DropTable("dbo.tblApplication");
        }
    }
}
