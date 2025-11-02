public class Coach
{
    // The missing property that fixes the build error in SettingsScreen.cs
    public required string Name { get; set; }
    
    // Properties previously defined for gameplay logic
    public required string Level { get; set; } // Bad, Bearable, Average, Above Average, Exceptional
    public required string Style { get; set; } // Aggressive, Conservative, Run of the Mill
    public double Experience { get; set; } // Cumulative score for season/career
    public string Status { get; set; } = "Active"; // Used for Hall of Fame (Legendary) or Fired

    public double DecisionModifier
    {
        get
        {
            return Level switch // Calculates the ± percentage change for decision-making
            {
                "Exceptional" => 0.05,
                "Above Average" => 0.03,
                "Average" => 0.00,
                "Bearable" => -0.03,
                "Bad" => -0.05,
                _ => 0.00,
            };
        }
    }
}
