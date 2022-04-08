using Application.Helpers;
using Logic.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly QueryClientRenderer _queryClientRenderer;

        public HomeController(QueryClientRenderer queryClientRenderer)
        {
            _queryClientRenderer = queryClientRenderer;
        }

        public ActionResult Index()
        {
            // Retreive any prefetch data before this line of code and set the renderer's PrerenderedData state
            ViewBag.queryClientRenderer = _queryClientRenderer;

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