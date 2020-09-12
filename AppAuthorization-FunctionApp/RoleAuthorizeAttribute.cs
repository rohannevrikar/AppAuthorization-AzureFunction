using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionADApp
{
    class RoleAuthorizeAttribute : FunctionInvocationFilterAttribute
    {
        private readonly string[] _validRoles;
        private readonly string tenantId;
        private readonly string expectedAudience;
        private TokenValidationParameters _validationParameters = null;

        public RoleAuthorizeAttribute(params string[] validRoles)
        {
            _validRoles = validRoles;
            tenantId = GetEnvironmentVariable("tenantId");
            expectedAudience = GetEnvironmentVariable("clientId");
        }

        public override async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            try
            {
                HttpRequest request = executingContext.Arguments.First().Value as HttpRequest;
                if (request.Headers.ContainsKey("authorization"))
                {
                    var authHeader = AuthenticationHeaderValue.Parse(request.Headers["authorization"]);

                    if (authHeader != null &&
                        authHeader.Scheme.ToLower() == "bearer" &&
                        !string.IsNullOrEmpty(authHeader.Parameter))
                    {
                        if (_validationParameters == null)
                        {
                            // load the tenant-specific OpenID config from Azure
                            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            $"https://login.microsoftonline.com/{tenantId}/.well-known/openid-configuration",
                            new OpenIdConnectConfigurationRetriever());

                            var config = await configManager.GetConfigurationAsync();

                            _validationParameters = new TokenValidationParameters
                            {
                                IssuerSigningKeys = config.SigningKeys, // Use signing keys retrieved from Azure
                                ValidateAudience = true,                                
                                ValidAudience = expectedAudience, // audience MUST be the app ID of the API
                                ValidateIssuer = true,                                
                                ValidIssuer = config.Issuer, // use the issuer retrieved from Azure
                                ValidateLifetime = true,

                            };
                        }

                        var tokenHandler = new JwtSecurityTokenHandler();
                        SecurityToken jwtToken;

                        var result = tokenHandler.ValidateToken(authHeader.Parameter,
                            _validationParameters, out jwtToken); // validate the token, if ValidateToken did not throw an exception, then token is valid.

                        var tokenObject = tokenHandler.ReadToken(authHeader.Parameter) as JwtSecurityToken;

                        var roles = tokenObject.Claims.Where(e => e.Type == "roles").Select(e => e.Value); //retrive roles from the token

                        bool hasRole = roles.Intersect(_validRoles).Count() > 0; //Check if the token contains roles which are specified in attribute

                        if (!hasRole)
                            throw new AuthorizationException();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }

}
