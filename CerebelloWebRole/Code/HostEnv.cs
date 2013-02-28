namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Enumeration of known host environments.
    /// </summary>
    public enum HostEnv
    {
        /// <summary>
        /// Unknown host environment.
        /// </summary>
        Unknown,

        /// <summary>
        /// IIS full version (not express, not embedded).
        /// </summary>
        Iis,

        /// <summary>
        /// IIS Express.
        /// </summary>
        IisExpress,

        /// <summary>
        /// Visual Studio web development server.
        /// </summary>
        WebDevServer,

        /// <summary>
        /// This is the WorkerRole execution host.
        /// Windows Azure runs the website inside IIS.
        /// </summary>
        WindowsAzureIisHost,
    }
}