using System.Collections.Generic;

namespace Application.Helpers
{
    public class ReactAppAssetManifest
    {
        public Dictionary<string, string> Files { get; set; }

        public List<string> Entrypoints { get; set; }
    }
}