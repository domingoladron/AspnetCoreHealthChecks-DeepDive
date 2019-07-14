using System.Collections.Generic;
using System.Threading.Tasks;
using SomeModelModel = HealthChecks.SomeModelService.Models.SomeModel.SomeModel;

namespace HealthChecks.SomeModelService.Repositories.SomeModel
{
    public interface ISomeModelRepository
    {
        Task<SomeModelModel> CreateAsync(SomeModelModel model);
        
        Task<SomeModelModel> UpdateAsync(
            SomeModelModel model);
        
        Task<SomeModelModel> GetAsync(long id);
        
        Task<IEnumerable<SomeModelModel>> ListAllAsync();
        
        Task DeleteAsync(long id);
        
    }
}