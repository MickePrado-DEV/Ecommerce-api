-- Migración: opciones globales (elimina ProductId por producto en product_options)
-- Ejecutar solo si prefieres SQL manual. En desarrollo, reiniciar la API suele recrear el esquema.

-- SQL Server: eliminar FK/columna legacy y crear tabla de asignaciones
IF COL_LENGTH('product_options', 'ProductId') IS NOT NULL
BEGIN
    IF OBJECT_ID('FK_product_options_products', 'F') IS NOT NULL
        ALTER TABLE product_options DROP CONSTRAINT FK_product_options_products;

    ALTER TABLE product_options DROP COLUMN ProductId;
END

IF COL_LENGTH('option_values', 'Description') IS NULL
    ALTER TABLE option_values ADD Description NVARCHAR(200) NULL;

IF OBJECT_ID('product_option_assignments', 'U') IS NULL
BEGIN
    CREATE TABLE product_option_assignments (
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductOptionId UNIQUEIDENTIFIER NOT NULL,
        FeaturesJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_product_option_assignments_FeaturesJson DEFAULT '[]',
        CONSTRAINT PK_product_option_assignments PRIMARY KEY (ProductId, ProductOptionId),
        CONSTRAINT FK_product_option_assignments_products FOREIGN KEY (ProductId) REFERENCES products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_product_option_assignments_product_options FOREIGN KEY (ProductOptionId) REFERENCES product_options(Id)
    );
END
