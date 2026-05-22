using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly MatchValidationHelper _validationHelper;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchLineupRepository matchLineupRepository,
        IMatchRepository matchRepository,
        MatchValidationHelper validationHelper,
        ILogger<MatchLineupService> logger)
    {
        _matchLineupRepository = matchLineupRepository;
        _matchRepository = matchRepository;
        _validationHelper = validationHelper;
        _logger = logger;
    }

    public async Task<MatchLineup> AddToLineupAsync(int matchID, MatchLineup lineup)
    {
        var match = await _validationHelper.ValidateMatchForLineupAsync(matchID);

        var player = await _validationHelper.ValidatePlayerInMatchAsync(lineup.PlayerId, match);

        var exists = await _matchLineupRepository.ExistsByMatchAndPlayerAsync(matchID, lineup.PlayerId);
        if (exists)
        {
            throw new InvalidOperationException("El jugador ya está registrado en la alineación de este partido");
        }

        if (lineup.IsStarter)
        {
            var teamId = player.TeamId;
            var currentStarters = await _matchLineupRepository.VerifyNumberOfStartersByMatchAndTeamAsync(matchID, teamId);

            if (currentStarters >= 11)
            {
                throw new InvalidOperationException("El equipo ya tiene 11 titulares registrados en este partido");
            }
        }

        lineup.MatchId = matchID;

        _logger.LogInformation(
            "Adding player {PlayerId} to lineup for match {MatchId} as {IsStarter} playing {Position}",
            lineup.PlayerId, matchID, lineup.IsStarter ? "starter" : "substitute", lineup.Position);

        return await _matchLineupRepository.CreateAsync(lineup);
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
    {
        {
            var match = await _matchRepository.GetByIdAsync(matchId);
            if (match == null)
                throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

            if (teamId != match.HomeTeamId && teamId != match.AwayTeamId)
                throw new InvalidOperationException("El equipo no juega en este partido");

            return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
        }
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAsync(matchId);
    }

    public async Task RemoveFromLineupAsync(int id)
    {
        var exists = await _matchLineupRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró al jugador con ID {id}");

        _logger.LogInformation("Removing lineup entry with ID {LineupId}", id);
        await _matchLineupRepository.DeleteAsync(id);
    }
}