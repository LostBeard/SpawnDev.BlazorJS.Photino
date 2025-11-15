using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace SpawnDev.BlazorJS.Photino
{
    /// <summary>
    /// Simple http server
    /// </summary>
    public class WebRootServer : IWebRootServer
    {
        /// <summary>
        /// True if running
        /// </summary>
        public bool Running { get; private set; }
        Task? RunningTask = null;
        /// <summary>
        /// The currently served url
        /// </summary>
        public string? Url { get; private set; }
        /// <summary>
        /// The served folder
        /// </summary>
        public string? WwwRootFolder { get; private set; }
        /// <summary>
        /// The WebApplication that is being used to host the files
        /// </summary>
        public WebApplication? WebApplication { get; private set; }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="baseUrl"></param>
        public void CreateStaticFileServer(out string baseUrl)
        {
            CreateStaticFileServer(8000, 100, "wwwroot", out baseUrl);
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="webRootFolder"></param>
        /// <param name="baseUrl"></param>
        public void CreateStaticFileServer(string webRootFolder, out string baseUrl)
        {
            CreateStaticFileServer(8000, 100, webRootFolder, out baseUrl);
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="portRange"></param>
        /// <param name="webRootFolder"></param>
        /// <param name="baseUrl"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="SystemException"></exception>
        public void CreateStaticFileServer(int startPort, int portRange, string webRootFolder, out string baseUrl)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                baseUrl = Url;
                return;
            }
            var webRootPath = Path.GetFullPath(webRootFolder);
            if (Debugger.IsAttached && webRootFolder == "wwwroot")
            {
                string? projectDir = null;
                var parts = webRootPath.Split(new[] { '/', '\\' }).ToList();
                var pos = parts.LastIndexOf("bin");
                if (pos > -1)
                {
                    parts = parts.Take(pos).ToList();
                    projectDir = string.Join("/", parts);
                    var projectWwwRoot = Path.GetFullPath(Path.Combine(projectDir, webRootFolder));
                    if (Directory.Exists(projectWwwRoot))
                    {
                        webRootPath = projectWwwRoot;
                    }
                }
            }
            if (!Directory.Exists(webRootPath))
            {
                throw new DirectoryNotFoundException(nameof(webRootPath));
            }
            WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                WebRootPath = webRootPath
            });
            IFileProvider webRootFileProvider = webApplicationBuilder.Environment.WebRootFileProvider;
            webApplicationBuilder.Environment.WebRootFileProvider = webRootFileProvider;
            int port;
            for (port = startPort; IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any((IPEndPoint x) => x.Port == port); port++)
            {
                if (port > port + portRange)
                {
                    throw new SystemException($"Couldn't find open port within range {port - portRange} - {port}.");
                }
            }

            baseUrl = $"http://localhost:{port}";
            webApplicationBuilder.WebHost.UseUrls(baseUrl);
            WebApplication webApplication = webApplicationBuilder.Build();
            webApplication.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = true,
                FileProvider = webRootFileProvider,
                StaticFileOptions =
                {
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "application/octet-stream"
                }
            });
            RunningTask = webApplication.RunAsync();
            WebApplication = webApplication;
            WwwRootFolder = webRootFolder;
            Url = baseUrl;
            Running = true;
        }
    }
}
