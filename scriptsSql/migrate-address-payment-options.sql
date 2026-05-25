-- Migración incremental (ejecutar sobre BD existente)
IF COL_LENGTH('addresses', 'Type') IS NULL
BEGIN
    ALTER TABLE addresses ADD
        Type INT NOT NULL CONSTRAINT DF_addresses_Type DEFAULT 1,
        ContactName NVARCHAR(120) NULL,
        ExternalNumber NVARCHAR(20) NULL,
        InternalNumber NVARCHAR(20) NULL,
        Neighborhood NVARCHAR(120) NULL,
        Municipality NVARCHAR(120) NULL,
        References NVARCHAR(500) NULL,
        DeliveryInstructions NVARCHAR(500) NULL,
        Latitude DECIMAL(9,6) NULL,
        Longitude DECIMAL(9,6) NULL;
END

IF COL_LENGTH('addresses', 'IsDefault') IS NULL
    ALTER TABLE addresses ADD IsDefault BIT NOT NULL CONSTRAINT DF_addresses_IsDefault DEFAULT 0;

IF COL_LENGTH('payments', 'CardHolderName') IS NULL
    ALTER TABLE payments ADD CardHolderName NVARCHAR(120) NULL;

IF COL_LENGTH('product_options', 'OptionType') IS NULL
    ALTER TABLE product_options ADD OptionType INT NOT NULL CONSTRAINT DF_product_options_OptionType DEFAULT 1;
