using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
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

        public SponsorController(
        ISponsorService sponsorService,
        IMapper mapper,
        ILogger<SponsorController> logger)
        {
            _sponsorService = sponsorService;
            _mapper = mapper;
            _logger = logger;
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
                    return StatusCode(500, new { message = "Error interno del servidor" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando sponsor con término '{Term}'", term);
                return StatusCode(500, new { message = "Error interno del servidor" });
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
    }
}
