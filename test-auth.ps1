# Authentication API Test Script
# Make sure the API is running: dotnet run --project src/JobApplier.Api

Write-Host "=== Testing Authentication Endpoints ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000/api/auth"

# Test 1: Register a new user
Write-Host "1. Testing Registration..." -ForegroundColor Yellow
try {
    $registerData = @{
        email           = "john.doe@example.com"
        firstName       = "John"
        lastName        = "Doe"
        password        = "SecurePassword123!"
        confirmPassword = "SecurePassword123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/register" -Method Post -Body $registerData -ContentType "application/json" -UseBasicParsing
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Registration successful!" -ForegroundColor Green
    Write-Host "  User ID: $($json.user.id)"
    Write-Host "  Email: $($json.user.email)"
    Write-Host "  Name: $($json.user.firstName) $($json.user.lastName)"
    Write-Host "  Access Token: $($json.accessToken.Substring(0, 30))..."
    Write-Host "  Refresh Token: $($json.refreshToken.Substring(0, 30))..."
    
    $global:accessToken = $json.accessToken
    $global:refreshToken = $json.refreshToken
    Write-Host ""
}
catch {
    Write-Host "✗ Registration failed" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "  Response: $($reader.ReadToEnd())"
    }
    Write-Host ""
}

# Test 2: Login with the registered user
Write-Host "2. Testing Login..." -ForegroundColor Yellow
try {
    $loginData = @{
        email    = "john.doe@example.com"
        password = "SecurePassword123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/login" -Method Post -Body $loginData -ContentType "application/json" -UseBasicParsing
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Login successful!" -ForegroundColor Green
    Write-Host "  User ID: $($json.user.id)"
    Write-Host "  Email: $($json.user.email)"
    Write-Host "  Access Token: $($json.accessToken.Substring(0, 30))..."
    
    $global:accessToken = $json.accessToken
    $global:refreshToken = $json.refreshToken
    Write-Host ""
}
catch {
    Write-Host "✗ Login failed" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "  Response: $($reader.ReadToEnd())"
    }
    Write-Host ""
}

# Test 3: Refresh Token
Write-Host "3. Testing Token Refresh..." -ForegroundColor Yellow
try {
    $refreshData = @{
        refreshToken = $global:refreshToken
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/refresh" -Method Post -Body $refreshData -ContentType "application/json" -UseBasicParsing
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Token refresh successful!" -ForegroundColor Green
    Write-Host "  New Access Token: $($json.accessToken.Substring(0, 30))..."
    Write-Host "  New Refresh Token: $($json.refreshToken.Substring(0, 30))..."
    
    $global:accessToken = $json.accessToken
    $global:refreshToken = $json.refreshToken
    Write-Host ""
}
catch {
    Write-Host "✗ Token refresh failed" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "  Response: $($reader.ReadToEnd())"
    }
    Write-Host ""
}

# Test 4: Logout
Write-Host "4. Testing Logout..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $($global:accessToken)"
    }

    $response = Invoke-WebRequest -Uri "$baseUrl/logout" -Method Post -Headers $headers -UseBasicParsing
    $json = $response.Content | ConvertFrom-Json
    
    Write-Host "✓ Logout successful!" -ForegroundColor Green
    Write-Host "  Message: $($json.message)"
    Write-Host ""
}
catch {
    Write-Host "✗ Logout failed" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "  Response: $($reader.ReadToEnd())"
    }
    Write-Host ""
}

# Test 5: Try to refresh with revoked token (should fail)
Write-Host "5. Testing Revoked Token (should fail)..." -ForegroundColor Yellow
try {
    $refreshData = @{
        refreshToken = $global:refreshToken
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$baseUrl/refresh" -Method Post -Body $refreshData -ContentType "application/json" -UseBasicParsing
    Write-Host "✗ Unexpected success - revoked token should not work!" -ForegroundColor Red
    Write-Host ""
}
catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ Correctly rejected revoked token!" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Unexpected error" -ForegroundColor Red
    }
    $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
    Write-Host "  Response: $($reader.ReadToEnd())"
    Write-Host ""
}

Write-Host "=== Test Complete ===" -ForegroundColor Cyan
