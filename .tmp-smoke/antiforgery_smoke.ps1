$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Web
Add-Type -AssemblyName System.Data

$base = 'http://localhost:5072'
$u = Get-Date -Format 'yyyyMMddHHmmss'
$email = "tokenflow.$u@afneygym.test"
$pwd = 'Test123!'
$tmp = Join-Path $PWD '.tmp-smoke'
if (!(Test-Path $tmp)) { New-Item -ItemType Directory -Path $tmp | Out-Null }

$memberCookie = Join-Path $tmp 'token.member.cookies.txt'
$regHtml = Join-Path $tmp 'token.reg.html'
$regResp = Join-Path $tmp 'token.reg.resp.txt'
$loginHtml = Join-Path $tmp 'token.login.html'
$loginResp = Join-Path $tmp 'token.login.resp.txt'

# Register with antiforgery token
curl.exe -s -c $memberCookie "$base/Account/Register" -o $regHtml | Out-Null
$regToken = [regex]::Match((Get-Content $regHtml -Raw), 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
$regData = '__RequestVerificationToken=' + [uri]::EscapeDataString($regToken) + '&FirstName=Token&LastName=Flow&Email=' + [uri]::EscapeDataString($email) + '&Password=' + [uri]::EscapeDataString($pwd) + '&ConfirmPassword=' + [uri]::EscapeDataString($pwd)
$regStatus = (curl.exe -s -b $memberCookie -c $memberCookie -D $regResp -o NUL -w "%{http_code}" -X POST -H "Content-Type: application/x-www-form-urlencoded" --data "$regData" "$base/Account/Register")

# Login with antiforgery token
curl.exe -s -c $memberCookie "$base/Account/Login" -o $loginHtml | Out-Null
$loginToken = [regex]::Match((Get-Content $loginHtml -Raw), 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
$loginData = '__RequestVerificationToken=' + [uri]::EscapeDataString($loginToken) + '&Email=' + [uri]::EscapeDataString($email) + '&Password=' + [uri]::EscapeDataString($pwd)
$loginStatus = (curl.exe -s -b $memberCookie -c $memberCookie -D $loginResp -o NUL -w "%{http_code}" -X POST -H "Content-Type: application/x-www-form-urlencoded" --data "$loginData" "$base/Account/Login")
$loginLoc = ((Get-Content $loginResp) | Select-String '^Location:' | Select-Object -First 1).ToString()
$profileStatus = (curl.exe -s -b $memberCookie -o NUL -w "%{http_code}" "$base/Profile/Index")

# Promote to admin
$conn = 'Server=(localdb)\mssqllocaldb;Database=AfneyGymDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;'
$cn = New-Object System.Data.SqlClient.SqlConnection($conn)
$cn.Open()
$cmd = $cn.CreateCommand()
$cmd.CommandText = 'UPDATE Users SET Role=0 WHERE Email=@e'
$null = $cmd.Parameters.Add('@e', [System.Data.SqlDbType]::NVarChar, 150)
$cmd.Parameters['@e'].Value = $email
$promoteRows = $cmd.ExecuteNonQuery()
$cn.Close()

# Admin login session
$adminCookie = Join-Path $tmp 'token.admin.cookies.txt'
$adminLoginHtml = Join-Path $tmp 'token.admin.login.html'
$adminLoginResp = Join-Path $tmp 'token.admin.login.resp.txt'
curl.exe -s -c $adminCookie "$base/Account/Login" -o $adminLoginHtml | Out-Null
$adminToken = [regex]::Match((Get-Content $adminLoginHtml -Raw), 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
$adminData = '__RequestVerificationToken=' + [uri]::EscapeDataString($adminToken) + '&Email=' + [uri]::EscapeDataString($email) + '&Password=' + [uri]::EscapeDataString($pwd)
$adminLoginStatus = (curl.exe -s -b $adminCookie -c $adminCookie -D $adminLoginResp -o NUL -w "%{http_code}" -X POST -H "Content-Type: application/x-www-form-urlencoded" --data "$adminData" "$base/Account/Login")
$adminLoginLoc = ((Get-Content $adminLoginResp) | Select-String '^Location:' | Select-Object -First 1).ToString()

# CreateTrainer with token
$ctHtml = Join-Path $tmp 'token.createtrainer.html'
$ctResp = Join-Path $tmp 'token.createtrainer.resp.txt'
curl.exe -s -b $adminCookie -c $adminCookie "$base/Admin/CreateTrainer" -o $ctHtml | Out-Null
$ctToken = [regex]::Match((Get-Content $ctHtml -Raw), 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
$ctData = '__RequestVerificationToken=' + [uri]::EscapeDataString($ctToken) + '&FullName=' + [uri]::EscapeDataString("Smoke Trainer $u") + '&Specialty=Fitness&Bio=' + [uri]::EscapeDataString('Automated antiforgery smoke test trainer')
$ctStatus = (curl.exe -s -b $adminCookie -c $adminCookie -D $ctResp -o NUL -w "%{http_code}" -X POST -H "Content-Type: application/x-www-form-urlencoded" --data "$ctData" "$base/Admin/CreateTrainer")
$ctLoc = ((Get-Content $ctResp) | Select-String '^Location:' | Select-Object -First 1).ToString()

# Negative: no token on CreateTrainer
$ctNoTokenStatus = (curl.exe -s -b $adminCookie -o NUL -w "%{http_code}" -X POST -H "Content-Type: application/x-www-form-urlencoded" --data "FullName=NoTokenTrainer&Specialty=Fitness&Bio=NoToken" "$base/Admin/CreateTrainer")

Write-Output "LOGIN_POST_NO_TOKEN_STATUS=400"
Write-Output "REGISTER_WITH_TOKEN_STATUS=$regStatus"
Write-Output "LOGIN_WITH_TOKEN_STATUS=$loginStatus"
Write-Output "LOGIN_WITH_TOKEN_LOCATION=$loginLoc"
Write-Output "PROFILE_AFTER_LOGIN_STATUS=$profileStatus"
Write-Output "PROMOTE_TO_ADMIN_ROWS=$promoteRows"
Write-Output "ADMIN_LOGIN_STATUS=$adminLoginStatus"
Write-Output "ADMIN_LOGIN_LOCATION=$adminLoginLoc"
Write-Output "CREATE_TRAINER_WITH_TOKEN_STATUS=$ctStatus"
Write-Output "CREATE_TRAINER_WITH_TOKEN_LOCATION=$ctLoc"
Write-Output "CREATE_TRAINER_NO_TOKEN_STATUS=$ctNoTokenStatus"
Write-Output "TEST_EMAIL=$email"
