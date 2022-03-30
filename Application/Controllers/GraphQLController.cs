using GraphQL.Instrumentation;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using GraphQL.Validation.Complexity;
using Logic.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace GraphQL.GraphiQL.Controllers
{
    public class GraphQLController : ApiController
    {
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;
        private readonly IServiceProvider _provider;

        public GraphQLController(IDocumentExecuter executer, IDocumentWriter writer, ISchema schema, IServiceProvider provider)
        {
            _executer = executer;
            _writer = writer;
            _schema = schema;
            _provider = provider;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync(GraphQLQuery query)
        {
            var inputs = query.Variables.ToInputs();
            var queryToExecute = query.Query;

            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = queryToExecute;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
                _.RequestServices = _provider;

                _.ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 };

            }).ConfigureAwait(false);

            var httpResult = result.Errors?.Count > 0
                ? HttpStatusCode.BadRequest
                : HttpStatusCode.OK;

            var json = await _writer.WriteToStringAsync(result);

            var response = Request.CreateResponse(httpResult);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }
    }

    public class GraphQLQuery
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

        public JObject Variables { get; set; }
    }
}