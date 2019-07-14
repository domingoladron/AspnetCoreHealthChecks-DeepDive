using System.ComponentModel.DataAnnotations;

namespace HealthChecks.SomeModelService.Contracts.Models.SomeModel
{
    public class SomeModelContract
    {
        [Required]
        public long Id { get; set; }
    }
}
