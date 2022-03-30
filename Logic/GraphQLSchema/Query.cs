using GraphQL.Types;
using Logic.GraphQLSchema.Queries;

namespace Logic.GraphQLSchema
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            Name = "Query";
            Field<OptionsGroupGraphType>(
              "options",
              resolve: context => new { }
            );
        }
    }
}
