namespace SueñoCelestePagos.Dal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tblCartones",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Numero = c.String(),
                        Año = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.tblCartonesVendidos",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CartonID = c.Int(nullable: false),
                        ClienteID = c.Int(nullable: false),
                        FechaVenta = c.DateTime(nullable: false),
                        TipoDePagoID = c.Int(nullable: false),
                        PagoID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblCartones", t => t.CartonID, cascadeDelete: true)
                .ForeignKey("dbo.tblClientes", t => t.ClienteID, cascadeDelete: true)
                .ForeignKey("dbo.tblTiposDePagos", t => t.TipoDePagoID, cascadeDelete: true)
                .Index(t => t.CartonID)
                .Index(t => t.ClienteID)
                .Index(t => t.TipoDePagoID);
            
            CreateTable(
                "dbo.tblClientes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Apellido = c.String(),
                        Dni = c.String(maxLength: 8),
                        AreaCelular = c.String(maxLength: 5),
                        NumeroCelular = c.String(maxLength: 10),
                        CelularConfirmado = c.Boolean(nullable: false),
                        Email = c.String(),
                        EmailConfirmado = c.Boolean(nullable: false),
                        FechaNacimiento = c.DateTime(nullable: false),
                        Sexo = c.String(),
                        Calle = c.String(),
                        Altura = c.String(),
                        LocalidadID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblLocalidad", t => t.LocalidadID, cascadeDelete: true)
                .Index(t => t.LocalidadID);
            
            CreateTable(
                "dbo.tblLocalidad",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        CodPostal = c.String(),
                        ProvinciaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblProvicias", t => t.ProvinciaID, cascadeDelete: true)
                .Index(t => t.ProvinciaID);
            
            CreateTable(
                "dbo.tblProvicias",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.tblTiposDePagos",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Descripcion = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.tblCodigosTelefonos",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Codigo = c.Int(nullable: false),
                        Telefono = c.String(),
                        Expira = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.tblCuotasPlanDePago",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CartonVendidoID = c.Int(nullable: false),
                        NroCuota = c.Int(nullable: false),
                        CuotaPagada = c.Boolean(nullable: false),
                        PagoID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblCartonesVendidos", t => t.CartonVendidoID, cascadeDelete: true)
                .ForeignKey("dbo.tblPago", t => t.PagoID, cascadeDelete: true)
                .Index(t => t.CartonVendidoID)
                .Index(t => t.PagoID);
            
            CreateTable(
                "dbo.tblPago",
                c => new
                    {
                        id = c.Int(nullable: false),
                        type = c.String(),
                        state = c.String(),
                        created_at = c.DateTime(nullable: false),
                        external_reference = c.String(),
                        payer_name = c.String(),
                        payer_email = c.String(),
                        description = c.String(),
                        first_due_date = c.DateTime(nullable: false),
                        first_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        second_due_date = c.DateTime(nullable: false),
                        second_total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        barcode = c.String(),
                        checkout_url = c.String(),
                        barcode_url = c.String(),
                        pdf_url = c.String(),
                        excluded_channels = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.tblItemPago",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PagoID = c.Int(nullable: false),
                        quantity = c.Int(nullable: false),
                        description = c.String(),
                        amount = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.tblPago", t => t.PagoID, cascadeDelete: true)
                .Index(t => t.PagoID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tblCuotasPlanDePago", "PagoID", "dbo.tblPago");
            DropForeignKey("dbo.tblItemPago", "PagoID", "dbo.tblPago");
            DropForeignKey("dbo.tblCuotasPlanDePago", "CartonVendidoID", "dbo.tblCartonesVendidos");
            DropForeignKey("dbo.tblCartonesVendidos", "TipoDePagoID", "dbo.tblTiposDePagos");
            DropForeignKey("dbo.tblCartonesVendidos", "ClienteID", "dbo.tblClientes");
            DropForeignKey("dbo.tblClientes", "LocalidadID", "dbo.tblLocalidad");
            DropForeignKey("dbo.tblLocalidad", "ProvinciaID", "dbo.tblProvicias");
            DropForeignKey("dbo.tblCartonesVendidos", "CartonID", "dbo.tblCartones");
            DropIndex("dbo.tblItemPago", new[] { "PagoID" });
            DropIndex("dbo.tblCuotasPlanDePago", new[] { "PagoID" });
            DropIndex("dbo.tblCuotasPlanDePago", new[] { "CartonVendidoID" });
            DropIndex("dbo.tblLocalidad", new[] { "ProvinciaID" });
            DropIndex("dbo.tblClientes", new[] { "LocalidadID" });
            DropIndex("dbo.tblCartonesVendidos", new[] { "TipoDePagoID" });
            DropIndex("dbo.tblCartonesVendidos", new[] { "ClienteID" });
            DropIndex("dbo.tblCartonesVendidos", new[] { "CartonID" });
            DropTable("dbo.tblItemPago");
            DropTable("dbo.tblPago");
            DropTable("dbo.tblCuotasPlanDePago");
            DropTable("dbo.tblCodigosTelefonos");
            DropTable("dbo.tblTiposDePagos");
            DropTable("dbo.tblProvicias");
            DropTable("dbo.tblLocalidad");
            DropTable("dbo.tblClientes");
            DropTable("dbo.tblCartonesVendidos");
            DropTable("dbo.tblCartones");
        }
    }
}
