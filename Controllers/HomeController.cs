using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Youtube.Models;
using YouTubeApiProject.Services; // Ensure this using directive is present

namespace Youtube.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly YouTubeApiService _youtubeService; // Add this field

        public HomeController(ILogger<HomeController> logger, YouTubeApiService youtubeService) // Inject the service
        {
            _logger = logger;
            _youtubeService = youtubeService; // Initialize the service
        }

        public async Task<IActionResult> Index() // Change to async method
        {
            var trendingVideos = await _youtubeService.GetTrendingVideosAsync(); // Get trending videos

            return View(trendingVideos); // Pass the trending videos to the view
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
