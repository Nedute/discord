﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using System.Net.Http;
using Discord.Commands;
using Shinoa.Attributes;

namespace Shinoa.Modules
{
    public class JapaneseDictModule : Abstract.Module
    {
        HttpClient httpClient = new HttpClient();

        [@Command("jp", "jisho", "jpdict", "japanese")]
        public async Task JishoSearch(CommandContext c, params string[] args)
        {
            var responseMessage = c.Channel.SendMessageAsync("Searching...").Result;

            var httpResponseText = httpClient.HttpGet($"http://jisho.org/api/v1/search/words?keyword={args.ToRemainderString()}");
            dynamic responseObject = JsonConvert.DeserializeObject(httpResponseText);

            try
            {
                dynamic firstResult = responseObject["data"][0];

                var responseText = "";

                foreach (var word in firstResult["japanese"])
                {
                    var wordKanji = word["word"];
                    var wordReading = word["reading"];

                    if (wordKanji != null && wordReading != null) responseText += $"**{wordKanji}** - {wordReading}, ";
                    else if (wordKanji != null) responseText += $"**{wordKanji}**, ";
                    else if (wordReading != null) responseText += $"**{wordReading}**, ";
                }

                responseText = responseText.Trim(new char[] { ',', ' ' });
                responseText += '\n';

                foreach (var sense in firstResult["senses"])
                {
                    responseText += "\u2022 ";

                    foreach (var definition in sense["english_definitions"])
                    {
                        responseText += $"{definition}, ";
                    }

                    responseText = responseText.Trim(new char[] { ',', ' ' });

                    if (sense["parts_of_speech"].Count > 0)
                    {
                        responseText += " (";

                        foreach (string part in sense["parts_of_speech"])
                        {
                            responseText += $"{part.ToLower()}, ";
                        }

                        responseText = responseText.Trim(new char[] { ',', ' ' });

                        responseText += ")";
                    }

                    responseText += '\n';
                }

                responseText += $"\nSee more: <http://jisho.org/search/{System.Uri.EscapeUriString(args.ToRemainderString())}>";

                await responseMessage.ModifyAsync(p => p.Content = responseText);
            }
            catch (Exception)
            {
                await responseMessage.ModifyAsync(p => p.Content = "Not found.");
            }
        }
    }
}
