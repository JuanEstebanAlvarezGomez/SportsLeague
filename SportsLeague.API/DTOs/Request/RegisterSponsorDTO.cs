namespace SportsLeague.API.DTOs.Request
{
    public class RegisterSponsorDTO
    {
        public int TournamentId { get; set; }
        public decimal ContractAmount { get; set; }
    }
}
