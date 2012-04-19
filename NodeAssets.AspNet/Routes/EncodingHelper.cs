using System.IO.Compression;
using System.Web;
using CSharpVitamins;

namespace NodeAssets.AspNet.Routes
{
    public static class EncodingHelper
    {
        public static void AddCompressionFilter(HttpRequest request, HttpResponse response)
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
                preferred = new QValue("gzip");

            // handle the preferred encoding
            switch (preferred.Name)
            {
                case "gzip":
                    response.AppendHeader("Content-Encoding", "gzip");
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                    break;
                case "deflate":
                    response.AppendHeader("Content-Encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                    break;
                case "identity":
                default:
                    break;
            }
        }
    }
}
