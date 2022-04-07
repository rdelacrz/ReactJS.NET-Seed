using Application.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Application.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Retrieves scripts and styles from the asset manifest file in wwwroot
            string manifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "asset-manifest.json");
            string rawManifest = System.IO.File.ReadAllText(manifestPath);
            var manifest = JsonConvert.DeserializeObject<ReactAppAssetManifest>(rawManifest);

            ViewBag.scripts = string.Join("\n", manifest.Entrypoints
                        .Where(path => path.EndsWith(".js"))
                        .Select(scriptPath => $"<script src=\"{scriptPath}\"></script>"));

            ViewBag.styles = string.Join("\n", manifest.Entrypoints
                .Where(path => path.EndsWith(".css"))
                .Select(stylePath => $"<link rel=\"stylesheet\" href=\"{(stylePath)}\" />"));

            return View();
        }
    }
}