using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Minimal.Domain.Core.Responces;

namespace Minmal.API.ActionFilters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Method == HttpMethod.Post.Method || context.HttpContext.Request.Method == HttpMethod.Put.Method)
            {
                if (!context.ModelState.IsValid)
                {
                    //a way to do data annotation localization on server side
                    //var Localizer = (IStringLocalizer<CommonResources>)context.HttpContext.RequestServices.GetService(typeof(IStringLocalizer<CommonResources>));

                    //context.HttpContext.Response.Headers.Add("ResponseType", "ValidationErrors");

                    var response = context.ModelState
                                        .Where(modelError => modelError.Value.Errors.Count > 0)
                                        .Select(modelError => new ValidationResult
                                        {
                                            Field = modelError.Key,
                                            Errors = modelError.Value.Errors.Select(error => error.ErrorMessage).ToList()
                                        }).ToList();
                    var result = new Response<string>
                    {
                        Result = null,
                        HasErrors = true,
                        ValidationErrors = response
                    };
                    context.Result = new OkObjectResult(result);

                    //context.Result = new BadRequestObjectResult(response);
                }

            }

        }
    }
}
