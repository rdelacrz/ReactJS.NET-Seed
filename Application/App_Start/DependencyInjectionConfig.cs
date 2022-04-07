using Autofac;
using Autofac.Extensions.DependencyInjection;
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

namespace Application
{
    public static class DependencyInjectionConfig
    {
        public static void SetupDependencyInjection()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;

            // Registers Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Internally registers service provider and ties it to IServiceProvider
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());

            // Registers injectable services
            RegisterServices(builder);
            RegisterGraphQLInstances(builder);
            RegisterGraphQLSchema(builder);

            // Set the dependency resolver to be Autofac
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            // Binds internal services
            builder.RegisterType<DatabaseService>().As<IDatabaseService>();
            builder.RegisterType<ConfigService>().As<IConfigService>();
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