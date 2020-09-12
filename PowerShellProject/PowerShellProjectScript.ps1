#- Create a function app X with Azure AD app registration (A)
#- Implement a HTTP trigger on app X
#- Build a PS script Y to call X using AD authentication (client credentials) with app registration (B)
#- Allow B to have permissions to call A (App Role)
#- Build a PS script Z to call X using AD authentication (client credentials) with app registration (C)
#- Do not provide permissions for C to call A
#- Calls from Y to X should succeed
#- Calls from Z to X should fail with a 403

# Load ADAL
Add-Type -Path "ADAL\Microsoft.IdentityModel.Clients.ActiveDirectory.dll"

# Application and Tenant Configuration
$clientIdC = "<clientIdOfAppC>" #This app does not have permission
$clientIdB = "<clientIdOfAppB>" #This app contains the permission to call the function app

$tenantId = "<tenantId>"
$resourceId = "<resourceId>" #ClientId of App registration A, against which other apps have to be authorized
$login = "https://login.microsoftonline.com"

$secretC = "<secretOfAppC>" 
$secretB = "<secretOfAppB>" 

# Get an Access Token with ADAL
$clientCredential = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential($clientIdC,$secretC) #change these variables to test authorization of different apps. B should be authorized, C should throw 403 error
$authContext = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext("{0}/{1}" -f $login,$tenantId)
$authenticationResult = $authContext.AcquireToken($resourceId, $clientcredential)
$token = $authenticationResult.AccessToken

$headers = @{ 
    "Authorization" = ("Bearer {0}" -f $token);
    "Content-Type" = "application/json";
}

# Call the Azure function
Invoke-RestMethod -Method Get -Uri ("http://localhost:7071/api/Function1" -f $resourceId,$tenantId)  -Headers $headers 