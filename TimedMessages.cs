using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System;
namespace Oxide.Plugins
{
    [Info("TimedMessages", "Royboot", "0.1.0")]
    [Description("Makes epic stuff happen")]
    class TimedMessages : CovalencePlugin
    {
        private static PluginConfig? _config;
        private List<float>? _intervals;// 3600 seconds = 1 hour
        private List<string>? _colors;
        private List<List<string>>? _messages;
        private static bool _isTimerRunning = false;
        private void Init()
        {
            _isTimerRunning = false;
            LoadConfig();
            _intervals = _config.Intervals;
            _colors = _config.Colors;
            _messages = _config.Messages;
            NullChecks();
            StartTimers();
        }

        void Loaded()
        {
            _isTimerRunning = false;
            if (!_isTimerRunning)
            {
                timer.Every(_intervals[0], () =>
                {
                    Puts("Timed Messages plugin enabled.");
                    BroadcastWipeMessage(_messages[0], _colors);
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

        private void StartTimers()
        {
            for (int i = 0; i < _intervals.Count; i++)
            {
            if(i !> _messages.Count)
                timer.Every(_intervals[i], () =>
                {
                    _isTimerRunning = true;
                    BroadcastWipeMessage(_messages[i],_colors);
                });
            }
        }
        private void NullChecks()
        {
            string failedCheck = null;

            if (_intervals.Count == 0) failedCheck = "_intervals";
            else if (_colors.Count == 0) failedCheck = "_colors";
            else if (_messages.Count == 0) failedCheck = "_messages";
            else if (_messages[0].Count == 0) failedCheck = "_messages[0]";

            if (failedCheck != null)
            {
                throw new Exception($"Config option {failedCheck} has failed. Please check config.");
            }
        }
        private static PluginConfig GetDefaultConfig()
        {
            return new PluginConfig
            {
                Messages = new List<List<string>>()
                {
                    new List<string>() {
                        "[Wipe Reminder] Server wipes every Friday at 5PM EST!",
                        "Join our Discord: <u><size=16>https://discord.gg/ZnEVyCtCQv</size></u>",
                    },
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
                    if (args.Length > 0)
                    {
                        string finalMessage = "<color=#fc0303>" + message + "</color>";
                        Puts($"message sent succesfuly to {player.displayName} message: {finalMessage}");
                        player.ChatMessage(finalMessage);
                    }
                    sender.Message($"Message send succesful {message}");
                }
            }
        }
    }
    

    class PluginConfig
    {
        public List<List<string>> Messages { get; set; } = new List<List<string>>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<float> Intervals { get; set; } = new List<float>();
    }
}