﻿using Discord;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Shinoa.Modules
{
    public class AnimeFeedModule : Abstract.UpdateLoopModule
    {
        int UPDATE_INTERVAL = 20 * 1000;

        bool InitialRun = true;

        class AnimeFeedBinding
        {
            [PrimaryKey]
            public string ChannelId { get; set; }
        }

        string FEED_URL = "http://www.nyaa.se/?page=rss&user=64513";
        Queue<string> itemQueue = new Queue<string>(5);

        public override void Init()
        {
            Shinoa.DatabaseConnection.CreateTable<AnimeFeedBinding>();

            foreach (var binding in Shinoa.DatabaseConnection.Table<AnimeFeedBinding>())
            {
                var channel = Shinoa.DiscordClient.GetChannel(ulong.Parse(binding.ChannelId));
                var channelName = channel is IPrivateChannel ? (channel as IPrivateChannel).Name : "#" + (channel as ITextChannel).Name;
                var serverName = channel is IPrivateChannel ? "[PM]" : (channel as ITextChannel).Guild.Name;

                Logging.Log($"  -> [{serverName} -> {channelName}]");
            }

            this.BoundCommands.Add("animefeed", (c) =>
            {
                if (c.Channel is IPrivateChannel || (c.User as IGuildUser).GuildPermissions.ManageGuild)
                {
                    var channelIdString = c.Channel.Id.ToString();

                    if (GetCommandParameters(c.Message.Content)[0] == "enable")
                    {

                        if (Shinoa.DatabaseConnection.Table<AnimeFeedBinding>()
                            .Where(item => item.ChannelId == channelIdString).Count() == 0)
                        {
                            Shinoa.DatabaseConnection.Insert(new AnimeFeedBinding()
                            {
                                ChannelId = c.Channel.Id.ToString()
                            });

                            if (!(c.Channel is IPrivateChannel))
                                c.Channel.SendMessageAsync($"Anime notifications have been bound to this channel (#{c.Channel.Name}).");
                            else
                                c.Channel.SendMessageAsync($"You will now receive anime notifications via PM.");
                        }
                        else
                        {
                            if (!(c.Channel is IPrivateChannel))
                                c.Channel.SendMessageAsync($"Anime notifications are already bound to this channel (#{c.Channel.Name}).");
                            else
                                c.Channel.SendMessageAsync($"You are already receiving anime notifications.");
                        }
                    }
                    else if (GetCommandParameters(c.Message.Content)[0] == "disable")
                    {
                        if (Shinoa.DatabaseConnection.Table<AnimeFeedBinding>()
                            .Where(item => item.ChannelId == channelIdString).Count() == 1)
                        {
                            var currentEntry = Shinoa.DatabaseConnection.Table<AnimeFeedBinding>()
                                .Where(item => item.ChannelId == channelIdString).First();

                            Shinoa.DatabaseConnection.Delete(new AnimeFeedBinding() { ChannelId = channelIdString });

                            if (!(c.Channel is IPrivateChannel))
                                c.Channel.SendMessageAsync($"Anime notifications have been unbound from this channel (#{c.Channel.Name}).");
                            else
                                c.Channel.SendMessageAsync($"You will no lonnger receive anime notifications.");
                        }
                        else
                        {
                            if (!(c.Channel is IPrivateChannel))
                                c.Channel.SendMessageAsync($"Anime notifications are not currently bound to this channel (#{c.Channel.Name}).");
                            else
                                c.Channel.SendMessageAsync($"You are not currently receiving anime notifications.");
                        }
                    }
                }
                else
                {
                    c.Channel.SendPermissionErrorAsync("Manage Server");
                }
            });
        }

        public async override Task UpdateLoop()
        {
            var responseText = HttpGet(FEED_URL);
            var document = XDocument.Load(new MemoryStream(Encoding.Unicode.GetBytes(responseText)));
            var entries = document.Root.Descendants().First(i => i.Name.LocalName == "channel").Elements().Where(i => i.Name.LocalName == "item");

            foreach (var item in entries.Take(5))
            {
                var entryTitle = item.Elements().First(i => i.Name.LocalName == "title").Value;

                Regex regex = new Regex(@"^\[.*\] (?<title>.*) - (?<ep>.*) \[.*$");
                Match match = regex.Match(entryTitle);
                if (match.Success)
                {
                    var showTitle = match.Groups["title"].Value;
                    var episodeNumber = match.Groups["ep"].Value;

                    bool alreadyProcessed = false;
                    foreach (var queueItem in itemQueue)
                    {
                        if (queueItem.Equals(showTitle))
                        {
                            alreadyProcessed = true;
                            break;
                        }
                    }

                    if (!alreadyProcessed)
                    {
                        itemQueue.Enqueue(showTitle);

                        if (!InitialRun)
                        {
                            foreach (var binding in Shinoa.DatabaseConnection.Table<AnimeFeedBinding>())
                            {
                                var channel = Shinoa.DiscordClient.GetChannel(ulong.Parse(binding.ChannelId)) as IMessageChannel;
                                await channel.SendMessageAsync($"```\nNew Episode: {showTitle} ep. {episodeNumber}\n```");
                            }
                        }
                    }
                }
            }

            if (InitialRun) InitialRun = false;
        }
    }
}
