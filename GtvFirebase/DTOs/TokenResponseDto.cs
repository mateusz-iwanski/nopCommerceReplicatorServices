using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopCommerceReplicatorServices.GtvFirebase.DTOs
{
    /// <summary>
    /// Token request object.
    /// </summary>
    /// <param name="expires_in">Date when token expires</param>
    public record TokenResponseDto
    (
        [property: JsonProperty("token_type")] string TokenType,
        [property: JsonProperty("access_token")] string AccessToken,
        [property: JsonProperty("expires_in")] DateTime ExpiresIn
    ) : IBaseDto, IResponseDto;
}
