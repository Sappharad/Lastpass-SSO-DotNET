namespace lastpass_sso_net
{
    internal static class Program
    {
        /// <summary>
        /// This is just a sample app that calls the LastPassSSOHelper class for Azure AD login.
        /// It outputs the password and fragment to the console like the Okta SSO sample application, 
        /// so it could theoretically be used with the lastpass CLI by exporting the exe path 
        /// to LPASS_PINENTRY but that didn't actually seem to work in my testing.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            (string username, string password, string fragment)? result = LastPassSSOHelper.LastPassSSOHelper.GetLastPassCredentials();
            if(result != null)
            {
                Console.WriteLine($"PASSWORD:{result.Value.password}");
                Console.WriteLine($"FRAGMENT:{result.Value.fragment}");
            }
        }
    }
}