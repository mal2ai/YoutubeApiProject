using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using YouTubeApiProject.Models;

namespace YouTubeApiProject.Services
{
    public class YouTubeApiService
    {
        private readonly string _apiKey;
        private readonly YouTubeService _youtubeService;

        public YouTubeApiService(IConfiguration configuration)
        {
            _apiKey = configuration["YouTubeApiKey"]; // Fetch API key from appsettings.json
            _youtubeService = new YouTubeService(new BaseClientService.Initializer() // Initialize YouTubeService
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });
        }

        public async Task<List<YouTubeVideoModel>> SearchVideosAsync(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });

            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = query; // User's query from form input
            searchRequest.MaxResults = 10;

            try
            {
                var searchResponse = await searchRequest.ExecuteAsync();
                var videos = new List<YouTubeVideoModel>();

                foreach (var item in searchResponse.Items)
                {
                    var videoId = item.Id.VideoId;

                    // Retrieve video details to get view count, author details, etc.
                    var videoRequest = youtubeService.Videos.List("snippet,statistics");
                    videoRequest.Id = videoId;

                    try
                    {
                        var videoResponse = await videoRequest.ExecuteAsync();

                        if (videoResponse.Items.Count > 0)
                        {
                            var video = videoResponse.Items[0];

                            // Safely parse the view count
                            long viewCount = 0;
                            if (video.Statistics.ViewCount.HasValue)
                            {
                                viewCount = (long)video.Statistics.ViewCount.Value; // Convert safely
                            }

                            // Retrieve author details
                            var authorProfilePhotoUrl = video.Snippet.Thumbnails.Medium.Url; // Use thumbnail as author photo
                            var authorChannelId = video.Snippet.ChannelId;
                            var authorName = video.Snippet.ChannelTitle;

                            // Get subscriber count
                            var subscriberCount = await GetSubscriberCountAsync(youtubeService, authorChannelId);

                            videos.Add(new YouTubeVideoModel
                            {
                                VideoId = video.Id,
                                Title = item.Snippet.Title,
                                Description = item.Snippet.Description,
                                ThumbnailUrl = item.Snippet.Thumbnails.Medium.Url,
                                Author = authorName,
                                AuthorProfilePhotoUrl = authorProfilePhotoUrl,
                                ViewCount = viewCount, // Assign the parsed view count here
                                AuthorSubscriberCount = subscriberCount // Add subscriber count to model
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log or handle video request errors
                        Console.WriteLine($"Error retrieving video details for ID {videoId}: {ex.Message}");
                    }
                }

                return videos;
            }
            catch (Exception ex)
            {
                // Log or handle search request errors
                Console.WriteLine($"Error during YouTube search: {ex.Message}");
                return new List<YouTubeVideoModel>(); // Return an empty list on error
            }
        }

        private async Task<string> GetSubscriberCountAsync(YouTubeService youtubeService, string channelId)
        {
            var channelRequest = youtubeService.Channels.List("statistics");
            channelRequest.Id = channelId;

            try
            {
                var channelResponse = await channelRequest.ExecuteAsync();

                if (channelResponse.Items.Count > 0)
                {
                    var statistics = channelResponse.Items[0].Statistics;

                    // Convert SubscriberCount to string, default to "0" if null
                    return statistics.SubscriberCount?.ToString() ?? "0"; // Ensure this is formatted as a string
                }
            }
            catch (Exception ex)
            {
                // Log or handle channel request errors
                Console.WriteLine($"Error retrieving subscriber count for Channel ID {channelId}: {ex.Message}");
            }

            return "0"; // Default if no channel found or error occurs
        }

        public async Task<YouTubeVideoModel> GetVideoDetailsAsync(string videoId)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _apiKey,
                ApplicationName = "YoutubeProject"
            });

            // First, get the video details
            var videoRequest = youtubeService.Videos.List("snippet,statistics");
            videoRequest.Id = videoId;

            var videoResponse = await videoRequest.ExecuteAsync();
            var video = videoResponse.Items.FirstOrDefault();

            if (video == null)
            {
                return null; // or handle the case where the video is not found
            }

            // Get the channel ID
            var channelId = video.Snippet.ChannelId;

            // Now, get the channel details to retrieve the subscriber count
            var channelRequest = youtubeService.Channels.List("statistics");
            channelRequest.Id = channelId;

            var channelResponse = await channelRequest.ExecuteAsync();
            var channel = channelResponse.Items.FirstOrDefault();

            // Safely parse the subscriber count, defaulting to "0" if null
            string authorSubscriberCount = channel?.Statistics.SubscriberCount?.ToString() ?? "0";

            var publishedAt = video.Snippet.PublishedAt?.ToString("MMMM dd, yyyy");

            return new YouTubeVideoModel
            {
                VideoId = video.Id,
                Title = video.Snippet.Title,
                Description = video.Snippet.Description,
                ThumbnailUrl = video.Snippet.Thumbnails.Medium.Url,
                Author = video.Snippet.ChannelTitle,
                AuthorProfilePhotoUrl = video.Snippet.Thumbnails.Medium.Url,
                ViewCount = video.Statistics.ViewCount.HasValue ? (long)video.Statistics.ViewCount.Value : 0, // Convert ulong? to long safely
                AuthorSubscriberCount = authorSubscriberCount,
                PublishedAt = publishedAt
            };
        }

        // Your existing GetTrendingVideosAsync method
        public async Task<List<YouTubeVideoModel>> GetTrendingVideosAsync()
        {
            var request = _youtubeService.Videos.List("snippet,contentDetails,statistics");
            request.Chart = VideosResource.ListRequest.ChartEnum.MostPopular; // Use MostPopular for trending videos
            request.RegionCode = "US"; // Change to your desired region
            request.MaxResults = 12; // Specify how many results you want

            var response = await request.ExecuteAsync();

            var trendingVideos = new List<YouTubeVideoModel>();
            foreach (var item in response.Items)
            {
                // Safely handle ViewCount
                long viewCount = 0; // Default value
                if (item.Statistics != null && item.Statistics.ViewCount.HasValue)
                {
                    viewCount = (long)item.Statistics.ViewCount.Value; // Explicitly cast ulong to long
                }

                // Safely handle PublishedAt
                string publishedAt = item.Snippet.PublishedAt?.ToString("yyyy-MM-dd") ?? "N/A"; // Default to "N/A"

                // Get channel details to retrieve the author's profile photo
                var channelRequest = _youtubeService.Channels.List("snippet");
                channelRequest.Id = item.Snippet.ChannelId; // Set the channel ID
                var channelResponse = await channelRequest.ExecuteAsync();

                // Retrieve the author's profile photo URL
                string authorProfilePhotoUrl = "";
                if (channelResponse.Items.Count > 0)
                {
                    authorProfilePhotoUrl = channelResponse.Items[0].Snippet.Thumbnails.Default__.Url; // Use default thumbnail
                }

                trendingVideos.Add(new YouTubeVideoModel
                {
                    VideoId = item.Id,
                    Title = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    ThumbnailUrl = item.Snippet.Thumbnails.High.Url,
                    Author = item.Snippet.ChannelTitle,
                    AuthorProfilePhotoUrl = authorProfilePhotoUrl, // Assign the channel thumbnail URL
                    AuthorSubscriberCount = "", // Fetch this if needed
                    ViewCount = viewCount, // Assign the safely parsed view count
                    PublishedAt = publishedAt // Assign the safely formatted publish date
                });
            }

            return trendingVideos; // Return the list of trending videos
        }


    }
}