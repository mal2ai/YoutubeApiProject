using Microsoft.AspNetCore.Mvc;
using YouTubeApiProject.Services;
using YouTubeApiProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static YouTubeApiProject.Models.YouTubeVideoModel;

namespace YouTubeApiProject.Controllers
{
    public class YouTubeController : Controller
    {
        private readonly YouTubeApiService _youtubeService;

        public YouTubeController(YouTubeApiService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        // Display Search Page
        public IActionResult Index()
        {
            return View(new List<YouTubeVideoModel>()); // Pass an empty list initially
        }

        // Handle the search query
        [HttpPost]
        public async Task<IActionResult> Search(string query)
        {
            // Fetch videos including Author and ViewCount
            var videos = await _youtubeService.SearchVideosAsync(query);

            // Pass the search query to the view using ViewBag
            ViewBag.SearchTerm = query;

            // Return the view with the fetched videos
            return View("Index", videos); // Ensure "Index" view expects the model correctly
        }


        // Handle video playback
        public async Task<IActionResult> PlayVideo(string videoId)
        {
            ViewBag.VideoId = videoId;

            // Fetch additional video details (title, author info, description)
            var videoDetails = await _youtubeService.GetVideoDetailsAsync(videoId); // Fetch video details

            if (videoDetails == null)
            {
                // Handle the case where video details are not found (optional)
                return NotFound(); // or redirect to an error page
            }

            // Pass video details to the ViewBag
            ViewBag.VideoTitle = videoDetails.Title;
            ViewBag.AuthorProfilePhoto = videoDetails.AuthorProfilePhotoUrl; // Use the correct property name
            ViewBag.AuthorName = videoDetails.Author; // Use the correct property name
            ViewBag.AuthorSubscriberCount = FormatHelper.FormatSubscriberCount(videoDetails.AuthorSubscriberCount); // Format subscriber count
            ViewBag.VideoDescription = videoDetails.Description;
            ViewBag.PublishedAt = videoDetails.PublishedAt;

            // Format view count with commas
            ViewBag.ViewCount = $"{videoDetails.ViewCount:n0} views";

            return View(); // This will return PlayVideo.cshtml by default
        }

        // New action to display trending videos
        public async Task<IActionResult> Trending() // Change to async method
        {
            var trendingVideos = await _youtubeService.GetTrendingVideosAsync(); // Get trending videos

            return View(trendingVideos); // Pass the trending videos to the view
        }
    }
}
