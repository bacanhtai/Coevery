﻿using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Coevery.Environment;
using Coevery.Environment.Configuration;

namespace Coevery.Specs.Hosting.Coevery.Web {
    public class MvcApplication : HttpApplication {
        private static ICoeveryHost _host;
        private static IContainer _container;

        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start() {
            RegisterRoutes(RouteTable.Routes);
            _container = CoeveryStarter.CreateHostContainer(MvcSingletons);
            _host = _container.Resolve<ICoeveryHost>();

            _host.Initialize();

            // initialize shells to speed up the first dynamic query
            _host.BeginRequest();
            _host.EndRequest();
        }

        protected void Application_BeginRequest() {
            Context.Items["originalHttpContext"] = Context;
            _host.BeginRequest();
        }

        protected void Application_EndRequest() {
            _host.EndRequest();
        }

        static void MvcSingletons(ContainerBuilder builder) {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }

        public static void ReloadExtensions() {
            _host.ReloadExtensions();
        }

        public static IWorkContextScope CreateStandaloneEnvironment(string name) {
            var settings = _container.Resolve<IShellSettingsManager>().LoadSettings().SingleOrDefault(x => x.Name == name);
            if (settings == null) {
                settings = new ShellSettings {
                    Name = name,
                    State = TenantState.Uninitialized
                };
            }
    
            return _host.CreateStandaloneEnvironment(settings);
        }
    }
}