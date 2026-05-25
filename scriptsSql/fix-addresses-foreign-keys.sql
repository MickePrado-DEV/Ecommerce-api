-- Elimina FKs heredadas en addresses que no apuntan a users (causan error 547 al guardar).
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'ALTER TABLE addresses DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.addresses')
  AND fk.referenced_object_id IS NOT NULL
  AND fk.referenced_object_id <> OBJECT_ID(N'dbo.users');

IF LEN(@sql) > 0
BEGIN
    PRINT N'Eliminando FKs incorrectas en addresses...';
    EXEC sp_executesql @sql;
END
ELSE
    PRINT N'No hay FKs extra en addresses.';

-- Asegura FK correcta hacia users
IF OBJECT_ID(N'dbo.addresses', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE name = N'FK_addresses_users'
         AND parent_object_id = OBJECT_ID(N'dbo.addresses'))
BEGIN
    ALTER TABLE addresses
    ADD CONSTRAINT FK_addresses_users
        FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE;
    PRINT N'FK_addresses_users creada.';
END
