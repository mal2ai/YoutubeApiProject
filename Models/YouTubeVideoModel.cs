namespace YouTubeApiProject.Models
{
    public class YouTubeVideoModel
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Author { get; set; }
        public string AuthorProfilePhotoUrl { get; set; }
        public long ViewCount { get; set; }
        public string AuthorSubscriberCount { get; set; }
        public string PublishedAt { get; set; }
        public string VideoUrl { get; set; }

        // New properties for channel information
        public string AuthorDescription { get; set; } // Add this for channel description
        public string AuthorChannelUrl { get; set; } // Add this for the channel URL

        // Existing methods remain unchanged...
        public string FormattedViewCount => FormatViewsCount(ViewCount);

        private static string FormatViewsCount(long viewCount)
        {
            if (viewCount >= 1000000)
                return (viewCount / 1000000D).ToString("0.#") + "M";
            if (viewCount >= 1000)
                return (viewCount / 1000D).ToString("0.#") + "k";
            return viewCount.ToString();
        }

        public static class FormatHelper
        {
            public static string FormatSubscriberCount(string count)
            {
                if (long.TryParse(count, out long subscriberCount))
                {
                    if (subscriberCount >= 1000000)
                        return $"{subscriberCount / 1000000.0:F1}M";
                    else if (subscriberCount >= 1000)
                        return $"{subscriberCount / 1000.0:F1}k";
                    else
                        return subscriberCount.ToString();
                }
                return "0";
            }
        }
    }
}
