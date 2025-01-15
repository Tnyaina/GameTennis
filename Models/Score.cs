public class Score
{
    private static readonly int[] TENNIS_POINTS = { 0, 15, 30, 40 };
    private int leftIndex = 0;
    private int rightIndex = 0;
    private bool leftAdvantage = false;
    private bool rightAdvantage = false;
    
    private int leftSets = 0;
    private int rightSets = 0;
    private const int SETS_TO_WIN = 2;
    private const int POINTS_TO_WIN_SET = 4; // Nombre de points nécessaires pour gagner un set

    public bool IsGameOver { get; private set; }
    public bool IsSetOver { get; private set; }
    public bool IsMatchOver { get; private set; }
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

    public string GetSetsScore()
    {
        return $"{leftSets}-{rightSets}";
    }

    private void HandlePointOverflow(bool isLeftPlayer, int remainingPoints)
    {
        while (remainingPoints >= POINTS_TO_WIN_SET && !IsMatchOver)
        {
            if (isLeftPlayer)
            {
                leftSets++;
            }
            else
            {
                rightSets++;
            }

            remainingPoints -= POINTS_TO_WIN_SET;

            // Vérifier si le match est terminé
            if (leftSets >= SETS_TO_WIN || rightSets >= SETS_TO_WIN)
            {
                IsMatchOver = true;
                Winner = isLeftPlayer ? LeftPlayerName : RightPlayerName;
                break;
            }
        }

        // S'il reste des points mais pas assez pour un set complet
        if (remainingPoints > 0 && !IsMatchOver)
        {
            ResetGame();
            if (isLeftPlayer)
            {
                for (int i = 0; i < remainingPoints; i++)
                {
                    if (leftIndex < TENNIS_POINTS.Length - 1)
                    {
                        leftIndex++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < remainingPoints; i++)
                {
                    if (rightIndex < TENNIS_POINTS.Length - 1)
                    {
                        rightIndex++;
                    }
                }
            }
        }
    }

    private void HandleGameWin(bool leftPlayerWon, int extraPoints = 0)
    {
        IsGameOver = true;
        
        if (leftPlayerWon)
        {
            leftSets++;
            Winner = LeftPlayerName;
        }
        else
        {
            rightSets++;
            Winner = RightPlayerName;
        }

        // Vérifier si le match est terminé
        if (leftSets >= SETS_TO_WIN || rightSets >= SETS_TO_WIN)
        {
            IsMatchOver = true;
        }
        else
        {
            // Gérer les points excédentaires
            if (extraPoints > 0)
            {
                ResetGame();
                HandlePointOverflow(leftPlayerWon, extraPoints);
            }
            else
            {
                // Réinitialiser pour le prochain jeu
                ResetGame();
            }
        }
    }

    public void AddPointLeft(int points = 1)
    {
        if (IsMatchOver) return;

        // Calculer combien de sets complets peuvent être gagnés avec les points
        int setsToAdd = points / POINTS_TO_WIN_SET;
        int remainingPoints = points % POINTS_TO_WIN_SET;

        if (setsToAdd > 0)
        {
            // Ajouter directement les sets complets
            for (int i = 0; i < setsToAdd && !IsMatchOver; i++)
            {
                leftSets++;
                if (leftSets >= SETS_TO_WIN)
                {
                    IsMatchOver = true;
                    Winner = LeftPlayerName;
                    return;
                }
            }
        }

        // Gérer les points restants
        if (remainingPoints > 0)
        {
            // Calculer les points nécessaires pour gagner le jeu actuel
            int pointsToWin = 4 - leftIndex;
            if (rightIndex >= 3) pointsToWin++; // Si l'adversaire est à 40 ou advantage

            if (remainingPoints >= pointsToWin)
            {
                // Gagner le jeu actuel et reporter les points excédentaires
                HandleGameWin(true, remainingPoints - pointsToWin);
            }
            else
            {
                // Ajouter les points normalement
                for (int i = 0; i < remainingPoints; i++)
                {
                    if (rightAdvantage)
                    {
                        rightAdvantage = false;
                    }
                    else if (leftAdvantage)
                    {
                        HandleGameWin(true);
                        break;
                    }
                    else if (leftIndex == 3 && rightIndex == 3)
                    {
                        leftAdvantage = true;
                    }
                    else if (leftIndex == 3 && rightIndex < 3)
                    {
                        HandleGameWin(true);
                        break;
                    }
                    else
                    {
                        leftIndex++;
                    }
                }
            }
        }
    }

    public void AddPointRight(int points = 1)
    {
        if (IsMatchOver) return;

        // Même logique que AddPointLeft mais pour le joueur de droite
        int setsToAdd = points / POINTS_TO_WIN_SET;
        int remainingPoints = points % POINTS_TO_WIN_SET;

        if (setsToAdd > 0)
        {
            for (int i = 0; i < setsToAdd && !IsMatchOver; i++)
            {
                rightSets++;
                if (rightSets >= SETS_TO_WIN)
                {
                    IsMatchOver = true;
                    Winner = RightPlayerName;
                    return;
                }
            }
        }

        if (remainingPoints > 0)
        {
            int pointsToWin = 4 - rightIndex;
            if (leftIndex >= 3) pointsToWin++;

            if (remainingPoints >= pointsToWin)
            {
                HandleGameWin(false, remainingPoints - pointsToWin);
            }
            else
            {
                for (int i = 0; i < remainingPoints; i++)
                {
                    if (leftAdvantage)
                    {
                        leftAdvantage = false;
                    }
                    else if (rightAdvantage)
                    {
                        HandleGameWin(false);
                        break;
                    }
                    else if (leftIndex == 3 && rightIndex == 3)
                    {
                        rightAdvantage = true;
                    }
                    else if (rightIndex == 3 && leftIndex < 3)
                    {
                        HandleGameWin(false);
                        break;
                    }
                    else
                    {
                        rightIndex++;
                    }
                }
            }
        }
    }

    private void ResetGame()
    {
        leftIndex = 0;
        rightIndex = 0;
        leftAdvantage = false;
        rightAdvantage = false;
        IsGameOver = false;
        Winner = string.Empty;
    }

    public void ResetMatch()
    {
        ResetGame();
        leftSets = 0;
        rightSets = 0;
        IsMatchOver = false;
    }
}