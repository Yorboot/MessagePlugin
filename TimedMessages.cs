using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("TimedMessages", "Royboot", "0.1.0")]
    [Description("Makes epic stuff happen")]
    class TimedMessages : CovalencePlugin
    {
        private static PluginConfig? _config;
        private List<float> _intervals;// 3600 seconds = 1 hour
        private List<string> _colors;
        private List<string> _messages;
        private static bool _isTimerRunning = false;
        private void Init()
        {
            _isTimerRunning = false;
            LoadConfig();
            _intervals = _config.Intervals;
            _colors = _config.Colors;
            _messages = _config.Messages;
            timer.Every(_intervals[0], () =>
            {
                _isTimerRunning = true;
                Puts("Message plugin enabled.");
                BroadcastWipeMessage(_messages,_colors);
            });
        }

        void Loaded()
        {
            _isTimerRunning = false;
            if (!_isTimerRunning)
            {
                timer.Every(_intervals[0], () =>
                {
                    Puts("Timed Messages plugin enabled.");
                    BroadcastWipeMessage(_messages, _colors);
                });

            }
        }
        private void BroadcastWipeMessage(List<string> messages,List<string> colors)
        {
            
            const string closingTag = "</color>";    
            List<string> finalMessages = new List<string>();
            for (int i = 0; i < messages.Count; i++)
            {
                if (i < colors.Count)
                {
                    string colorTag = $"<color={colors[i]}>";
                    if (colorTag == "<color=>")
                    {
                        continue;
                    }
                    finalMessages.Add(colorTag+messages[i]+closingTag);
                }
            }
            foreach (var message in finalMessages)
            {
                server.Broadcast(message);
            }
        }
        

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config= Config.ReadObject<PluginConfig>();

            if (_config == null)
            {
                PrintWarning("Configuration file is missing or invalid. Loading default settings.");
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(GetDefaultConfig(), true);
        }

        private static PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                Messages = new List<string>()
                {
                    "[Wipe Reminder] Server wipes every Friday at 5PM EST!",
                    "Join our Discord: <u><size=16>https://discord.gg/ZnEVyCtCQv</size></u>",
                },
                Colors = new List<string>()
                {
                    "#ffa500",
                    "#00ffff",
                },
                Intervals = new List<float>()
                {
                    60.0f,
                }
            };
        }
        [Command("Broadcast")]
        private void BroadCast(IPlayer sender, string command, string[] args)
        {
            var playerlist = BasePlayer.activePlayerList;
            string message = string.Join(" ", args);
            if (args.Length == 0)
            {
                sender.Message("Usage: /broadcast <message>");
                return;
            }
            foreach (var player in playerlist)
            {
                if (player != null)
                {
                    if (args.Length > 0 && args.Length == 1)
                    {
                        player.ChatMessage(message);
                    }
                    sender.Message($"Message send succesful {message}");
                }
            }
            if (!_isTimerRunning)
            {
                Puts(_intervals[0].ToString());
                timer.Every(_intervals[0], () =>
                {
                    Puts("Timer started");
                    BroadcastWipeMessage(_messages, _colors);
                });
            }

            return;
        }
    }
    

    class PluginConfig
    {
        public List<string> Messages { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<float> Intervals { get; set; } = new List<float>();
    }
}