param(
    [string]$Action = "add",
    [string]$Name = "Initial"
)

# Usage:
# .\migrate.ps1 -Action add -Name Initial
# .\migrate.ps1 -Action update

$proj = "..\CardioBackend.csproj"

if ($Action -eq "add") {
    dotnet ef migrations add $Name --project $proj --startup-project $proj
} elseif ($Action -eq "update") {
    dotnet ef database update --project $proj --startup-project $proj
} else {
    Write-Host "Unknown action. Use add or update"
}
