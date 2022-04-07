using Newtonsoft.Json;
using React;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Application.Helpers
{
    public class QueryClientRenderer : RenderFunctionsBase
    {
        internal class ReactAppAssetManifest
        {
            public Dictionary<string, string> Files { get; set; }
            public List<string> Entrypoints { get; set; }
        }

        public string DehydratedState { get; private set; } = "";

        public override void PreRender(Func<string, string> executeJs)
        {
            executeJs("var queryClient = new QueryClient(queryClientOptions);");
            // executeJs("await queryClient.prefetchQuery('key', fn);");
            executeJs("var dehydratedState = dehydrate(queryClient);");

            DehydratedState = executeJs("JSON.stringify(dehydratedState)");
        }

        public override string WrapComponent(string componentToRender)
        {
            string cleanedBase = componentToRender.Substring(0, componentToRender.Length - 1);
            string wrappedComponent = $"React.createElement(QueryClientProvider, {{ client: queryClient }}, React.createElement(Hydrate, {{ state: dehydratedState }}, {cleanedBase})))";
            return wrappedComponent;
        }

        public override void PostRender(Func<string, string> executeJs)
        {
            executeJs("queryClient.clear();");
        }
    }

}