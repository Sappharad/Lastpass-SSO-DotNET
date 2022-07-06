namespace LastPassSSOHelper
{
    public static class LastPassSSOHelper
    {
        private static string _profileDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LastPassDotNetSSO");
        /// <summary>
        /// Get or set the profile directory used by the WebView2 for cookie storage and caching.
        /// Defaults to %LocalAppData%/LastPassDotNetSSO/
        /// Null is a valid value for this, if you switch this to null it will create the profile folder as a subfolder relative to your app.
        /// That is not ideal for Published Apps where the app folders are read only, hence the AppData default
        /// </summary>
        public static string? ProfileDirectory
        {
            get { return _profileDirectory;}
            set { _profileDirectory = value; }
        }
        /// <summary>
        /// Path for WebView2 browser executable files, if you aren't installing the runtime on the machine.
        /// Optional. If not set, the runtime installed to the machine is used.
        /// </summary>
        public static string? BrowserFiles { get; set; }

        public static (string username, string password, string fragment)? GetLastPassCredentials()
        {
            using (var ssov = new SingleSignOnView())
            {
                ssov.ShowDialog();
                if (!string.IsNullOrEmpty(ssov.Email) && !string.IsNullOrEmpty(ssov.Password) && !string.IsNullOrEmpty(ssov.Fragment))
                {
                    return (ssov.Email, ssov.Password, ssov.Fragment);
                }
                return null;
            }
        }
    }
}