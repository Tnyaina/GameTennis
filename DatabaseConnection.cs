using System;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace GameTennis.Data
{
    public class DatabaseConnection
    {
        private readonly string connectionString;

        public DatabaseConnection()
        {
            connectionString = "Data Source=localhost:1521/OracleDB;User Id=NYX;Password=tiger;";
        }

        public OracleConnection GetConnection()
        {
            return new OracleConnection(connectionString);
        }

        public async Task<List<MatchHistory>> GetMatchHistory()
        {
            var matches = new List<MatchHistory>();

            using var connection = GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT *
                FROM V_TENNIS_HISTORY
                ORDER BY MATCH_DATE DESC";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                matches.Add(new MatchHistory
                {
                    MatchId = reader.GetInt32(0),
                    Player1Name = reader.GetString(1),
                    Player2Name = reader.GetString(2),
                    WinnerName = reader.GetString(3),
                    MatchDate = reader.GetDateTime(4),
                    ScoreProgression = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                });
            }

            return matches;
        }

        public async Task<int> SaveMatch(string player1Name, string player2Name, string winnerName)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "INSERT_TENNIS_MATCH";

            command.Parameters.Add("p_player1_name", OracleDbType.Varchar2).Value = player1Name;
            command.Parameters.Add("p_player2_name", OracleDbType.Varchar2).Value = player2Name;
            command.Parameters.Add("p_winner_name", OracleDbType.Varchar2).Value = winnerName;

            var matchIdParam = new OracleParameter("p_match_id", OracleDbType.Int32);
            matchIdParam.Direction = ParameterDirection.Output;
            command.Parameters.Add(matchIdParam);

            await command.ExecuteNonQueryAsync();

            int matchId = Convert.ToInt32(matchIdParam.Value.ToString());
            await SaveInitialScore(matchId, connection);
            
            return matchId;
        }

        public async Task UpdateMatchWinner(int matchId, string winnerName)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE TENNIS_MATCHES SET WINNER_NAME = :winner WHERE MATCH_ID = :matchId";
            
            command.Parameters.Add(":winner", OracleDbType.Varchar2).Value = winnerName;
            command.Parameters.Add(":matchId", OracleDbType.Int32).Value = matchId;

            await command.ExecuteNonQueryAsync();
        }

        private async Task SaveInitialScore(int matchId, OracleConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "INSERT_TENNIS_POINT";

            command.Parameters.Add("p_match_id", OracleDbType.Int32).Value = matchId;
            command.Parameters.Add("p_player1_score", OracleDbType.Varchar2).Value = "0";
            command.Parameters.Add("p_player2_score", OracleDbType.Varchar2).Value = "0";

            await command.ExecuteNonQueryAsync();
        }

        public async Task SavePoint(int matchId, string player1Score, string player2Score)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "INSERT_TENNIS_POINT";

            command.Parameters.Add("p_match_id", OracleDbType.Int32).Value = matchId;
            command.Parameters.Add("p_player1_score", OracleDbType.Varchar2).Value = player1Score;
            command.Parameters.Add("p_player2_score", OracleDbType.Varchar2).Value = player2Score;

            await command.ExecuteNonQueryAsync();
        }
    }
}