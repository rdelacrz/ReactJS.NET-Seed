using GraphQL.Instrumentation;
using GraphQL.Types;
using System;

namespace Logic.GraphQLSchema
{
    public class GraphQLSchema : Schema
    {
        public GraphQLSchema(Query query)
        {
            Query = query;
            //Mutation = (StarWarsMutation)provider.GetService(typeof(StarWarsMutation)) ?? throw new InvalidOperationException();

            FieldMiddleware.Use(new InstrumentFieldsMiddleware());
        }
    }
}
