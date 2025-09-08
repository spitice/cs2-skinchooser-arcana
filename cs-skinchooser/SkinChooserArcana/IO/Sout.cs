using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace SkinChooserArcana.IO
{
    public class Sout: ISout
    {
        private ILogger _logger;
        private IStringLocalizer _localizer;
        private string? _translationKeyPrefix;
        private string? _messagePrefix;
        private char _chatMessagePrefixColor;

        public Sout(ILogger logger, IStringLocalizer localizer, string? translationKeyPrefix, string? messagePrefix, char chatMessagePrefixColor)
        {
            _logger = logger;
            _localizer = localizer;
            _translationKeyPrefix = translationKeyPrefix;
            _messagePrefix = messagePrefix;
            _chatMessagePrefixColor = chatMessagePrefixColor;
        }

        private string WithPrefix(string text)
        {
            if (_messagePrefix == null)
            {
                return text;
            }
            // Requires a leading space to apply custom color from the beginning of the chat message.
            return $" {_chatMessagePrefixColor}[{_messagePrefix}]{ChatColors.Default} {text}";
        }

        public void ToPlayer(CCSPlayerController? player, string message, params object[] args)
        {
            ToPlayerImpl(player, message, args, true);
        }

        public void ToPlayerWithoutPrefix(CCSPlayerController? player, string message, params object[] args)
        {
            ToPlayerImpl(player, message, args, false);
        }

        private void ToPlayerImpl(CCSPlayerController? player, string message, object[] args, bool isPrefix)
        {
            Server.NextFrame(() =>
            {
                var translationKey = message;
                if (_translationKeyPrefix != null)
                {
                    translationKey = $"{_translationKeyPrefix}.{translationKey}";
                }

                var localizedMessage = _localizer.ForPlayer(player, translationKey, args);
                if (localizedMessage == translationKey)
                {
                    // If failed to localize the message, just show the message as is
                    localizedMessage = message;
                }

                if (player == null)
                {
                    Log(localizedMessage);
                }
                else
                {
                    if (isPrefix)
                    {
                        localizedMessage = WithPrefix(localizedMessage);
                    }
                    player.PrintToChat(localizedMessage);
                }
            });
        }

        public void ToAllPlayers(string message, params object[] args)
        {
            ToAllPlayersImpl(message, args, true);
        }

        public void ToAllPlayersWithoutPrefix(string message, params object[] args)
        {
            ToAllPlayersImpl(message, args, false);
        }

        private void ToAllPlayersImpl(string message, object[] args, bool isPrefix)
        {
            foreach (var player in Utilities.GetPlayers().Where(x => x.IsValid))
            {
                ToPlayerImpl(player, message, args, isPrefix);
            }
        }

        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }
    }
}
