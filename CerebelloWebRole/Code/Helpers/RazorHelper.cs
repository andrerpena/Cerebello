using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Routing;
using System.Web.WebPages;
using CerebelloWebRole.Controllers;
using Microsoft.CSharp;

namespace CerebelloWebRole.Code.Helpers
{
    /// <summary>
    /// Helps rendering razor web pages, outside of the web environment,
    /// such as test environment, and also Azure WorkerRole environment.
    /// </summary>
    public static class RazorHelper
    {
        private static readonly ConcurrentDictionary<RequesterTypeAndResourcePath, Type> pageTypeByResourcePath =
            new ConcurrentDictionary<RequesterTypeAndResourcePath, Type>();

        /// <summary>
        /// Renders an embedded razor view.
        /// </summary>
        /// <param name="requesterType"> </param>
        /// <param name="viewName"></param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static string RenderEmbeddedRazor(Type requesterType, [JetBrains.Annotations.AspMvcView] string viewName, ViewDataDictionary viewData)
        {
            // TODO: support layout page
            // TODO: support _ViewStart pages

            // Creating or getting the type that renders the view.
            var resourcePath = string.Format(
                "{0}\\Views\\{1}\\{2}.cshtml",
                Path.GetFileNameWithoutExtension(requesterType.Assembly.GetName().Name),
                requesterType.Name,
                viewName);

            var key = new RequesterTypeAndResourcePath
                {
                    RequesterType = requesterType,
                    ResourcePath = resourcePath,
                };

            var type = pageTypeByResourcePath.GetOrAdd(key, CreateTypeForEmbeddedRazor);
            if (type == null)
                throw new Exception("View not found.");

            // Rendering the page, using the passed model.
            var page = (WebViewPage)Activator.CreateInstance(type);
            using (var stringWriter = new StringWriter())
            {
                // We need to emulate the environment of a web-request to render the view.
                var httpContext = new MvcHelper.MockHttpContext { Request2 = new MvcHelper.MockHttpRequest() };
                var requestContext = new RequestContext(httpContext, new RouteData());

                page.ViewContext = new ViewContext(
                    new ControllerContext(requestContext, new HomeController()),
                    new RazorView(new ControllerContext(), resourcePath, null, true, new[] { "cshtml", "vbhtml" }),
                    viewData,
                    new TempDataDictionary(),
                    stringWriter);

                page.ViewData = viewData;
                page.Url = new UrlHelper(requestContext, RouteTable.Routes);

                page.PushContext(new WebPageContext(httpContext, null, viewData), stringWriter);
                try
                {
                    // Executing the renderer, so that it writes to the stringWriter.
                    page.Execute();
                }
                finally
                {
                    page.PopContext();
                }

                // The rendered page is inside the stringWriter.
                var result = stringWriter.GetStringBuilder().ToString();
                return result;
            }
        }

        class RequesterTypeAndResourcePath
        {
            public Type RequesterType { get; set; }
            public string ResourcePath { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as RequesterTypeAndResourcePath;
                if (other == null) return false;
                if (this.RequesterType != other.RequesterType) return false;
                if (this.ResourcePath != other.ResourcePath) return false;
                return true;
            }

            public override int GetHashCode()
            {
                return (this.RequesterType ?? typeof(object)).GetHashCode() + (this.ResourcePath ?? "").GetHashCode();
            }
        }

        private static Type CreateTypeForEmbeddedRazor(RequesterTypeAndResourcePath data)
        {
            // Getting razor code from resource.
            string code = null;
            string resourceName = data.ResourcePath.Replace('\\', '.');
            using (var stream = data.RequesterType.Assembly.GetManifestResourceStream(resourceName))
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                        code = reader.ReadToEnd();

            if (code == null)
                throw new Exception("Resource was not found. Maybe it has not been embedded.");

            var virtualPath = string.Format("~/{0}", data.ResourcePath.Replace('\\', '/'));
            var className = resourceName.Replace('.', '_');

            // Creating compile unit, using Mvc Razor syntax.
            var factory = new MvcWebRazorHostFactory();
            var host = factory.CreateHost(virtualPath, data.ResourcePath);
            var engine = new RazorTemplateEngine(host);

            GeneratorResults results;
            using (TextReader reader = new StringReader(code))
                results = engine.GenerateCode(reader, className, rootNamespace: null, sourceFileName: host.PhysicalPath);

            if (!results.Success)
                throw CreateExceptionFromParserError(results.ParserErrors.Last(), virtualPath);

            // Compiling an assembly with the code.
            var codeProvider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters
                {
                    GenerateInMemory = true,
                    GenerateExecutable = false,
#if DEBUG
                    IncludeDebugInformation = true,
#endif
                };

            compilerParams.ReferencedAssemblies.AddRange(
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .GroupBy(a => a.FullName)
                    .Select(grp => grp.First())
                    .Select(a => a.Location)
                    .ToArray());

            var compilerResult = codeProvider.CompileAssemblyFromDom(compilerParams, results.GeneratedCode);

            // Returning the compiled type.
            var type = compilerResult.CompiledAssembly.GetTypes().Single(t => t.Name == className);
            return type;
        }

        private static HttpParseException CreateExceptionFromParserError(RazorError error, string virtualPath)
        {
            return new HttpParseException(error.Message + Environment.NewLine, null, virtualPath, null, error.Location.LineIndex + 1);
        }
    }
}