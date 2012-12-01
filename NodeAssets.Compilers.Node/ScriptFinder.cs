using System.Collections.Generic;
using System.IO;

namespace NodeAssets.Compilers
{
    internal static class ScriptFinder
    {
        public static IEnumerable<string> GetAllScripts()
        {
            return typeof (ScriptFinder).Assembly.GetManifestResourceNames();
        }

        public static string GetScript(string script)
        {
            using (var stream = typeof(ScriptFinder).Assembly.GetManifestResourceStream(script))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
