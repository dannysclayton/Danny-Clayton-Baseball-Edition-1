// Models/PlayerFieldingStats.cs
public class PlayerFieldingStats
{
    public required string PlayerName { get; set; }
    public int G, PO, A, E, DPInduced, CSAgainst;
    public double FieldingPercentage;
}