# Recrea taxonomía + productos demo (sin imágenes ni covers).
# Detén la API antes si Visual Studio tiene bloqueados los DLL.

$env:ASPNETCORE_ENVIRONMENT = "Sqlite"
$env:SEED_RESET_CATALOG = "true"
$env:SEED_PRODUCT_COUNT = "50"

Push-Location "$PSScriptRoot\..\src\Ecommerce.Api"
try {
    dotnet run --launch-profile Sqlite
} finally {
    Pop-Location
}
