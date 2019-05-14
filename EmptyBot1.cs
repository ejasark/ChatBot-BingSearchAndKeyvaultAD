// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EmptyBot1
{
    public class EmptyBot : ActivityHandler
    {
        private BotState _conversationState;
        private BotState _userState;

        public EmptyBot()
        {
            ConversationState conversationState = new ConversationState(Startup.memoryStorage);
            UserState userState = new UserState(Startup.memoryStorage);
            _conversationState = conversationState;
            _userState = userState;
        }

        // Messages sent to the user.
        private const string WelcomeMessage = @"This is a simple Welcome Bot sample. This bot will introduce you
                                            to welcoming and greeting users. You can say 'intro' to see the
                                            introduction card. If you are running this bot in the Bot Framework
                                            Emulator, press the 'Start Over' button to simulate user joining
                                            a bot or a channel";

        private const string InfoMessage = @"You are seeing this message because the bot received at least one
                                        'ConversationUpdate' event, indicating you (and possibly others)
                                        joined the conversation. If you are using the emulator, pressing
                                        the 'Start Over' button to trigger this event again. The specifics
                                        of the 'ConversationUpdate' event depends on the channel. You can
                                        read more information at:
                                         https://aka.ms/about-botframework-welcome-user";

        private const string PatternMessage = @"It is a good pattern to use this event to send general greeting
                                          to user, explaining what your bot can do. In this example, the bot
                                          handles 'hello', 'hi', 'help' and 'intro'. Try it now, type 'hi'";

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync($"Hi there - {member.Name}. {WelcomeMessage}", cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(InfoMessage, cancellationToken: cancellationToken);
                    await turnContext.SendActivityAsync(PatternMessage, cancellationToken: cancellationToken);
                }
            }
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the state properties from the turn context.

            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationData());

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());
            var text = turnContext.Activity.Text.ToLowerInvariant();
            if (text.Equals("hi") || text.Equals("hello"))
            {
                await turnContext.SendActivityAsync($"Hello");
                return;
            }
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // First time around this is set to false, so we will prompt user for name.
                if (conversationData.PromptedUserForName)
                {
                    // Set the name to what the user provided.
                    userProfile.Name = turnContext.Activity.Text?.Trim();

                    // Acknowledge that we got their name.
                    await turnContext.SendActivityAsync($"Thanks {userProfile.Name}. To see conversation data, type anything.");

                    // Reset the flag to allow the bot to go though the cycle again.
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    // Prompt the user for their name.
                    await turnContext.SendActivityAsync($"What is your name?");

                    // Set the flag to true, so we don't prompt in the next turn.
                    conversationData.PromptedUserForName = true;
                }
            }
            else
            {
                KeywordSearch_webCrawler webCrawler = new KeywordSearch_webCrawler();
                webCrawler.webSearch(text, turnContext);

                // Add message details to the conversation data.
                // Convert saved Timestamp to local DateTimeOffset, then to string for display.
                /*var messageTimeOffset = (DateTimeOffset)turnContext.Activity.Timestamp;
                var localMessageTime = messageTimeOffset.ToLocalTime();
                conversationData.Timestamp = localMessageTime.ToString();
                conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();

                // Display state data.
                //await turnContext.SendActivityAsync($"{userProfile.Name} sent: {turnContext.Activity.Text}");
                //await turnContext.SendActivityAsync($"Message received at: {conversationData.Timestamp}");
                //await turnContext.SendActivityAsync($"Message received from: {conversationData.ChannelId}");

                QnA qa = new QnA();
                if (text.Contains("please") || text.Contains("help"))
                {
                    await turnContext.SendActivityAsync($"Sure, I will help you.", cancellationToken: cancellationToken);
                    //await SendIntroCardAsync(turnContext, cancellationToken);
                }
                else if (text.Contains("ok") || text.Contains("thank"))
                {
                    await turnContext.SendActivityAsync($"Welcome. Have a good day. Good Bye.", cancellationToken: cancellationToken);
                }
                else
                {
                    switch (text)
                    {
                        case "hello":
                        case "hi":
                            await turnContext.SendActivityAsync($"You said {text}.", cancellationToken: cancellationToken);
                            break;
                        case "intro":
                        case "help":
                            await SendIntroCardAsync(turnContext, cancellationToken);
                            break;
                        default:
                            qa.generateAnswer(turnContext);
                            //await turnContext.SendActivityAsync(WelcomeMessage, cancellationToken: cancellationToken);
                            break;
                    }
                }*/
            }
        }
        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var response = turnContext.Activity.CreateReply();

            var card = new HeroCard();
            card.Title = "Welcome to Bot Framework!";
            card.Text = @"Welcome to Welcome Users bot sample! This Introduction card
                 is a great way to introduce your Bot to the user and suggest
                 some things to get them started. We use this opportunity to
                 recommend a few next steps for learning more creating and deploying bots.";
            card.Images = new List<CardImage>() { new CardImage("https://aka.ms/bf-welcome-card-image") };
            card.Buttons = new List<CardAction>()
            {
                new CardAction(ActionTypes.OpenUrl, "Get an overview", null, "Get an overview", "Get an overview", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                new CardAction(ActionTypes.OpenUrl, "Ask a question", null, "Ask a question", "Ask a question", "https://stackoverflow.com/questions/tagged/botframework"),
                new CardAction(ActionTypes.OpenUrl, "Learn how to deploy", null, "Learn how to deploy", "Learn how to deploy", "https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-deploy-azure?view=azure-bot-service-4.0"),
            };

            response.Attachments = new List<Attachment>() { card.ToAttachment() };
            await turnContext.SendActivityAsync(response, cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
