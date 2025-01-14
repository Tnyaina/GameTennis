// MatchHistory.cs
public class MatchHistory
{
    public int MatchId { get; set; }
    public string Player1Name { get; set; }
    public string Player2Name { get; set; }
    public string WinnerName { get; set; }
    public DateTime MatchDate { get; set; }
    public string ScoreProgression { get; set; }

    public MatchHistory()
    {
        Player1Name = string.Empty;
        Player2Name = string.Empty;
        WinnerName = string.Empty;
        ScoreProgression = string.Empty;
    }
}