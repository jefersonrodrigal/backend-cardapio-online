[CmdletBinding()]
param(
    [string]$AdminEmail = "admin@cardapioonline.local",
    [string]$AdminPassword,
    [string]$JwtSecret
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($AdminPassword)) {
    $AdminPassword = "Admin@123"
}

if ([string]::IsNullOrWhiteSpace($JwtSecret)) {
    $jwtSecretBytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($jwtSecretBytes)
    $JwtSecret = [Convert]::ToBase64String($jwtSecretBytes)
}

$salt = New-Object byte[] 16
$saltRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$saltRng.GetBytes($salt)
$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes(
    $AdminPassword,
    $salt,
    100000,
    [System.Security.Cryptography.HashAlgorithmName]::SHA256
)
$hash = $pbkdf2.GetBytes(32)
$passwordHash = "{0}.{1}" -f [Convert]::ToBase64String($salt), [Convert]::ToBase64String($hash)

dotnet user-secrets set "AdminAuth:Email" $AdminEmail
dotnet user-secrets set "AdminAuth:PasswordHash" $passwordHash
dotnet user-secrets set "AdminAuth:JwtIssuer" "CardapioOnline"
dotnet user-secrets set "AdminAuth:JwtAudience" "CardapioOnlineAdmin"
dotnet user-secrets set "AdminAuth:JwtSecret" $JwtSecret
dotnet user-secrets set "AdminAuth:TokenExpirationMinutes" "400"

Write-Host "User secrets configured for Development."
Write-Host "Admin e-mail: $AdminEmail"
Write-Host "Admin password: $AdminPassword"
