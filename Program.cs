using System;
using NuGet.Protocol;
using Oxide.Plugins;
using Oxide.Core;
using Oxide.Game.Rust.Cui;


namespace Oxide.Plugins
{
    [Info("Epic Stuff", "Unknown Author", "0.1.0")]
    [Description("Makes epic stuff happen")]
    class MessagePlugin : CovalencePlugin
    {
        private static PluginConfig config;
        private float interval = config.Interval; // 3600 seconds = 1 hour
        private List<string> Colors = config.Colors;
        private List<string> Messages = config.Messages;
        private void Init()
        {
            timer.Every(interval, () =>
            {
                BroadcastWipeMessage(Messages,Colors);
            });
        }
        private void BroadcastWipeMessage(List<string> messages,List<string> Colors)
        {
            
            string closingTag = "</color>";    
            List<string> finalMessages = new List<string>();
            for (int i = 0; i < messages.Count; i++)
            {
                if (i < Colors.Count)
                {
                    string ColorTag =  "<color=#" + Colors[i] + ">";
                    finalMessages.Add(ColorTag+messages[i]+closingTag);
                }
            }
            foreach (var message in messages)
            {
                server.Broadcast(message);
            }
        }
        

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config= Config.ReadObject<PluginConfig>();

            if (config == null)
            {
                PrintWarning("Configuration file is missing or invalid. Loading default settings.");
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            Config.WriteObject(GetDefaultConfig(), true);
        }

        public PluginConfig GetDefaultConfig()
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
                    "<color=#ffa500>",
                    "<color=#00ffff>",
                },
                Interval = 3600f
            };
        }
    }
    

    class PluginConfig
    {
        public List<string> Messages { get; set; }
        public List<string> Colors = new List<string>();
        public float Interval;
    }
}