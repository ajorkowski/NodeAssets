using CSharpVitamins;
using Microsoft.Owin;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NodeAssets.AspNet.Routes
{
    public sealed class DefaultHandler
    {
        private readonly AssetsConfiguration _config;
        private readonly Lazy<Task<AssetData>> _asset;
        private readonly FileInfo _file;

        public DefaultHandler(FileInfo info, AssetsConfiguration config)
        {
            _file = info;
            _asset = new Lazy<Task<AssetData>>(() => CreateAssetData(info), true);
            _config = config;
        }

        public async Task Execute(IOwinContext context)
        {
            var response = context.Response;

            // If we have live css enabled, load the file every time
            var asset = await (_config.IsLiveCss ? CreateAssetData(_file) : _asset.Value).ConfigureAwait(false);
            if (asset != null)
            {
                response.ContentType = asset.ContentType;

                // If caching we set the cache for a year (the recommended 'forever' cache amount)
                if (_config.UseCache)
                {
                    response.Headers.Add("Cache-Control", new [] { "max-age=31556926" });
                }

                // We can safely use ETags/LastModified
                context.Response.Headers.Add("ETag", new[] { asset.Hash });
                context.Response.Headers.Add("Last-Modified", new[] { asset.LastModified.ToString("R") });

                DateTime modifiedSince;
                bool has304Header = context.Request.Headers.ContainsKey("If-None-Match") || context.Request.Headers.ContainsKey("If-Modified-Since");
                if (has304Header
                    && (!context.Request.Headers.ContainsKey("If-None-Match") || context.Request.Headers["If-None-Match"] == asset.Hash)
                    && (!context.Request.Headers.ContainsKey("If-Modified-Since") || (DateTime.TryParse(context.Request.Headers["If-Modified-Since"], out modifiedSince) && modifiedSince.ToUniversalTime() >= asset.LastModified)))
                {
                    // Not modified response
                    context.Response.StatusCode = 304;
                }
                else
                {
                    // If we are compressing make sure to add the zip header
                    // Only compress if they allow gzip encoding
                    if (_config.UseCompress)
                    {
                        await AddCompressionFilter(context.Request, response, asset).ConfigureAwait(false);
                    }
                    else
                    {
                        response.ContentLength = asset.Data.Length;
                        await context.Response.WriteAsync(asset.Data).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        private static async Task AddCompressionFilter(IOwinRequest request, IOwinResponse response, AssetData data)
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
                case "deflate":
                    response.ContentLength = data.DeflateData.Length;
                    response.Headers.Add("Content-Encoding", new[] { "deflate" });
                    await response.WriteAsync(data.DeflateData).ConfigureAwait(false);
                    break;
                case "gzip":
                    response.ContentLength = data.ZipData.Length;
                    response.Headers.Add("Content-Encoding", new[] { "gzip" });
                    await response.WriteAsync(data.ZipData).ConfigureAwait(false);
                    break;
                default:
                    response.ContentLength = data.Data.Length;
                    await response.WriteAsync(data.Data).ConfigureAwait(false);
                    break;
            }
        }

        private async Task<AssetData> CreateAssetData(FileInfo file)
        {
            if (!file.Exists) { return null; }

            byte[] data;
            byte[] defData;
            byte[] zipData;
            using (var ms = new MemoryStream())
            {
                using (var fs = file.OpenRead())
                {
                    await fs.CopyToAsync(ms).ConfigureAwait(false);
                }

                data = ms.ToArray();
                ms.Position = 0;

                using (var saveMs = new MemoryStream())
                {
                    using (var defStream = new DeflateStream(saveMs, CompressionMode.Compress, true))
                    {
                        await ms.CopyToAsync(defStream).ConfigureAwait(false);
                    }
                    defData = saveMs.ToArray();
                    saveMs.SetLength(0);
                    ms.Position = 0;

                    using (var zipStream = new GZipStream(saveMs, CompressionMode.Compress, true))
                    {
                        await ms.CopyToAsync(zipStream).ConfigureAwait(false);
                    }
                    zipData = saveMs.ToArray();
                }
            }

            string hash;
            using (var hasher = MD5.Create())
            {
                hash = Convert.ToBase64String(hasher.ComputeHash(data));
            }

            // Set the correct type
            string extension = file.Extension;
            string contentType;
            switch (file.Extension)
            {
                case ".js":
                    contentType = "application/javascript";
                    break;
                case ".css":
                    contentType = "text/css";
                    break;
                default:
                    contentType = "text/plain";
                    break;
            }

            return new AssetData
            {
                Extension = extension,
                ContentType = contentType,
                Hash = hash,
                Data = data,
                ZipData = zipData,
                DeflateData = defData,
                LastModified = DateTime.Parse(file.LastWriteTimeUtc.ToString("R")).ToUniversalTime()
            };
        }

        private class AssetData
        {
            public string Extension { get; set; }
            public string ContentType { get; set; }
            public string Hash { get; set; }
            public byte[] Data { get; set; }
            public byte[] ZipData { get; set; }
            public byte[] DeflateData { get; set; }
            public DateTime LastModified { get; set; }
        }
    }
}
