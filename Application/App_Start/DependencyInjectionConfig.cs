using Application.Helpers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Logic.GraphQLSchema;
using Logic.GraphQLSchema.Queries;
using Logic.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Application
{
    public static class DependencyInjectionConfig
    {
        public static void SetupDependencyInjection()
        {
            var builder = new ContainerBuilder();

            // Registers Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Registers MVC controllers
            builder.RegisterControllers(typeof(MainApplication).Assembly);

            // Enables property injection in view pages
            builder.RegisterSource(new ViewRegistrationSource());

            // Internally registers service provider and ties it to IServiceProvider
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());

            // Registers injectable services
            RegisterServices(builder);
            RegisterReactRenderingFunctions(builder);
            RegisterGraphQLInstances(builder);
            RegisterGraphQLSchema(builder);

            // Set the dependency resolver to be Autofac
            var container = builder.Build();
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            // Binds internal services
            builder.RegisterType<DatabaseService>().As<IDatabaseService>();
            builder.RegisterType<ConfigService>().As<IConfigService>();
        }

        private static void RegisterReactRenderingFunctions(ContainerBuilder builder)
        {
            builder.RegisterType<QueryClientRenderer>().AsSelf().InstancePerRequest();
        }

        private static void RegisterGraphQLInstances(ContainerBuilder builder)
        {
            builder.RegisterType<DocumentExecuter>().As<IDocumentExecuter>();
            builder.RegisterType<DocumentWriter>().As<IDocumentWriter>();
            builder.RegisterType<GraphQLSchema>().As<ISchema>();
        }

        private static void RegisterGraphQLSchema(ContainerBuilder builder)
        {
            builder.RegisterType<LookupType>().AsSelf();
            builder.RegisterType<OptionsGroupGraphType>().AsSelf();
            builder.RegisterType<Query>().AsSelf();
        }
    }
}