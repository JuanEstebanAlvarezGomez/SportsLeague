using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Services
{
    public interface ISponsorService
    {
        Task<IEnumerable<Sponsor>> GetAllAsync();
        Task<Sponsor?> GetByIdAsync(int id);
        Task<Sponsor?> GetByNameAsync(string name);
        Task<Sponsor?> GetByContactEmail(string email);
        Task<Sponsor?> CreateAsync(Sponsor sponsor);
        Task<Sponsor?> UpdateAsync(int id, Sponsor sponsor);
        Task DeleteAsync(int id);
        Task UpdateCategoryAsync(int id, SponsorCategory newCategory);
    }
}
