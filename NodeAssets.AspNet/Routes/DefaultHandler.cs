using CSharpVitamins;
using Microsoft.Owin;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace NodeAssets.AspNet.Routes
{
    public sealed class DefaultHandler
    {
        private readonly FileInfo _file;
        private readonly IAssetsConfiguration _config;

        public DefaultHandler(FileInfo info, IAssetsConfiguration config)
        {
            _file = info;
            _config = config;
        }

        public async Task Execute(IOwinContext context)
        {
            var response = context.Response;
            if (File.Exists(_file.FullName))
            {
                // Set the correct type
                switch (_file.Extension)
                {
                    case ".js":
                        response.ContentType = "application/javascript";
                        break;
                    case ".css":
                        response.ContentType = "text/css";
                        break;
                    default:
                        response.ContentType = "text/plain";
                        break;
                }

                // If caching we set the cache for a year (the recommended 'forever' cache amount)
                if (_config.UseCache)
                {
                    response.Headers.Add("Cache-Control", new [] { "max-age=31556926" });
                }

                // If we are compressing make sure to add the zip header
                // Only compress if they allow gzip encoding
                var bytes = File.ReadAllBytes(_file.FullName);
                if (_config.UseCompress)
                {
                    await AddCompressionFilter(context.Request, response, bytes);
                }
                else
                {
                    await context.Response.WriteAsync(bytes);
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        private static async Task AddCompressionFilter(IOwinRequest request, IOwinResponse response, byte[] data)
        {
            // load encodings from header
            QValueList encodings = new QValueList(request.Headers["Accept-Encoding"]);

            // get the types we can handle, can be accepted and
            // in the defined client preference
            QValue preferred = encodings.FindPreferred("gzip", "deflate", "identity");

            // if none of the preferred values were found, but the
            // client can accept wildcard encodings, we'll default
            // to Gzip.
            if (preferred.IsEmpty && encodings.AcceptWildcard && encodings.Find("gzip").IsEmpty)
            {
                preferred = new QValue("gzip");
            }

            // handle the preferred encoding
            switch (preferred.Name)
            {
                case "gzip":
                    response.Headers.Add("Content-Encoding", new [] { "gzip" });
                    using (var zipStream = new GZipStream(response.Body, CompressionMode.Compress, true))
                    {
                        await zipStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        await zipStream.FlushAsync();
                    }
                    break;
                case "deflate":
                    response.Headers.Add("Content-Encoding", new[] { "deflate" });
                    using (var defStream = new DeflateStream(response.Body, CompressionMode.Compress, true))
                    {
                        await defStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
                        await defStream.FlushAsync();
                    }
                    break;
                case "identity":
                default:
                    await response.WriteAsync(data);
                    break;
            }
        }
    }
}
