# AppAuthorization-AzureFunction

This repo contains two projects:

1. AppAuthorization-FunctionApp: An Azure function app which contains HTTP-triggered Azure function. This function is an API which enforces authorization on incoming requests. 
2. PowerShellProject: Contains ADAL and a script which generates token for applications whose audience is the Azure function app.

Primary goal of this implementation is to understand how applications can be authorized by an API. If an application contains necessary roles, then the application should be allowed to access the API, or else, it shouldn't be allowed (obviously).

To learn more about application authorization using Azure function, check out this blog which I have written about the same: https://rohannevrikar.wordpress.com/2020/09/16/authorization-of-applications-in-an-azure-function/


