using System.Diagnostics;
using System.Web;
using SpotifyAPI.Web;

const string redirectUri = "http://127.0.0.1:5000/callback";

Console.Write("Enter your Spotify Client ID: ");
var clientId = Console.ReadLine()?.Trim();
Console.Write("Enter your Spotify Client Secret: ");
var clientSecret = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
{
    Console.WriteLine("Client ID and Secret are required.");
    return;
}

var loginRequest = new LoginRequest(new Uri(redirectUri), clientId, LoginRequest.ResponseType.Code)
{
    Scope = new[]
    {
        Scopes.PlaylistModifyPublic,
        Scopes.PlaylistModifyPrivate,
        Scopes.PlaylistReadPrivate,
        Scopes.UserReadPlaybackPosition,
    }
};

var uri = loginRequest.ToUri();
Console.WriteLine($"\nOpen this URL in your browser:\n\n{uri}\n");

try
{
    Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
}
catch { }

Console.WriteLine("After you approve, the browser will redirect to a URL that fails to load.");
Console.WriteLine("That's expected! Copy the FULL URL from your browser's address bar and paste it here.\n");
Console.Write("Paste the redirect URL: ");
var fullUrl = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(fullUrl))
{
    Console.WriteLine("No URL provided.");
    return;
}

var queryParams = HttpUtility.ParseQueryString(new Uri(fullUrl).Query);
var code = queryParams["code"];

if (string.IsNullOrEmpty(code))
{
    var error = queryParams["error"];
    Console.WriteLine($"No authorization code found in URL. Error: {error ?? "unknown"}");
    return;
}

Console.WriteLine("Exchanging authorization code for tokens...");

var tokenResponse = await new OAuthClient().RequestToken(
    new AuthorizationCodeTokenRequest(clientId, clientSecret, code, new Uri(redirectUri))
);

Console.WriteLine("\n========================================");
Console.WriteLine("SUCCESS! Here is your refresh token:\n");
Console.WriteLine(tokenResponse.RefreshToken);
Console.WriteLine("\n========================================");
Console.WriteLine("\nAdd this to your local.settings.json as Spotify__RefreshToken");
