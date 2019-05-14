using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace EmptyBot1
{
    public class KeywordSearch_webCrawler
    {
        // Enter a valid subscription key.
        string accessKey = "";
        /*
         * If you encounter unexpected authorization errors, double-check this value
         * against the endpoint for your Bing Web search instance in your Azure
         * dashboard.
         */
        const string uriBase = "https://api.cognitive.microsoft.com/bing/v7.0/search";
        struct SearchResult
        {
            public String jsonResult;
            public Dictionary<String, String> relevantHeaders;
        }
        async public void webSearch(string searchTerm,ITurnContext turnContext)
        {
            accessKey = Startup.keyVaultInstance.Message;
            if (accessKey.Length == 32)
            {
                SearchResult result = BingWebSearch(searchTerm);
                //foreach (var header in result.relevantHeaders)
                    //await turnContext.SendActivityAsync(header.Key + ": " + header.Value);

                await turnContext.SendActivityAsync(JsonPrettyPrint(result.jsonResult));
            }
            else
            {
                await turnContext.SendActivityAsync("Invalid Bing Search API subscription key!");
            }
        }
        /// <summary>
        /// Performs a Bing Web search and return the results as a SearchResult.
        /// </summary>
        static SearchResult BingWebSearch(string searchQuery)
        {
            string key = Startup.keyVaultInstance.Message;
            // Construct the URI of the search request
            var uriQuery = uriBase + "?q=" + Uri.EscapeDataString(searchQuery);
            // Perform the Web request and get the response
            WebRequest request = HttpWebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = key;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            // Create result object for return
            var searchResult = new SearchResult()
            {
                jsonResult = json,
                relevantHeaders = new Dictionary<String, String>()
            };
            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }
            return searchResult;
        }
        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;
            json = json.Replace(Environment.NewLine, "").Replace("\t", "");
            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            char last = ' ';
            int offset = 0;
            int indentLength = 2;
            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\\':
                        if (quote && last != '\\') ignore = true;
                        break;
                }
                if (quote)
                {
                    sb.Append(ch);
                    if (last == '\\' && ignore) ignore = false;
                }
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (quote || ch != ' ') sb.Append(ch);
                            break;
                    }
                }
                last = ch;
            }
            return sb.ToString().Trim();
        }
    }
}
