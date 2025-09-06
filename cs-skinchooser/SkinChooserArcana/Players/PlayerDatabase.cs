using CounterStrikeSharp.API;
using Dapper;
using SkinChooserArcana.IO;
using System.Data.SQLite;

namespace SkinChooserArcana.Players
{
    public class PlayerDatabase
    {
        private Dictionary<ulong, PlayerPreferences> _players = new Dictionary<ulong, PlayerPreferences>();

        private ISout _sout;
        private string _databasePath;

        public PlayerDatabase(string databasePath, ISout sout)
        {
            _sout = sout;
            _databasePath = databasePath;

            Server.NextFrameAsync(async () =>
            {
                await InitializeDatabase();
            });
        }

        private async Task<SQLiteConnection> ConnectAsync()
        {
            var conn = new SQLiteConnection($"Data Source={_databasePath}");
            await conn.OpenAsync();
            return conn;
        }

        private async Task InitializeDatabase()
        {
            using var conn = await ConnectAsync();

            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `Players` (
                    `Id` UNSIGNED BIG INT NOT NULL,
                    `SkinName` STRING NOT NULL DEFAULT '',
                    `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (`Id`)
                );
            ");

            var players = (await conn.QueryAsync<PlayerPreferences>("SELECT * from `Players`;")).ToList();
            foreach (var player in players)
            {
                _players[player.Id] = player;
            }

            _sout.Log($"Database has been loaded. Num players = {players.Count}");
        }

        public string GetSkinForPlayer(ulong id)
        {
            var playerPrefs = _players.GetValueOrDefault(id);
            if (playerPrefs == null)
            {
                return "";
            }
            return playerPrefs.SkinName ?? "";
        }

        public void SetSkinForPlayer(ulong id, string skinName)
        {
            var playerPrefs = _players.GetValueOrDefault(id);
            if (playerPrefs == null)
            {
                playerPrefs = new PlayerPreferences()
                {
                    Id = id,
                    SkinName = skinName,
                };
                _players.Add(id, playerPrefs);
            }

            playerPrefs.SkinName = skinName;

            Server.NextFrameAsync(async () =>
            {    
                using var conn = await ConnectAsync();
                
                await conn.ExecuteAsync(@"
                    INSERT INTO `Players` (`Id`, `SkinName`) VALUES (@id, @skinName)
                    ON CONFLICT(`Id`) DO UPDATE SET `SkinName` = @skinName;
                ", new
                {
                    id,
                    skinName,
                });

                _sout.Log($"Update database for player {id}, skin = {skinName}");
            });
        }
    }
}
