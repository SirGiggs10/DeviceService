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
    }
}
