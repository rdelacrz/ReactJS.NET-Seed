using GraphQL;
using GraphQL.Types;
using Logic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.GraphQLSchema.Queries
{
    public class LookupType : ObjectGraphType
    {
        public LookupType()
        {
            Field<NonNullGraphType<IdGraphType>>("valueCode");
            Field<StringGraphType>("valueName");
            Field<IntGraphType>("orderSequence");
        }
    }

    public class OptionsGroupGraphType : ObjectGraphType
    {
        public OptionsGroupGraphType()
        {
            Name = "OptionsGroupQuery";
            FieldAsync<ListGraphType<LookupType>>(
              "lookups",
              arguments: new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "fieldName" },
                new QueryArgument<BooleanGraphType> { Name = "activeOnly" }
              ),
              resolve: async context =>
              {
                  var configService = context.RequestServices.GetRequiredService<IConfigService>();
                  var fieldName = context.GetArgument<string>("fieldName");
                  var activeOnly = context.GetArgument<bool?>("activeOnly");
                  return await configService.GetLookup(fieldName, activeOnly.GetValueOrDefault(true));
              }
            );
        }
    }
}
