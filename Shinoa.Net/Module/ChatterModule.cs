﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace Shinoa.Net.Module
{
    class ChatterModule : IModule
    {
        TimeSpan MinimumGreetingInterval = TimeSpan.FromMinutes(10);
        DateTime LastGreetingTime;        

        public void Init()
        {
            LastGreetingTime = DateTime.Now.Subtract(MinimumGreetingInterval);
        }

        public void MessageReceived(object sender, MessageEventArgs e)
        {
            var cleanMessage = Convenience.RemoveMentions(e.Message.RawText).Trim().ToLower();

            if (e.Message.User.Id != ShinoaNet.DiscordClient.CurrentUser.Id)
            {                
                if (e.Message.Text.Trim().Equals(@"/o/"))
                {
                    e.Channel.SendMessage(@"\o\");
                    Logging.LogMessage(e.Message);
                }
                else if (e.Message.Text.Trim().Equals(@"\o\"))
                {
                    e.Channel.SendMessage(@"/o/");
                    Logging.LogMessage(e.Message);
                }
                else if (cleanMessage.Equals("soon"))
                {
                    Logging.LogMessage(e.Message);

                    List<object> soonImages = ShinoaNet.Config["soon_images"];
                    e.Channel.SendMessage((string) soonImages[new Random().Next(soonImages.Count)]);
                }
                else if (cleanMessage.Contains("stoppu desu"))
                {
                    Logging.LogMessage(e.Message);
                    e.Channel.SendMessage("http://i.imgur.com/It8FH8b.jpg");
                }
                else if (cleanMessage.Equals("keikaku doori") || cleanMessage.Equals("計画通り"))
                {
                    var keikakuDooriImages = ShinoaNet.Config["keikaku_doori"];
                    e.Channel.SendMessage((string)keikakuDooriImages[new Random().Next(keikakuDooriImages.Count)]);
                }
            }
        }

        public string DetailedStats()
        {
            return null;
        }
    }
}
