using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Enums;

namespace SportsLeague.DataAccess.Repositories
{
    public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
    {
        public SponsorRepository(LeagueDbContext context) : base(context)
        {

        }

        public async Task<Sponsor?> ExistsByContactEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.ContactEmail == email);
        }

        public async Task<Sponsor?> ExistsByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<IEnumerable<Sponsor>> GetByCategoryAsync(SponsorCategory category)
        {
            return await _dbSet
                .Where(s => s.Category == category)
                .ToListAsync();
        }
    }
}
