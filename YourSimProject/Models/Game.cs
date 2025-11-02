public class Game
{
    public required string HomeTeamName { get; set; }
    public required string AwayTeamName { get; set; }
    public bool IsPlayed { get; set; }
    public int FinalScoreHome { get; set; }
    public int FinalScoreAway { get; set; }
}
