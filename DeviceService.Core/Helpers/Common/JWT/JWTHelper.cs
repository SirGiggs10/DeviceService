using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DeviceService.Core.Helpers.Common.JWT
{
    public class JWTHelper
    {
        public static string JwtDecoderGetClaimValue(string token, string claimTypeToReturn)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var claimTypeValue = jwtSecurityToken?.Claims?.FirstOrDefault(claim => claim.Type == claimTypeToReturn)?.Value;

                return claimTypeValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GenerateJwtToken(List<Claim> claims, string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
