using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System;


namespace Oxide.Plugins
{
    [Info("TimedMessages", "Royboot", "1.0.0")]
    [Description("Makes epic stuff happen")]
    class TimedMessages : CovalencePlugin
    {
        private static PluginConfig? _config;
        private List<float>? _intervals;// 3600 seconds = 1 hour
        private List<string>? _colors;
        private List<List<string>>? _messages;
        private static bool _isTimerRunning = false;
        private bool _isMessageRedAdminBroadcast = false;
        private int _adminBroadCastFontSize = 12;
        private bool _isAdminBroadCastBold = false;
        private bool _isAdminBroadCastUnderlined = false;
        private bool _isAdminBroadcastItalic = false;
        private void Init()
        {
            permission.RegisterPermission("timedmessages.admin", this);
            _isTimerRunning = false;
            LoadConfig();
            _intervals = _config.Intervals;
            _colors = _config.Colors;
            _messages = _config.Messages;
            NullChecks();
            StartTimers();
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

            if (finalMessages.Count > 0)
            {
                foreach (var message in finalMessages)
                {
                    server.Broadcast(message);
                }
            }
            else
            {
                Puts("There are no final messages to broadcast");
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
            if (!_isTimerRunning)
            {
                Puts("Starting timers");
                for (int i = 0; i < _intervals.Count; i++)
                {
                    if (i < _messages.Count && i <_colors.Count)
                    {
                        int index = i;
                        foreach (string str in _messages[i])
                        {
                            Puts(str);
                        }
                        timer.Every(_intervals[index], () =>
                        {
                            _isTimerRunning = true;
                            BroadcastWipeMessage(_messages[index],_colors);
                        });
                    }
                }
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
                },
                IsAdminBroadcastItalic = false,
                IsAdminBroadCastBold = false,
                IsAdminBroadCastUnderlined = false,
                IsMessageRedAdminBroadcast = false,
                AdminBroadCastFontSize = 12,
            };
        }
        
        [Command("Broadcast")]
        private void BroadCast(IPlayer sender, string command, string[] args)
        {
            if (!sender.HasPermission("timedmessages.admin"))
            {
                sender.Message("You do not have permission to use this command this is a admin only command.");
                return;
            }
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
        public bool IsMessageRedAdminBroadcast { get; set; } = false;
        public int AdminBroadCastFontSize { get; set; } = 12;
        public bool IsAdminBroadCastBold { get; set; } = false;
        public bool IsAdminBroadCastUnderlined { get; set; } = false;
        public bool IsAdminBroadcastItalic { get; set; } = false;
    }
}