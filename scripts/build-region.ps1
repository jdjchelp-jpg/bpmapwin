param(
    [string]$RegionId = "jamaica",
    [string]$InputPbf = "$(Join-Path $PSScriptRoot '..\Maps\jamaica.osm.pbf')",
    [string]$OutputRoot = "",
    [string]$TilemakerConfig = "$(Join-Path $PSScriptRoot 'tilemaker-config.json')",
    [string]$TilemakerProcess = "$(Join-Path $PSScriptRoot 'tilemaker-process.lua')"
)
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
if ([string]::IsNullOrWhiteSpace($OutputRoot)) { $OutputRoot = Join-Path $PSScriptRoot "..\Maps\$RegionId" }

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Missing '$Name'. Install the tools described in scripts/README.md and run this script again."
    }
}

if (-not (Test-Path -LiteralPath $InputPbf)) { throw "PBF not found: $InputPbf" }
Require-Command "osmium"
Require-Command "tilemaker"
Require-Command "python"
osmium fileinfo $InputPbf

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
$searchGeoJson = Join-Path $OutputRoot "$RegionId-search.geojson"
$searchPbf = Join-Path $OutputRoot "$RegionId-search.osm.pbf"
$tiles = Join-Path $OutputRoot "$RegionId.mbtiles"
$searchDb = Join-Path $OutputRoot "$RegionId-search.sqlite"

Write-Host "[1/3] Exporting searchable OSM names and POIs..."
osmium tags-filter $InputPbf nwr/name -o $searchPbf --overwrite
osmium export $searchPbf -o $searchGeoJson --overwrite
python (Join-Path $PSScriptRoot 'import-search.py') $searchGeoJson $searchDb

Write-Host "[2/3] Building MapLibre vector tiles..."
tilemaker --input $InputPbf --output $tiles --config $TilemakerConfig --process $TilemakerProcess --store $OutputRoot

Write-Host "[3/3] Routing graph..."
Write-Host "Routing graph generation is delegated to the selected engine. For Itinero, run the RegionGraphBuilder project after installing the .NET SDK; for OSRM, use its extract/partition/customize commands."

$manifest = [ordered]@{
    regionId = $RegionId
    source = (Resolve-Path $InputPbf).Path
    vectorTiles = (Resolve-Path $tiles).Path
    searchDatabase = (Resolve-Path $searchDb).Path
    routing = "pending"
    builtUtc = [DateTime]::UtcNow.ToString('O')
}
$manifest | ConvertTo-Json | Set-Content (Join-Path $OutputRoot 'manifest.json')
Write-Host "Complete. Manifest: $(Join-Path $OutputRoot 'manifest.json')"
