using System.Security.Claims;

namespace Synerixis.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetSellerId(this ClaimsPrincipal user)
        {
            var sellerIdClaim = user.FindFirst("sellerId")?.Value;
            if (string.IsNullOrEmpty(sellerIdClaim) || !Guid.TryParse(sellerIdClaim, out var sellerId))
            {
                throw new UnauthorizedAccessException("Invalid or missing sellerId claim");
            }
            return sellerId;
        }
    }
}