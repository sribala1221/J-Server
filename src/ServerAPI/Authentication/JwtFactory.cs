using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerAPI.Services;

namespace ServerAPI.Authentication
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IUserPermissionsService _upService;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, IUserPermissionsService up) {
            _upService = up;
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<string> GenerateEncodedToken(ClaimsIdentity identity) {

            Claim[] claims = {
                new Claim(JwtRegisteredClaimNames.Sub, identity.Name),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                identity.FindFirst("personnelId"),
                identity.FindFirst("facilityId"),
                identity.FindFirst("personId"),
                identity.FindFirst("housingUnitListId"),
                identity.FindFirst("id"),
                identity.FindFirst("permissions"),
                identity.FindFirst("user_name"),
                identity.FindFirst("badge_number")

            };

            // Create the JWT security token and encode it.
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public ClaimsIdentity GenerateClaimsIdentity(string userName) {
            UserAccess user = _upService.GetUser(userName);
            IList<int> permissions = _upService.BuildPermissionNumbers(user.UserId);
           
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[] {
                new Claim("id", user.UserId.ToString()),
                new Claim("personnelId", user.PersonnelId.ToString()),
                new Claim("facilityId",  user.UserDefaultFacilityId.ToString()),
                new Claim("personId", user.PersonId.HasValue ? user.PersonId.Value.ToString() : ""),
                new Claim("housingUnitListId", user.HousingUnitListId.ToString()),
                new Claim("permissions", JsonConvert.SerializeObject(permissions ))
            });
        }

        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() -
            new DateTimeOffset(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero))
            .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

    }
}
