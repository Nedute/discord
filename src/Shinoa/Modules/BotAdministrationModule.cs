﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Net.Http;
using System.IO;

namespace Shinoa.Modules
{
    public class BotAdministrationModule : Abstract.Module
    {
        public override void Init()
        {            
            this.BoundCommands.Add("setavatar", (e) =>
            {
                if (e.User.Id == ulong.Parse(Shinoa.Config["owner_id"]))
                {
                    var stream = new HttpClient().GetAsync(GetCommandParameters(e.Message.RawText)[0]).Result.Content.ReadAsStreamAsync().Result;
                    Shinoa.DiscordClient.CurrentUser.Edit(avatar: stream);
                }
            });

            this.BoundCommands.Add("stats", (e) =>
            {
                if (e.User.Id == ulong.Parse(Shinoa.Config["owner_id"]))
                {
                    e.Channel.SendMessage(GenerateStatsMessage());
                }
            });
        }

        string GenerateStatsMessage()
        {
            var output = "";
            output += "== Shinoa ==\n";

            var computerName = Environment.MachineName;
            var uptime = (DateTime.Now - Shinoa.StartTime);
            var uptimeString = $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds.";

            output += $"Running on {computerName}\n";
            output += $"Uptime: {uptimeString}\n\n";

            output += "Running modules:\n\n```";
            foreach (var module in Shinoa.RunningModules)
            {
                output += $"{module.GetType().Name}\n";
                var indentedDetailedStats = "";
                
                if (module.DetailedStats != null)
                {
                    using (StringReader reader = new StringReader(module.DetailedStats))
                    {
                        string line = string.Empty;
                        do
                        {
                            line = reader.ReadLine();
                            if (line != null)
                            {
                                indentedDetailedStats += "  " + line + '\n';
                            }

                        } while (line != null);
                    }

                    output += $"{indentedDetailedStats}\n";
                }                
            }
            output += "```";

            return output;
        }
    }
}