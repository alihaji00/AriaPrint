using AriaPrint.Controllers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiSecurity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public const string HeaderName = "X-Api-Key";


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            {
                return;
            }

            var request = context.HttpContext.Request;

            if (!request.Headers.TryGetValue(HeaderName, out var providedKey) ||
                providedKey.Count != 1 || string.IsNullOrWhiteSpace(providedKey.ToString()))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var apiKey = providedKey.ToString();
            CurrentUser? user = null;

            // To validate a JWT sent in the header, use this instead of the database lookup:
            // user = JwtToken.Validate(apiKey);

            ///////////////////////////////////////


            if (apiKey.Length == 36)
            {
                ClassSQL sql = new();

                var query = @"
                    SELECT *
                    FROM [dbo].[ApiPersonsTbl]
                    WHERE apiKey = @ApiKey";

                var result = sql.SelectOneAsync(query, new { ApiKey = apiKey }).Result;

                if (result != null)
                {
                    user = new CurrentUser();

                    user.Id = Convert.ToInt32(result["Id"]);

                    user.UserName = Convert.ToString(result["Id"]);

                    user.FirstName = "";

                    user.LastName = "";

                    user.UserType = "user";
                }
            }

            if (apiKey.IndexOf(".") > -1)
            {
                user = JwtToken.Validate(apiKey);
            }


            //if (apiKey == "66386637")
            //{

            //    user = new CurrentUser();

            //    user.Id = 1;

            //}

            //if (apiKey == "66386638")
            //{

            //    user = new CurrentUser();

            //    user.Id = 2;

            //}

            //if (apiKey == "66386639")
            //{

            //    user = new CurrentUser();

            //    user.Id = 3;

            //}

            ///////////////////////////////////////


            if (user == null || user.Id <= 0)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.Items["CurrentUser"] = user;
        }



    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CookieAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string LoginPath = "/Account/Login";
        private const string Cookie = "ApiKey";
        private const bool EnableAntiforgeryValidation = true;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            CurrentUser? user = null;

            // Read the cookie and validate its JWT before accepting user.



            ///////////////////////////////////////


            var token = AuthCookie.GetCookie(request, Cookie);

            ////////////////////////////

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Result = new LocalRedirectResult(LoginPath);
                return;
            }

            ///////////////////////////

            user = JwtToken.Validate(token);

            // Add any additional database checks here before accepting user.

            ///////////////////////////////////////

            if (user == null || user.Id <= 0 || string.IsNullOrWhiteSpace(user.UserName))
            {
                context.Result = new LocalRedirectResult(LoginPath);
                return;
            }

            if (EnableAntiforgeryValidation && !HttpMethods.IsGet(request.Method) &&
                !HttpMethods.IsHead(request.Method) && !HttpMethods.IsOptions(request.Method))
            {
                var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

                try
                {
                    await antiforgery.ValidateRequestAsync(context.HttpContext);
                }
                catch (AntiforgeryValidationException)
                {
                    context.Result = new BadRequestResult();
                    return;
                }
            }

            context.HttpContext.Items["CurrentUser"] = user;
        }





    }


    public sealed class CurrentUser
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string UserType { get; set; } = string.Empty;

        public object? Data { get; set; }
    }

    public static class JwtToken
    {
        private const string SecretKey = "QbFgmIjD7q3r090Hz94C4f/0BIQuiaIRoaQwSw65o4c=";
        private const string Issuer = "MyLoginServer";
        private const string Audience = "MyApi";

        private static readonly SymmetricSecurityKey signingKey = new(Convert.FromBase64String(SecretKey));

        public static string Create(CurrentUser user, int expiresInMinutes = 60)
        {
            if (user == null || user.Id <= 0 || string.IsNullOrWhiteSpace(user.UserName))
            {
                throw new ArgumentException("A valid user ID and username are required.", nameof(user));
            }

            if (expiresInMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expiresInMinutes));
            }

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: new[]
                {
                    new Claim("user", JsonSerializer.Serialize(user)),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
                },
                notBefore: now,
                expires: now.AddMinutes(expiresInMinutes),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static CurrentUser? Validate(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    RequireSignedTokens = true,
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = Audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwt = (JwtSecurityToken)validatedToken;
                var userJson = jwt.Claims.FirstOrDefault(claim => claim.Type == "user")?.Value;

                if (string.IsNullOrWhiteSpace(userJson))
                {
                    return null;
                }

                // An object stored in Data is deserialized as a JsonElement.
                var user = JsonSerializer.Deserialize<CurrentUser>(userJson);

                if (user == null || user.Id <= 0 || string.IsNullOrWhiteSpace(user.UserName))
                {
                    return null;
                }

                return user;
            }
            catch (Exception exception) when (exception is SecurityTokenException or ArgumentException or JsonException)
            {
                return null;
            }
        }
    }

    public static class AuthCookie
    {
        public static void SetCookie(HttpResponse response, string cookieName, CurrentUser user, int expiresInMinutes = 60)
        {
            var token = JwtToken.Create(user, expiresInMinutes);

            response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes)
            });
        }

        public static string? GetCookie(HttpRequest request, string cookieName)
        {
            return request.Cookies[cookieName];
        }

        public static void DeleteCookie(HttpResponse response, string cookieName)
        {
            response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
        }
    }
}
