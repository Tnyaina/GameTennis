public class Score
{
    private static readonly int[] TENNIS_POINTS = { 0, 15, 30, 40 };
    private int leftIndex = 0;
    private int rightIndex = 0;
    private bool leftAdvantage = false;
    private bool rightAdvantage = false;

    public bool IsGameOver { get; private set; }
    public string Winner { get; private set; } = string.Empty;
    public string LeftPlayerName { get; set; }
    public string RightPlayerName { get; set; }

    public Score(string leftPlayerName, string rightPlayerName)
    {
        LeftPlayerName = leftPlayerName;
        RightPlayerName = rightPlayerName;
    }

    public string GetLeftScore()
    {
        if (leftAdvantage) return "AV";
        return leftIndex < TENNIS_POINTS.Length ? TENNIS_POINTS[leftIndex].ToString() : "40";
    }

    public string GetRightScore()
    {
        if (rightAdvantage) return "AV";
        return rightIndex < TENNIS_POINTS.Length ? TENNIS_POINTS[rightIndex].ToString() : "40";
    }

    public void AddPointLeft(int points = 1)
    {
        if (IsGameOver) return;

        for (int i = 0; i < points; i++)
        {
            if (rightAdvantage)
            {
                rightAdvantage = false;
                continue;
            }

            // Changement de l'ordre des conditions
            if (leftAdvantage)
            {
                // Si gauche a l'avantage et marque encore, c'est gagné
                IsGameOver = true;
                Winner = LeftPlayerName;
                break;
            }
            else if (leftIndex == 3 && rightIndex == 3)
            {
                leftAdvantage = true;
            }
            else if (leftIndex == 3 && rightIndex < 3)
            {
                IsGameOver = true;
                Winner = LeftPlayerName;
                break;
            }
            else
            {
                leftIndex++;
            }
        }
    }

    public void AddPointRight(int points = 1)
    {
        if (IsGameOver) return;

        for (int i = 0; i < points; i++)
        {
            if (leftAdvantage)
            {
                leftAdvantage = false;
                continue;
            }

            // Changement de l'ordre des conditions
            if (rightAdvantage)
            {
                // Si droite a l'avantage et marque encore, c'est gagné
                IsGameOver = true;
                Winner = RightPlayerName;
                break;
            }
            else if (leftIndex == 3 && rightIndex == 3)
            {
                rightAdvantage = true;
            }
            else if (rightIndex == 3 && leftIndex < 3)
            {
                IsGameOver = true;
                Winner = RightPlayerName;
                break;
            }
            else
            {
                rightIndex++;
            }
        }
    }

    public void Reset()
    {
        leftIndex = 0;
        rightIndex = 0;
        leftAdvantage = false;
        rightAdvantage = false;
        IsGameOver = false;
        Winner = string.Empty;
    }
}