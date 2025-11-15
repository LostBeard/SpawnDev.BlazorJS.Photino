namespace SpawnDev.BlazorJS.Photino
{
    /// <summary>
    /// Simple http server
    /// </summary>
    public interface IWebRootServer
    {
        /// <summary>
        /// True if running
        /// </summary>
        bool Running { get; }
        /// <summary>
        /// The currently served url
        /// </summary>
        string? Url { get; }
        /// <summary>
        /// The served folder
        /// </summary>
        string? WwwRootFolder { get; }
        /// <summary>
        /// Starts the server
        /// </summary>
        void CreateStaticFileServer(int startPort, int portRange, string webRootFolder, out string baseUrl);
        /// <summary>
        /// Starts the server
        /// </summary>
        void CreateStaticFileServer(out string baseUrl);
        /// <summary>
        /// Starts the server
        /// </summary>
        void CreateStaticFileServer(string webRootFolder, out string baseUrl);
    }
}
