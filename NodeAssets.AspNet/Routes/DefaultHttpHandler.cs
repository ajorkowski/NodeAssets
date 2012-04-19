using System.IO;
using System.Net;
using System.Web;

namespace NodeAssets.AspNet.Routes
{
    public sealed class DefaultHttpHandler : IHttpHandler
    {
        private readonly FileInfo _file;
        private readonly IAssetsConfiguration _config;

        public DefaultHttpHandler(FileInfo info, IAssetsConfiguration config)
        {
            _file = info;
            _config = config;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var response = context.Response;
            if (File.Exists(_file.FullName))
            {
                response.ClearHeaders();
                response.ClearContent();
               
                // Set the correct type
                switch (_file.Extension)
                {
                    case ".js":
                        response.AddHeader("Content-type", "application/javascript");
                        break;
                    case ".css":
                        response.AddHeader("Content-type", "text/css");
                        break;
                    default:
                        response.AddHeader("Content-type", "text/plain");
                        break;
                }

                // If caching we set the cache for a year (the recommended 'forever' cache amount)
                if(_config.UseCache)
                {
                    response.AddHeader("Cache-Control", "max-age=31556926");
                }

                // If we are compressing make sure to add the zip header
                // Only compress if they allow gzip encoding
                if (_config.UseCompress)
                {
                    EncodingHelper.AddCompressionFilter(context.Request, response);
                }

                context.Response.Write(File.ReadAllText(_file.FullName));
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
    }
}
