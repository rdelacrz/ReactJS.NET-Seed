using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Jint;
using JavaScriptEngineSwitcher.Jurassic;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.V8;

namespace Application
{
    /// <summary>
    /// Contains the configuration for the JS Engine Switcher, which is responsible for setting up the unified interface 
    /// for access to the basic features of popular JavaScript engines (MSIE JavaScript Engine for .Net, Microsoft 
    /// ClearScript.V8, Jurassic, Jint, ChakraCore and VroomJs). It allows the application to quickly and easily switch 
    /// to another JavaScript engine.
    /// </summary>
    public class JsEngineSwitcherConfig
    {
        /// <summary>
        /// Configure the various Javascript engines that can access different features of Javascript.
        /// </summary>
        /// <param name="engineSwitcher">The Javascript engine switcher being configured.</param>
        public static void Configure(IJsEngineSwitcher engineSwitcher)
        {
            engineSwitcher.EngineFactories
                .AddChakraCore()
                .AddJint()
                .AddJurassic()
                .AddMsie(new MsieSettings
                {
                    UseEcmaScript5Polyfill = true,
                    UseJson2Library = true
                })
                .AddV8();

            engineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
        }
    }
}
