﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.DesignTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApplicationWithConfigureStartup
{
    public class Startup : IMvcBuilderConfiguration
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            var builder = services.AddMvc();
            ConfigureMvc(builder);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public void ConfigureMvc(IMvcBuilder builder)
        {
            builder.AddRazorOptions(options =>
            {
                options.ParseOptions = options.ParseOptions.WithPreprocessorSymbols(new[] { "TEST123" });
                var callback = options.CompilationCallback;
                options.CompilationCallback = context =>
                {
                    callback(context);
                    foreach (var tree in context.Compilation.SyntaxTrees)
                    {
                        var rewrittenRoot = new RazorRewriter().Visit(tree.GetRoot());
                        var rewrittenTree = tree.WithRootAndOptions(rewrittenRoot, tree.Options);
                        context.Compilation = context.Compilation.ReplaceSyntaxTree(tree, rewrittenTree);
                    }
                };
            });
        }
    }
}
