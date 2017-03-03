using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;
using System.Net.Http;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using System.Web.Configuration;
using System.Configuration;

namespace TubeTalk
{
    [Serializable]
    public class MainDialog : IDialog<object>
    {

        static HttpClient client = new HttpClient();
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            // Put correct regex here 
            var regex = new Regex("^http");
            //var regex = new Regex(@"http(?:s?):\/\/(?:www\.)?youtu(?:be\.com\/watch\?v=|\.be\/)([\w\-\_]*)(&(amp;)?‌​[\w\?‌​=]*)?");
            if (regex.Match(message.Text).Success)
            {
                try
                {
                    string x = await CallYoutubeExtractorFunction(message.Text);
                    await context.PostAsync("Here's your video: " + x);
                    context.Wait(MessageReceivedAsync);
                }
                catch (Exception e)
                {
                    await context.PostAsync("Could not receive the video try a different link?");
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                Activity replyToConversation = context.MakeMessage() as Activity;
                replyToConversation.Attachments = new List<Attachment>();

                // Non link - query Youtube here
        
                var herocards = await GetYoutubeVideos(message.Text);
                
                foreach(var heroCard in herocards)
                {
                    Attachment plAttachment = heroCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                }

                await context.PostAsync(replyToConversation);

                //await context.PostAsync("Please paste a URL for the Youtube content you desire.");
                context.Wait(MessageReceivedAsync);
            }
        }

        //public static async Task<string> CallYoutubeExtractorFunction(string youtubeUrl)
        //{
        //    HttpRequestMessage msg = new HttpRequestMessage();
        //    msg.Content = new StringContent( "{\"link\":\""+youtubeUrl+"\"}");
        //    msg.Headers.Add("code", "rxS4U/QFYPlDunsoVI5dqaL9XAINP2e5vp685hDuixVcPpmBbuzaNA");
        //    string url = "https://yourtubefunctions.azurewebsites.net/api/YoutubeExtractorFunction?code=rxS4U/QFYPlDunsoVI5dqaL9XAINP2e5vp685hDuixVcPpmBbuzaNA==";

        //    HttpResponseMessage response = await client.PostAsync(new Uri(url), msg.Content);


        //    response.EnsureSuccessStatusCode();
        //    string responseBody = await response.Content.ReadAsStringAsync();

        //    return responseBody;
        //}

        public static async Task<string> CallYoutubeExtractorFunction(string youtubeUrl)
        {
            StringContent content = new StringContent("");
            string url = "https://yourtubefunctions.azurewebsites.net/api/YoutubeExtractorFunction?code=rxS4U/QFYPlDunsoVI5dqaL9XAINP2e5vp685hDuixVcPpmBbuzaNA==&link=";

            url = url + youtubeUrl;

            var response = await(new HttpClient().PostAsync(url, content));
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Use this method to return a list of hero cards to return to the bot for PostAsync
        /// Hero Cards will appear in a carosel and look pretty
        /// </summary>
        /// <param name="text"></param>
        private async Task<List<HeroCard>> GetYoutubeVideos(string text)
        {
            List<HeroCard> vidcards = new List<HeroCard>();


            string youtubePrefix = "https://www.youtube.com/watch?v=";

            // query youtube api
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()

            {

                ApiKey = WebConfigurationManager.AppSettings["YoutubeKey"],

                ApplicationName = "Searchler"

            });



            var searchListRequest = youtubeService.Search.List("snippet");

            searchListRequest.MaxResults = 3;

            // get back title of the video, thumbnail, desctiption, url

            searchListRequest.Q = text;

            var searchListResponse = await searchListRequest.ExecuteAsync();


            foreach (var searchResult in searchListResponse.Items)
            {

                var downloadURL = await CallYoutubeExtractorFunction(youtubePrefix + searchResult.Id.VideoId);

                downloadURL = downloadURL.Remove(0,13);

                HeroCard vidHeroCard = new HeroCard()
                {
                    Title = searchResult.Snippet.Title,
                    Subtitle = searchResult.Snippet.Description,
                    Images = new List<CardImage>()
                        {
                            new CardImage() { Url = searchResult.Snippet.Thumbnails.Medium.Url }
                        },
                    Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Open Download Link",
                                Type = ActionTypes.DownloadFile,
                                Value = downloadURL
                            }
                        }
                };
                vidcards.Add(vidHeroCard);
            }

            return vidcards;
        }
    }
}
    