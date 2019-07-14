using System.Collections.Generic;
using System.Threading.Tasks;
using HealthChecks.SomeModelService.Contracts.Models.SomeModel;

namespace HealthChecks.SomeModelService.AppServices.SomeModel
{
    public interface ISomeModelApplicationService
    {
        Task<SomeModelContract> CreateAsync(SomeModelContract model);
        
        Task<SomeModelContract> UpdateAsync(
            SomeModelContract model);
        
        Task<SomeModelContract> GetAsync(long id);
        
        Task<IEnumerable<SomeModelContract>> ListAllAsync();
        
        Task DeleteAsync(long id);
        
    }
}