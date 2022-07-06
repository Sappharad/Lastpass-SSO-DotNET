using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Web.WebView2.Core;

namespace LastPassSSOHelper
{
    internal partial class SingleSignOnView : Form
    {
        private JsonObject? _lastpass;
        private JsonObject? _oidc;
        private string? _loginLink;
        private int _companyId;
        private string? _idToken;
        private string? _k1;
        private string? _k2;
        private string? _fragmentId;
        private readonly HttpClientHandler _handler;
        private readonly CookieContainer _cookieJar;
        internal string? Email { get; private set; }
        internal string? Password { get; private set; }
        internal string? Fragment { get; private set; }

        public SingleSignOnView()
        {
            InitializeComponent();
            wvContent.CoreWebView2InitializationCompleted += WebViewInitialized;
            _cookieJar = new CookieContainer();
            _handler = new HttpClientHandler { CookieContainer = _cookieJar };
        }

        private async void SingleSignOnView_Shown(object sender, EventArgs e)
        {
            CoreWebView2EnvironmentOptions cwvopts = new CoreWebView2EnvironmentOptions()
            {
                AllowSingleSignOnUsingOSPrimaryAccount = true
            };
            if (Directory.Exists(LastPassSSOHelper.ProfileDirectory))
            {
                Directory.CreateDirectory(LastPassSSOHelper.ProfileDirectory);
            }
            CoreWebView2Environment environment =
                await CoreWebView2Environment.CreateAsync(LastPassSSOHelper.BrowserFiles,
                    LastPassSSOHelper.ProfileDirectory, cwvopts);
            await wvContent.EnsureCoreWebView2Async(environment);
            wvContent.Source = new Uri("about:blank");
        }

        private async void WebViewInitialized(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            Email = System.DirectoryServices.AccountManagement.UserPrincipal.Current.EmailAddress.ToLower();
            wvContent.CoreWebView2.NavigationStarting += BeforeRequest;
            if(!(await CheckLoginAsync()))
            {
                DialogResult = DialogResult.Cancel;
                Close(); //Failed
            }
        }

        private void ContinueLogin()
        {
            var endpoint = new Uri((string)_oidc["authorization_endpoint"]);
            var redirectUri = "https://accounts.lastpass.com/federated/oidcredirect.html";
            var clientId = _lastpass["OpenIDConnectClientId"];
            var responseType = "id_token token";
            var scope = "openid email profile";
            var state = Guid.NewGuid().ToString().Replace("-", ""); //Value can be random
            var nonce = Guid.NewGuid().ToString().Replace("-", "");
            var loginUrl = _oidc["authorization_endpoint"] +
                "?client_id=" + clientId +
                "&redirect_uri=" + redirectUri +
                "&response_type=" + responseType +
                "&scope=" + scope +
                "&state=" + state +
                "&nonce=" + nonce +
                "&login_hint=" + Email;
            _loginLink = loginUrl;
        }

        private async Task<bool> BeginLogin()
        {
            if ((int)_lastpass["type"] == 3)
            {
                _companyId = (int)_lastpass["CompanyId"];
                _oidc = await QueryOidc((string)_lastpass["OpenIDConnectAuthority"]);
                return true;
            }
            else
            {
                MessageBox.Show("Account is not OAuth SSO!");
            }
            return false;
        }

        private async Task<bool> CheckLoginAsync()
        {
            if (!string.IsNullOrEmpty(Email) && Email.IndexOf('@') > 0)
            {
                _lastpass = await QueryEmail(Email);
                if(_lastpass != null && (int)(_lastpass["type"]) > 0)
                {
                    if(await BeginLogin())
                    {
                        ContinueLogin();
                        wvContent.CoreWebView2.Navigate(_loginLink);
                    }
                }
                return true;
            }
            return false;
        }

