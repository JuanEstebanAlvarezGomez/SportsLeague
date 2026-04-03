using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ILogger _logger;
        private readonly ISponsorRepository _sponsorRepository;
        
        public SponsorService(
            ILogger<SponsorService> logger, 
            ISponsorRepository sponsorRepository)
        {
            _logger = logger;
            _sponsorRepository = sponsorRepository;
        }
        public async Task<Sponsor?> CreateAsync(Sponsor sponsor)
        {
            var existsName = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
            var existsEmail = await _sponsorRepository.ExistsByContactEmailAsync(sponsor.ContactEmail);

            if (existsName != null)
            {
                _logger.LogWarning("Sponsor with name {Name} already exists", sponsor.Name);
                throw new InvalidOperationException(
                    $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }

            if (existsEmail != null)
            {
                _logger.LogWarning("Sponsor with contact email {Email} already exists", sponsor.ContactEmail);
                throw new InvalidOperationException(
                    $"Ya existe un patrocinador con el correo electrónico '{sponsor.ContactEmail}'");
            }

            if (string.IsNullOrWhiteSpace(sponsor.ContactEmail))
            {
                _logger.LogWarning("Contact email is required for sponsor '{Name}'", sponsor.Name);
                throw new InvalidOperationException("El email de contacto es obligatorio.");
            }

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(sponsor.ContactEmail);
                if (mailAddress.Address != sponsor.ContactEmail)
                {
                    throw new Exception(); // fuerzado de catch
                }
            }
            catch
            {
                _logger.LogWarning("Invalid email format for sponsor '{Name}': {Email}",
                    sponsor.Name, sponsor.ContactEmail);
                throw new InvalidOperationException(
                    $"El email de contacto '{sponsor.ContactEmail}' no tiene un formato válido.");
            }

            _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.ExistsAsync(id);
            if (!exists)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found for deletion", id);
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con ID '{id}'");
            }

            _logger.LogInformation("Deleting Sponsor with ID: {Id}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByContactEmailAsync(string email)
        {
            
            var sponsor = await _sponsorRepository.ExistsByContactEmailAsync(email);

            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with contact email {Email} not found", email);
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con el correo electrónico '{email}'");
            }

            _logger.LogInformation("Retrieving sponsor with contact email: {Email}", email);
            return sponsor;

        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {Id}", id);
            var sponsor = await _sponsorRepository.GetByIdAsync(id);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {RefereeId} not found", id);
            }
            
            return sponsor;
        }

        public async Task<Sponsor?> GetByNameAsync(string name)
        {
            var sponsor = await _sponsorRepository.ExistsByNameAsync(name);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with name {Name} not found", name);
                throw new KeyNotFoundException(
                    $"No se encontró el patrocinador con el nombre '{name}'");
            }
            _logger.LogInformation("Retrieving sponsor with name: {Name}", name);
            return sponsor;
        }

        public async Task<Sponsor?> UpdateAsync(int id, Sponsor sponsor)
        {
            var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

            if (existingSponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found for update", id);
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID '{id}'");
            }

            if (existingSponsor.Name != sponsor.Name)
            {
                var existsName = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
                if (existsName != null)
                {
                    _logger.LogWarning("Sponsor with name {Name} already exists", sponsor.Name);
                    throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
                }
            }

            if (existingSponsor.ContactEmail != sponsor.ContactEmail)
            {
                var existsEmail = await _sponsorRepository.ExistsByContactEmailAsync(sponsor.ContactEmail);
                if (existsEmail != null)
                {
                    _logger.LogWarning("Sponsor with contact email {Email} already exists", sponsor.ContactEmail);
                    throw new InvalidOperationException($"Ya existe un patrocinador con el correo electrónico '{sponsor.ContactEmail}'");
                }
            }

            if (string.IsNullOrWhiteSpace(sponsor.ContactEmail))
            {
                _logger.LogWarning("Contact email is required for update on sponsor ID {Id}", id);
                throw new InvalidOperationException("El email de contacto es obligatorio.");
            }

            try
            {
                var mail = new System.Net.Mail.MailAddress(sponsor.ContactEmail);
                if (mail.Address != sponsor.ContactEmail)
                    throw new Exception();
            }
            catch
            {
                _logger.LogWarning("Invalid email format for update on sponsor ID {Id}: {Email}",
                    id, sponsor.ContactEmail);
                throw new InvalidOperationException($"El email de contacto '{sponsor.ContactEmail}' no tiene un formato válido.");
            }

            existingSponsor.Name = sponsor.Name;
            existingSponsor.ContactEmail = sponsor.ContactEmail;
            existingSponsor.Phone = sponsor.Phone;
            existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;

            _logger.LogInformation("Updating sponsor with ID: {Id}", id);
            await _sponsorRepository.UpdateAsync(existingSponsor);

            return existingSponsor;
        }

        public async Task UpdateCategoryAsync(int id, SponsorCategory newCategory)
        {
            var sponsor = await _sponsorRepository.GetByIdAsync(id);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {Id} not found for category update", id);
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID '{id}'");
            }

            if (sponsor.Category == newCategory)
            {
                _logger.LogWarning("Sponsor {Id} already has category {Category}", id, newCategory);
                throw new InvalidOperationException("El patrocinador ya tiene esa categoría.");
            }

            sponsor.Category = newCategory;

            _logger.LogInformation(
                "Actualizando categoría del patrocinador {SponsorId} a {NewCategory}",
                id, newCategory);

            await _sponsorRepository.UpdateAsync(sponsor);
        }
    }
}
