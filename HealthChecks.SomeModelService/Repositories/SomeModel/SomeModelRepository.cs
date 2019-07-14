using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SomeModelModel = HealthChecks.SomeModelService.Models.SomeModel.SomeModel;

namespace HealthChecks.SomeModelService.Repositories.SomeModel
{
    public class SomeModelRepository : ISomeModelRepository
    {
        private static readonly List<SomeModelModel> SomeModels = new List<SomeModelModel>();

        private readonly ILogger<SomeModelRepository> _logger;

        public SomeModelRepository(
            ILogger<SomeModelRepository> logger)
        {
            _logger = logger;
        }

        public async Task<SomeModelModel> CreateAsync(SomeModelModel model)
        {

            var nextId = SomeModels.Count + 1;
            model.Id = nextId;
            _logger.LogTrace(
                $"Creating an SomeModel with Id {nextId} based on the command data sent: " +
                $"{JsonConvert.SerializeObject(model)}");
            SomeModels.Add(model);

            return model;
        }

         public async Task<SomeModelModel> UpdateAsync(
            SomeModelModel model)
        {
            _logger.LogTrace(
                $"Updating a SomeModel with Id {model.Id} based on the command data sent: " +
                $"{JsonConvert.SerializeObject(model)}");

            var storedModel = SomeModels.Find(g => g.Id.Equals(model.Id));
            if (storedModel == null)
            {
                _logger.LogWarning($"No such SomeModel model with Id {model.Id} found.  Doing nothing.");
                return model;
            }

            SomeModels.Remove(storedModel);

            SomeModels.Add(model);

            return model;
        }

        public async Task<SomeModelModel> GetAsync(long id)
        {
            _logger.LogDebug($"Retrieving SomeModel with Id of {id}");
            var model = SomeModels.FirstOrDefault(i => i.Id.Equals(id));

            if (model != null)
            {
                return model;
            }
            _logger.LogDebug($"No SomeModel with Id of {id} found.  Returning null");
            return null;
        }

        public async Task<IEnumerable<SomeModelModel>> ListAllAsync()
        {
            return SomeModels.AsEnumerable();
        }

        public async Task DeleteAsync(long id)
        {
            _logger.LogDebug($"Deleting SomeModel with Id of {id}");
            var modelFound = SomeModels.Find(g => g.Id.Equals(id));
            if (modelFound != null)
            {
                SomeModels.Remove(modelFound);
                _logger.LogDebug($"SomeModel with Id of {id} Deleted");
            }
            else
            {
                _logger.LogDebug($"No SomeModel with Id of {id} found to delete.  Returning");
            }
        }

    }
}