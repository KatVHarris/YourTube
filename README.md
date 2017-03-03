# YourTube
Yourtube is a bot, built with the Microsoft Bot Framework and returns direct links to youtube content. Users can type a search terms or paste a direct link from your video. You can interact with yourtube at http://yourtubebot.azurewebsites.net

## Contributors
Kat Harris - @KatVHarris | Paul DeCarlo - @pjdecarlo | Jessica Deen - @jldeen

## Settings
Before running you need to add your keys for the Microsoft Bot Framework in Web.config.
  * Microsoft App ID
  * Microsoft Password
  
You also need to get a youtube developer key to add to Web.config
  * https://developers.google.com/youtube/android/player/register
  
## Commands
The majority of the code is in MainDialog.cs 
"Hi" and "Help" return 
* I can give you direct links to Youtube content! Paste a link to a Youtube video or type a search term to find a video =) 
