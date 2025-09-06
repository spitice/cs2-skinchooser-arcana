using CounterStrikeSharp.API.Core;

namespace SkinChooserArcana.IO
{
    public interface ISout
    {
        void ToPlayer(CCSPlayerController player, string message, params object[] args);
        void ToPlayerWithoutPrefix(CCSPlayerController player, string message, params object[] args);
        void ToAllPlayers(string message, params object[] args);
        void ToAllPlayersWithoutPrefix(string message, params object[] args);
        void Log(string message);
        void Error(string message);
    }
}
