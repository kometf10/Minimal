using Microsoft.AspNetCore.Mvc;
using Minimal.Domain.Core;
using Minimal.Domain.Core.RequestFeatures;
using Minimal.Services.Core;

namespace Minmal.API.Controllers.Core
{
    public class BaseController<U> : ControllerBase where U : BaseDto, new()
    {
        protected readonly IServiceBase<U> ServiceBase;
        public BaseController(IServiceBase<U> serviceBase)
        {
            ServiceBase = serviceBase;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Get([FromQuery] RequestParameters pagingParameters)
        {
            var result = await ServiceBase.GetPaged(pagingParameters);

            return Ok(result);
        }

        [HttpGet("GetAll")]
        public virtual async Task<IActionResult> GetAll([FromQuery] RequestParameters pagingParameters = null!)
        {
            var result = await ServiceBase.GetAll(pagingParameters);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> Get(int id)
        {
            var result = await ServiceBase.Get(id);

            return Ok(result);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(U entityDto)
        {
            var result = await ServiceBase.Create(entityDto);

            return Ok(result);
        }

        [HttpPut]
        public virtual async Task<IActionResult> Update(U entityDto)
        {
            var result = await ServiceBase.Update(entityDto);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            var result = await ServiceBase.Delete(id);

            return Ok(result);
        }
    }
}
