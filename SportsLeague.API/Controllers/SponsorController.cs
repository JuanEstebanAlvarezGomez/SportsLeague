using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Services;

namespace SportsLeague.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class SponsorController : ControllerBase
    {
        private readonly ISponsorService _sponsorService;
        private readonly IMapper _mapper;
        private readonly ILogger<SponsorController> _logger;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;

        public SponsorController(
        ISponsorService sponsorService,
        IMapper mapper,
        ILogger<SponsorController> logger,
        ITournamentSponsorRepository tournamentSponsorRepository)
        {
            _sponsorService = sponsorService;
            _mapper = mapper;
            _logger = logger;
            _tournamentSponsorRepository = tournamentSponsorRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
        {
            var sponsors = await _sponsorService.GetAllAsync();
            var sponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);

            return Ok(sponsorsDto);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
        {
            var sponsor = await _sponsorService.GetByIdAsync(id);

            if (sponsor == null)
                return NotFound(new { message = $"Patrocinador con ID '{id}' no encontrado" });

            var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
            return Ok(sponsorDto);
        }

        [HttpGet("search/{term}")]
        public async Task<ActionResult<SponsorResponseDTO>> GetByNameOrEmail(string term)
        {
            try
            {
                _logger.LogInformation("Buscando sponsor con término: {Term}", term);

                var sponsor = await _sponsorService.GetByNameAsync(term);

                if (sponsor == null)
                {
                    sponsor = await _sponsorService.GetByContactEmailAsync(term);
                }

                if (sponsor == null)
                {
                    _logger.LogWarning("Sponsor no encontrado con término: {Term}", term);
                    return NotFound(new { message = $"Patrocinador con nombre o email '{term}' no encontrado" });
                }

                _logger.LogInformation("Sponsor encontrado: {Name} - {Email}", sponsor.Name, sponsor.ContactEmail);

                var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
                return Ok(sponsorDto);
            }
            catch (KeyNotFoundException) 
            {
                try
                {
                    var sponsor = await _sponsorService.GetByContactEmailAsync(term);

                    if (sponsor == null)
                        return NotFound(new { message = $"Patrocinador con nombre o email '{term}' no encontrado" });

                    var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
                    return Ok(sponsorDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error buscando por email");
                    return NotFound(new { message = $"Patrocinador con nombre o email '{term}' no encontrado" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando sponsor con término '{Term}'", term);
                return NotFound(new { message = $"Patrocinador con nombre o email '{term}' no encontrado" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
        {
            try
            {
                var sponsor = _mapper.Map<Sponsor>(dto);
                var createdSponsor = await _sponsorService.CreateAsync(sponsor);
                var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = responseDto.Id },
                    responseDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating sponsor with name '{Name}' and email '{Email}'",
                    dto.Name, dto.ContactEmail);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]

        public async Task<ActionResult<SponsorResponseDTO>> Update(int id, SponsorRequestDTO dto)
        {
            try
            {
                var sponsor = _mapper.Map<Sponsor>(dto);
                var updatedSponsor = await _sponsorService.UpdateAsync(id, sponsor);

                if (updatedSponsor == null)
                    return NotFound(new { message = $"Patrocinador con ID {id} no encontrado" });

                var responseDto = _mapper.Map<SponsorResponseDTO>(updatedSponsor);
                return Ok(responseDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating sponsor with ID '{Id}' to name '{Name}' and email '{Email}'",
                    id, dto.Name, dto.ContactEmail);
                return Conflict(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _sponsorService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sponsor with ID {Id} not found for deletion", id);
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/category")]

        public async Task<ActionResult> UpdateCategory(int id, UpdateCategoryDTO dto)
        {
            try
            {
                await _sponsorService.UpdateCategoryAsync(id, dto.Category);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sponsor with ID {Id} not found for category update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating category for sponsor with ID {Id} to category {Category}",
                    id, dto.Category);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/tournaments")]
        public async Task<ActionResult<TournamentSponsorResponseDTO>> RegisterSponsorToTournament(int id, RegisterSponsorDTO dto)
        {
            try
            {
                var tournamentSponsor = await _sponsorService.RegisterSponsorToTournamentAsync
                    (id, dto.TournamentId, dto.ContractAmount);
                var responseDto = _mapper.Map<TournamentSponsorResponseDTO>(tournamentSponsor);
                return Ok(responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sponsor with ID {Id} or Tournament with ID {TournamentId} not found for registration",
                    id, dto.TournamentId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error registering sponsor with ID {Id} to tournament with ID {TournamentId}",
                    id, dto.TournamentId);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/tournaments")]
        public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetTournamentsBySponsor(int id)
        {
            try
            {
                var tournaments = await _sponsorService.GetTournamentsBySponsorAsync(id);
                var tournamentSponsors = await _tournamentSponsorRepository.GetBySponsorIdAsync(id);
                var responseDto = _mapper.Map<IEnumerable<TournamentSponsorResponseDTO>>(tournamentSponsors);
                return Ok(responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sponsor with ID {Id} not found when retrieving tournaments", id);
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{sponsorId}/tournaments/{tournamentId}")]
        public async Task<ActionResult> RemoveSponsorFromTournament(int sponsorId, int tournamentId)
        {
            try
            {
                await _sponsorService.RemoveSponsorFromTournamentAsync(sponsorId, tournamentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Sponsor with ID {SponsorId} or Tournament with ID {TournamentId} not found for unregistration",
                    sponsorId, tournamentId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error unregistering sponsor with ID {SponsorId} from tournament with ID {TournamentId}",
                    sponsorId, tournamentId);
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
