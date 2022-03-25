using Logic.Exceptions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Application.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);
            HandleException(actionExecutedContext);
        }

        public override async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            await base.OnExceptionAsync(actionExecutedContext, cancellationToken);
            HandleException(actionExecutedContext);
        }

        private void HandleException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is ValidationException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    Status422UnprocessableEntity, actionExecutedContext.Exception.Message);
            }
            else if (actionExecutedContext.Exception is AuthenticationException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized, actionExecutedContext.Exception.Message);
            }
            else if (actionExecutedContext.Exception is NotFoundException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.NotFound, actionExecutedContext.Exception.Message);
            }
            else if (actionExecutedContext.Exception is ForbiddenException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.Forbidden, actionExecutedContext.Exception.Message);
            }
        }

        /// <summary>
        /// The 422 (Unprocessable Entity) status code means the server understands the content type of the request entity (hence a
        /// 415 (Unsupported Media Type) status code is inappropriate), and the syntax of the request entity is correct (thus a 400
        /// (Bad Request) status code is inappropriate) but was unable to process the contained instructions. For example, this error
        /// condition may occur if an XML request body contains well-formed (i.e., syntactically correct), but semantically erroneous,
        /// XML instructions.
        /// 
        /// See https://tools.ietf.org/html/rfc4918 for more info.
        /// </summary>
        private const HttpStatusCode Status422UnprocessableEntity = (HttpStatusCode)422;
    }
}