param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$Database = "swipemate",
    [string]$Username = "postgres",
    [string]$OutputPath = ""
)

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $fileName = "swipemate-{0}.backup" -f (Get-Date -Format "yyyyMMdd-HHmmss")
    $OutputPath = Join-Path $PSScriptRoot $fileName
}

$pgDump = Get-Command pg_dump -ErrorAction SilentlyContinue
if (-not $pgDump) {
    throw "pg_dump was not found. Add PostgreSQL bin folder to PATH or run this from pgAdmin/PostgreSQL tools shell."
}

& $pgDump.Source -h $HostName -p $Port -U $Username -d $Database -F c -f $OutputPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Database backup created: $OutputPath"
