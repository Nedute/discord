﻿using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shinoa
{
    public static class Util
    {
        public static async Task SendPermissionErrorAsync(this IMessageChannel channel, string permissionName)
        {
            await channel.SendMessageAsync($"Sorry, but you need the `{permissionName}` permission to do that.");
        }

        public static ulong IdFromMention(string mentionString)
        {
            var idString = mentionString
                .Trim()
                .Replace("<", "")
                .Replace(">", "")
                .Replace("@", "")
                .Replace("!", "");

            return ulong.Parse(idString);
        }

        public static string RemoveMentions(string message)
        {
            var mentionRegexPattern = @"<@.*>";
            var mentionRegex = new Regex(mentionRegexPattern);

            return mentionRegex.Replace(message, "");
        }

        public static Task<IUserMessage> SendEmbedAsync(this IMessageChannel channel, Embed embed)
        {
            return channel.SendMessageAsync("", embed: embed);
        }

        public static Task ModifyToEmbedAsync(this IUserMessage message, Embed embed)
        {
            return message.ModifyAsync(p =>
            {
                p.Content = "";
                p.Embed = embed;
            });
        }

        public static void SetBasicHttpCredentials(this HttpClient client, string username, string password)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public static string HttpGet(this HttpClient client, string relativeUrl)
        {
            var response = client.GetAsync(relativeUrl).Result;
            var content = response.Content;
            return content.ReadAsStringAsync().Result;
        }

        public static string HttpPost(this HttpClient client, string relativeUrl, HttpContent httpContent)
        {
            var response = client.PostAsync(relativeUrl, httpContent).Result;
            var content = response.Content;
            return content.ReadAsStringAsync().Result;
        }

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static string FirstParagraph(this string value)
        {
            return value.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public static int ParagraphCount(this string value)
        {
            return value.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string ToRemainderString(this string[] array)
        {
            var output = "";

            foreach (var word in array)
            {
                output += word + " ";
            }

            output = output.Trim();

            return output;
        }

        public static IEnumerable<int> To(this int from, int to)
        {
            if (from < to)
            {
                while (from <= to)
                {
                    yield return from++;
                }
            }
            else
            {
                while (from >= to)
                {
                    yield return from--;
                }
            }
        }

        public static IEnumerable<T> InStepsOf<T>(this IEnumerable<T> source, int step)
        {
            if (step == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(step), "Parameter cannot be zero.");
            }

            return source.Where((x, i) => (i % step) == 0);
        }
    }
}
