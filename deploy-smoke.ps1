$ErrorActionPreference = "Stop"

$baseUrl = if ($env:AFNEYGYM_BASE_URL) { $env:AFNEYGYM_BASE_URL.TrimEnd('/') } else { "http://localhost:5171" }

Write-Host "Smoke checks basliyor: $baseUrl"

$paths = @(
    "/",
    "/Account/Login",
    "/classes",
    "/health",
    "/ready"
)

foreach ($path in $paths) {
    $url = "$baseUrl$path"
    $response = Invoke-WebRequest $url -UseBasicParsing
    if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400) {
        throw "Smoke failed: $url -> $($response.StatusCode)"
    }
    Write-Host "OK $url -> $($response.StatusCode)"
}

Write-Host "Tum smoke kontrolleri basarili."

