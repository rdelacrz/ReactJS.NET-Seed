using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using Logic.GraphQLSchema;
using Logic.GraphQLSchema.Queries;
using Ninject;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Application
{
    public static class GraphQLConfig
    {
        public static void RegisterGraphQLInstances(IKernel kernel)
        {
            kernel.Bind<IDocumentExecuter>().To<DocumentExecuter>();
            kernel.Bind<IDocumentWriter>().To<DocumentWriter>();
            kernel.Bind<ISchema>().To<GraphQLSchema>();

            RegisterGraphQLSchema(kernel);
        }

        private static void RegisterGraphQLSchema(IKernel kernel)
        {
            kernel.Bind<LookupType>().ToSelf();
            kernel.Bind<OptionsGroupGraphType>().ToSelf();
            kernel.Bind<Query>().ToSelf();
        }
    }
}