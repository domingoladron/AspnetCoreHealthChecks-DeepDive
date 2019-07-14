using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.SomeModelService.Contracts.Models.SomeModel;
using HealthChecks.SomeModelService.Repositories.SomeModel;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using SomeModelModel = HealthChecks.SomeModelService.Models.SomeModel.SomeModel;

namespace HealthChecks.SomeModelService.AppServices.SomeModel
{
    public class SomeModelApplicationService : ISomeModelApplicationService
    {
        private readonly ILogger<SomeModelApplicationService> _logger;

        private readonly ISomeModelRepository _somemodelRepository;


        public SomeModelApplicationService(
            ILogger<SomeModelApplicationService> logger,
            ISomeModelRepository somemodelRepository)
        {
            _logger = logger;
            _somemodelRepository = somemodelRepository;
        }

        public async Task<SomeModelContract> CreateAsync(SomeModelContract model)
        {
            var mappedData = CastToDomainModel(model);
            var returnedData = await _somemodelRepository.CreateAsync(mappedData);
            return CastToContract(returnedData);
        }

         public async Task<SomeModelContract> UpdateAsync(
            SomeModelContract model)
        {
            _logger.LogTrace(
                $"Updating a SomeModel with Id {model.Id} based on the command data sent: " +
                $"{JsonConvert.SerializeObject(model)}");

            var storedModel = _somemodelRepository.GetAsync(model.Id);
            if (storedModel == null)
            {
                _logger.LogWarning($"No such SomeModel model with Id {model.Id} found.  Doing nothing.");
                return model;
            }

            var result = await _somemodelRepository.UpdateAsync(CastToDomainModel(model));

            return CastToContract(result);
        }

        public async Task<SomeModelContract> GetAsync(long id)
        {
            _logger.LogDebug($"Retrieving SomeModel with Id of {id}");
            var somemodelFound = await _somemodelRepository.GetAsync(id);
            if(somemodelFound != null)
            {
                _logger.LogDebug($"No SomeModel with Id of {id} found.  Returning null");
            }

            return CastToContract(somemodelFound);
        }

        public async Task<IEnumerable<SomeModelContract>> ListAllAsync()
        {
            var results = await _somemodelRepository.ListAllAsync();
            return await Task.FromResult(CastAllToContracts(results));
        }

        public async Task DeleteAsync(long id)
        {
            _logger.LogDebug($"Deleting SomeModel with Id of {id}");
            await _somemodelRepository.DeleteAsync(id);
        }

        public static IEnumerable<SomeModelContract> CastAllToContracts(
            IEnumerable<SomeModelModel> models)
        {
            return models.Select(CastToContract).ToList();
        }

        public static SomeModelContract CastToContract(
            SomeModelModel model)
        {
            TinyMapper.Bind<SomeModelModel, SomeModelContract>();
            TinyMapper.Bind<SomeModelContract, SomeModelModel>();
            return TinyMapper.Map<SomeModelContract>(model);
        }

        public static SomeModelModel CastToDomainModel(
            SomeModelContract model)
        {
            TinyMapper.Bind<SomeModelModel, SomeModelContract>();
            TinyMapper.Bind<SomeModelContract, SomeModelModel>();
            return TinyMapper.Map<SomeModelModel>(model);
        }
    }
}