using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using IdentityModel;
using IdentityServer4.Extensions;

namespace IdentityServer4.Models;

public class LoginRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentRequest"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="loginRequestId">The loginRequestId.</param>
    public LoginRequest(AuthorizationRequest request, string loginRequestId)
    {
        ClientId = request.Client.ClientId;
        Nonce = request.Parameters[OidcConstants.AuthorizeRequest.Nonce];
        ScopesRequested = request.Parameters[OidcConstants.AuthorizeRequest.Scope].ParseScopesString();
        LoginRequestId = loginRequestId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentRequest"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    /// <param name="loginRequestId">The loginRequestId.</param>
    public LoginRequest(NameValueCollection parameters, string loginRequestId)
    {
        ClientId = parameters[OidcConstants.AuthorizeRequest.ClientId];
        Nonce = parameters[OidcConstants.AuthorizeRequest.Nonce];
        ScopesRequested = parameters[OidcConstants.AuthorizeRequest.Scope].ParseScopesString();
        LoginRequestId = loginRequestId;
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string ClientId { get; set; }

    public string LoginRequestId { get; set; }

    /// <summary>
    /// Gets or sets the nonce.
    /// </summary>
    /// <value>
    /// The nonce.
    /// </value>
    public string Nonce { get; set; }

    /// <summary>
    /// Gets or sets the scopes requested.
    /// </summary>
    /// <value>
    /// The scopes requested.
    /// </value>
    public IEnumerable<string> ScopesRequested { get; set; }

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public string Id
    {
        get
        {
            var normalizedScopes = ScopesRequested?.OrderBy(x => x).Distinct().Aggregate((x, y) => x + "," + y);
            var value = $"{ClientId}:{LoginRequestId}:{Nonce}:{normalizedScopes}";

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = sha.ComputeHash(bytes);

                return Base64Url.Encode(hash);
            }
        }
    }
}