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

        // New property to get the formatted view count
        public string FormattedViewCount
        {
            get { return FormatViewsCount(ViewCount); }
        }

        // Static method to format the view count
        private static string FormatViewsCount(long viewCount)
        {
            if (viewCount >= 1000000)
                return (viewCount / 1000000D).ToString("0.#") + "M"; // Format as "1M"
            if (viewCount >= 1000)
                return (viewCount / 1000D).ToString("0.#") + "k";    // Format as "1k"
            return viewCount.ToString(); // Return plain if below 1000
        }

        public static class FormatHelper
        {
            public static string FormatSubscriberCount(string count)
            {
                if (long.TryParse(count, out long subscriberCount))
                {
                    if (subscriberCount >= 1000000)
                        return $"{subscriberCount / 1000000.0:F1}M"; // e.g., 1.5M for 1,500,000
                    else if (subscriberCount >= 1000)
                        return $"{subscriberCount / 1000.0:F1}k"; // e.g., 1.5k for 1,500
                    else
                        return subscriberCount.ToString(); // Less than 1000
                }
                return "0"; // Fallback if parsing fails
            }
        }
    }
}