        private async void BeforeRequest(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if(e.Uri.StartsWith("https://accounts.lastpass.com/federated/oidcredirect.html", StringComparison.OrdinalIgnoreCase))
            {
                string[] pieces = e.Uri.Split('#');
                if (pieces.Length > 1)
                {
                    await ConsumeFragment(pieces[1]);
                    e.Cancel = true;
                    wvContent.CoreWebView2.Navigate("about:blank"); //We don't need the browser anymore
                    await GetKeyAsync();
                }
                //If the URL can't be consumed we should still close the hidden window
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private async Task<JsonObject?> QueryEmail(string arg)
        {
            const string api = "https://lastpass.com/lmiapi/login/type?";
            string url = $"{api}username={arg}";

            HttpClient client = new HttpClient(_handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            
            HttpResponseMessage? reply = await client.SendAsync(request);
            if (reply.IsSuccessStatusCode)
            {
                string data = await reply.Content.ReadAsStringAsync();
                return JsonNode.Parse(data).AsObject();
            }
            return null;
        }

        private async Task<JsonObject?> QueryOidc(string arg)
        {
            HttpClient client = new HttpClient(_handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, arg);
            request.Content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage? reply = await client.SendAsync(request);
            if (reply.IsSuccessStatusCode)
            {
                string data = await reply.Content.ReadAsStringAsync();
                return JsonNode.Parse(data).AsObject();
            }
            return null;
        }

        private async Task GetKeyAsync()
        {
            const string api = "https://accounts.lastpass.com/federatedlogin/api/v1/getkey";
            dynamic payload = new { company_id = _companyId, id_token = _idToken };
            HttpClient client = new HttpClient(_handler);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, api);
            string payloadText = System.Text.Json.JsonSerializer.Serialize(payload);
            HttpResponseMessage? reply = await client.PostAsync(api, new StringContent(payloadText, System.Text.Encoding.UTF8, "application/json"));
            if (reply.IsSuccessStatusCode)
            {
                string data = await reply.Content.ReadAsStringAsync();
                JsonObject result = JsonNode.Parse(data).AsObject();
                _k2 = (string)result["k2"];
                _fragmentId = (string)result["fragment_id"];
                CalculatePassword();
            }
        }

        private void CalculatePassword()
        {
            if(!string.IsNullOrEmpty(_k1) && !string.IsNullOrEmpty(_k2))
            {
                byte[] k1View = Convert.FromBase64String(PadBase64(_k1));
                //For Azure, K1 is decoded from Base64. But for Okta it is not Base64, it's a password. System.Text.Encoding.UTF8.GetBytes(_k1);
                byte[] k2View = Convert.FromBase64String(PadBase64(_k2));
                byte[] kView = new byte[k1View.Length < k2View.Length ? k1View.Length : k2View.Length];
                for (int i = 0; i < kView.Length; i++)
                {
                    kView[i] = (byte)(k1View[i] ^ k2View[i]);
                }
                var sha = System.Security.Cryptography.SHA256.Create();
                string hashed = Convert.ToBase64String(sha.ComputeHash(kView));
                Password = hashed;
                Fragment = _fragmentId;
            }
        }

        private async Task ConsumeFragment(string fragment)
        {
            string[] pieces = fragment.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach(string piece in pieces)
            {
                string[] part = piece.Split('=');
                if(part.Length == 2)
                {
                    if(part[0] == "access_token")
                    {
                        try
                        {
                            string graphApi = "https://graph.microsoft.com/v1.0/me?$select=id,displayName,mail&$expand=extensions";
                            HttpClient client = new HttpClient(_handler);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", part[1]);
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphApi);
                            HttpResponseMessage? reply = await client.SendAsync(request);
                            if (reply.IsSuccessStatusCode)
                            {
                                string data = await reply.Content.ReadAsStringAsync();
                                JsonObject payloadData = JsonNode.Parse(data).AsObject();
                                if (payloadData["extensions"] != null && payloadData["extensions"][0]["LastPassK1"] != null)
                                {
                                    _k1 = (string)payloadData["extensions"][0]["LastPassK1"];
                                }
                                else
                                {
                                    Console.WriteLine("No LastPass K1 found!");
                                }
                            }
                            //In the event someone is using Okta instead of Azure, this code should work instead:
                            /* string payload = part[1].Split('.')[1];
                            byte[] basedData = Convert.FromBase64String(PadBase64(payload));
                            string text = System.Text.Encoding.UTF8.GetString(basedData);
                            JsonObject payloadData = JsonNode.Parse(text).AsObject();
                            if (payloadData["LastPassK1"] != null)
                            {
                                _k1 = (string)payloadData["LastPassK1"];
                            }
                            else
                            {
                                Console.WriteLine("K1 is missing!!!");
                            }*/
                        }
                        catch
                        {
                            Console.WriteLine("Failed to handle payload!");
                        }
                    }
                    else if(part[0] == "id_token")
                    {
                        _idToken = part[1];
                    }
                }
            }
        }

        private static string PadBase64(string input)
        {
            //Payload is not padded. We need to pad it to a multiple of 4 to decode properly.
            if (input.Length % 4 == 3)
            {
                return input + "=";
            }
            else if (input.Length % 4 == 2)
            {
                return input + "==";
            }
            //There is no case for Length % 4 == 1, that cannot be valid Base64
            return input;
        }

    }
}