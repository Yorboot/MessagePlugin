using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using System;


namespace Oxide.Plugins
{
    [Info("Timed messages", "Royboot", "1.0.0")]
    [Description("Timed messages allows for customizable timed messages, and includes a admin broadcast command")]
    class TimedMessages : CovalencePlugin
    {
        private static PluginConfig? _config;
        private List<float>? _intervals;
        private List<List<string>>? _colors;
        private List<List<string>>? _messages;
        private static bool _isTimerRunning = false;
        private bool _isMessageRedAdminBroadcast = false;
        private int _adminBroadCastFontSize = 12;
        private bool _isAdminBroadCastBold = false;
        private bool _isAdminBroadCastUnderlined = false;
        private bool _isAdminBroadcastItalic = false;
        private void Init()
        {
            //Register the permission needed for the broadcast command
            permission.RegisterPermission("timedmessages.admin", this);
            SetIsTimerRunning(false);
            //Load config values
            LoadConfig();
            _intervals = _config.Intervals;
            _colors = _config.Colors;
            _messages = _config.Messages;
            _isAdminBroadcastItalic = _config.IsAdminBroadcastItalic;
            _isAdminBroadCastBold = _config.IsAdminBroadCastBold;
            _isMessageRedAdminBroadcast = _config.IsMessageRedAdminBroadcast;
            _adminBroadCastFontSize = _config.AdminBroadCastFontSize;
            _isAdminBroadCastUnderlined = _config.IsAdminBroadCastUnderlined;
            //do null checks to make sure everything is in order to start broadcasting messages
            NullChecks();
            StartTimers();
        }
        //set the state to false to prevent memory leaks
        private void OnServerShutdown()
        {
           SetIsTimerRunning(false);
        }
        //define getters and setters to cache the _isTimerRunning var
        private void SetIsTimerRunning(bool value)
        {
            _isTimerRunning = value;
        }

        private bool GetIsTimerRunning()
        {
            return _isTimerRunning;
        }
        //custom function to add in customizations to the Broadcasted messages
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
    
        //method to start running all timers on multiple different intervals
        private void StartTimers()
        {
            //a check to make sure this function does not get set multiple times
            if (!GetIsTimerRunning())
            {
                Puts("Starting timers");
                for (int i = 0; i < _intervals.Count; i++)
                {
                    if (i < _messages.Count && i <_colors.Count)
                    {
                        int index = i;
                        timer.Every(_intervals[index], () =>
                        {
                            BroadcastWipeMessage(_messages[index],_colors);
                        });
                    }
                }
                SetIsTimerRunning(true);
            }
        }
        
        private void NullChecks()
        {
            //run null checks to make sure no lists are left empty else throw a error with wich list is empty
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
                        "Join our Discord: <u><size=16>https://discord.gg/</size></u>",
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

        private string ConvertStyleToString(string message)
        {
            // Apply the red text color
            if (_isMessageRedAdminBroadcast)
            {
                message = $"<color=#ff0000>{message}</color>";
            }

            // Apply an underline
            if (_isAdminBroadCastUnderlined)
            {
                message = $"<u>{message}</u>";
            }

            // Apply the bold effect
            if (_isAdminBroadCastBold)
            {
                message = $"<b>{message}</b>";
            }

            // Apply the italic styling effect
            if (_isAdminBroadcastItalic)
            {
                message = $"<i>{message}</i>";
            }

            // Apply the font size
            if (_adminBroadCastFontSize > 0)
            {
                message = $"<size={_adminBroadCastFontSize}>{message}</size>";
            }

            return message;
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
            message = ConvertStyleToString(message);
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
                        Puts($"message sent succesfuly to {player.displayName} message: {message}");
                        player.ChatMessage(message);
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