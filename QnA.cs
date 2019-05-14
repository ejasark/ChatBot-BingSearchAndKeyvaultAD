using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace EmptyBot1
{
    public class QnA
    {
        // Represents the various elements used to create HTTP request URIs
        // for QnA Maker operations.
        // From Publish Page: HOST
        // Example: https://YOUR-RESOURCE-NAME.azurewebsites.net/qnamaker
        string host = "https://qnaechobot1.azurewebsites.net/qnamaker";

        // Authorization endpoint key
        // From Publish Page
        string endpoint_key = "4142a337-bbea-498f-b25d-e00b6656c92d";

        // Management APIs postpend the version to the route
        // From Publish Page, value after POST
        // Example: /knowledgebases/ZZZ15f8c-d01b-4698-a2de-85b0dbf3358c/generateAnswer
        string route = "/knowledgebases/234c573b-843b-490b-be6f-149f88aa6cd7/generateAnswer";

        // JSON format for passing question to service
        string question = @"{'question': 'Why cant I see anything in the drop-down when I try to create a new knowledge base?'}";

        async public void generateAnswer(ITurnContext turnContext)
        {
            // Create http client
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // POST method
                request.Method = HttpMethod.Post;

                // Add host + service to get full URI
                request.RequestUri = new Uri(host + route);
                string qstn = turnContext.Activity.Text;
                question = @"{'question': '"+ qstn + "'}";
                // set question
                request.Content = new StringContent(question, Encoding.UTF8, "application/json");

                // set authorization
                request.Headers.Add("Authorization", "EndpointKey " + endpoint_key);

                // Send request to Azure service, get response
                var response = client.SendAsync(request).Result;
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    await turnContext.SendActivityAsync("Sorry, I don't have it's answer.");
                }
                else
                {
                    JObject json = JObject.Parse(jsonResponse.ToString());
                    await turnContext.SendActivityAsync(json["answers"][0]["answer"].ToString());
                }
            }
        } 
    }
}
