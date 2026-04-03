using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Repositories
{
    public interface ISponsorRepository : IGenericRepository<Sponsor>
    {
        Task<Sponsor?>ExistsByNameAsync(string name);
        Task<Sponsor?> ExistsByContactEmailAsync(string email);
        Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category);
    }
}
