# Recrea BD ecommerce e inserta seed masivo
param(
    [ValidateSet('SqlServer', 'MySql', 'MariaDb', 'PostgreSql')]
    [string]$Provider = 'SqlServer',
    [string]$Server = "(localdb)\mssqllocaldb",
    [string]$MySqlHost = "127.0.0.1",
    [string]$MySqlUser = "root",
    [string]$MySqlPassword = "",
    [string]$PgHost = "127.0.0.1",
    [string]$PgUser = "postgres",
    [string]$PgPassword = "",
    [switch]$SchemaOnly,
    [switch]$SeedOnly
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Invoke-SqlServerFile {
    param([string]$File, [string]$Database = "master")
    Write-Host ">> $File" -ForegroundColor Cyan
    if ($Database -eq "master") {
        sqlcmd -S $Server -E -i $File
    } else {
        sqlcmd -S $Server -E -d $Database -i $File
    }
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed: $File" }
}

Push-Location $root
try {
    switch ($Provider) {
        'SqlServer' {
            if (-not $SeedOnly) { Invoke-SqlServerFile (Join-Path $root "schema.sqlserver.sql") }
            if (-not $SchemaOnly) { Invoke-SqlServerFile (Join-Path $root "seed.sqlserver.sql") -Database "ecommerce" }
        }
        'MySql' {
            $schema = Join-Path $root "schema.mysql.sql"
            $seed = Join-Path $root "seed.mysql.sql"
            if (-not $SeedOnly) {
                Write-Host ">> $schema" -ForegroundColor Cyan
                $args = @("-h", $MySqlHost, "-u", $MySqlUser)
                if ($MySqlPassword) { $args += "-p$MySqlPassword" }
                Get-Content $schema -Raw | & mysql @args
            }
            if (-not $SchemaOnly) {
                Write-Host ">> $seed" -ForegroundColor Cyan
                $args = @("-h", $MySqlHost, "-u", $MySqlUser, "ecommerce")
                if ($MySqlPassword) { $args = @("-h", $MySqlHost, "-u", $MySqlUser, "-p$MySqlPassword", "ecommerce") }
                Get-Content $seed -Raw | & mysql @args
            }
        }
        'MariaDb' {
            $schema = Join-Path $root "schema.mariadb.sql"
            $seed = Join-Path $root "seed.mariadb.sql"
            if (-not $SeedOnly) {
                Write-Host ">> $schema" -ForegroundColor Cyan
                $args = @("-h", $MySqlHost, "-u", $MySqlUser)
                if ($MySqlPassword) { $args += "-p$MySqlPassword" }
                Get-Content $schema -Raw | & mariadb @args
            }
            if (-not $SchemaOnly) {
                Write-Host ">> $seed" -ForegroundColor Cyan
                $args = @("-h", $MySqlHost, "-u", $MySqlUser, "ecommerce")
                if ($MySqlPassword) { $args = @("-h", $MySqlHost, "-u", $MySqlUser, "-p$MySqlPassword", "ecommerce") }
                Get-Content $seed -Raw | & mariadb @args
            }
        }
        'PostgreSql' {
            $schema = Join-Path $root "schema.postgresql.sql"
            $seed = Join-Path $root "seed.postgresql.sql"
            $env:PGPASSWORD = $PgPassword
            if (-not $SeedOnly) {
                Write-Host ">> $schema" -ForegroundColor Cyan
                psql -h $PgHost -U $PgUser -f $schema
            }
            if (-not $SchemaOnly) {
                Write-Host ">> $seed" -ForegroundColor Cyan
                psql -h $PgHost -U $PgUser -d ecommerce -f $seed
            }
            Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
        }
    }
    Write-Host "`nListo ($Provider). API + Postman listos para pruebas." -ForegroundColor Green
}
finally {
    Pop-Location
}
