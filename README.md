# Lastpass-SSO-DotNET
An implementation in .NET 6 for fetching the fragment necessary to do SSO login to LastPass with an Azure AD account

The code spawns a WebView2 (Edge) browser window to handle 2FA if necessary, then the window automatically closes when done and gives you both the password and SSO fragment for use with a lastpass client such as LastPass-Sharp (https://github.com/detunized/lastpass-sharp) or jnewbigin's fork of the lastpass CLI with SSO support (https://github.com/jnewbigin/lastpass-cli/tree/sso)

No username input is necessary because your current username running the application is used.
