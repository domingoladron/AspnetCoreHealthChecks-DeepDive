using System.Threading.Tasks;
using HealthChecks.Configuration.Logging;
using HealthChecks.SomeModelService.AppServices.SomeModel;
using HealthChecks.SomeModelService.Contracts.Models.SomeModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthChecks.SomeModelService.Controllers
{
    [Route("somemodels")]
    [ApiController]
    public class SomeModelController : ControllerBase
    {
        private readonly ISomeModelApplicationService _somemodelApplicationService;

        private readonly ILogger<SomeModelController> _logger;

        public SomeModelController(
            ISomeModelApplicationService somemodelApplicationService,
            ILogger<SomeModelController> logger)
        {
            _somemodelApplicationService = somemodelApplicationService;
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult> CreateAsync(SomeModelContract model)
        {
            _logger.LogTraceJson("Starting SomeModel create", model);
            if (!ModelState.IsValid)
            {
                _logger.LogErrorJson(
                    "SomeModel model is not valid",
                    model);

                return BadRequest(ModelState);
            }

            var newSomeModel = await _somemodelApplicationService.CreateAsync(model);
            _logger.LogTraceJson("Completing SomeModel create");
            return Ok(newSomeModel);
        }

        [HttpGet]
        public async Task<ActionResult> ListAllAsync()
        {
            _logger.LogTraceJson("ListAll SomeModels");
            var SomeModels = await _somemodelApplicationService.ListAllAsync();
            _logger.LogTraceJson("Completing ListAll SomeModels", SomeModels);
            return Ok(SomeModels);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetAsync([FromRoute]long id)
        {
            _logger.LogTraceJson($"Starting Get SomeModel: {id}");
            var SomeModel = await _somemodelApplicationService.GetAsync(id);

            if (SomeModel == null)
            {
                _logger.LogTraceJson($"SomeModel not found: {id}");
                return NotFound();
            }
            _logger.LogTraceJson($"Completing Get SomeModel: {id}");
            return Ok(SomeModel);
        }


        [HttpPut]
        public async Task<ActionResult> UpdateAsync(
            [FromBody] SomeModelContract model)
        {
            _logger.LogTraceJson("Starting SomeModel update", model);
            if (!ModelState.IsValid)
            {
                _logger.LogErrorJson(
                    "SomeModel model is not valid",
                    model);

                return BadRequest(ModelState);
            }

            var updatedSomeModel = await _somemodelApplicationService.UpdateAsync(model);
            _logger.LogTraceJson("Completing SomeModel update");
            return Ok(updatedSomeModel);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteAsync(long id)
        {
            _logger.LogTraceJson($"Starting deletion of SomeModel: {id}");
            await _somemodelApplicationService.DeleteAsync(id);
            _logger.LogTraceJson($"Completing deletion of SomeModel: {id}");
            return Ok();
        }
    }
}