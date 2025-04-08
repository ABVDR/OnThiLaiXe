using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using elFinder.NetCore;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/elfinder")]
    [Authorize(Roles = "Admin")]
    public class ElFinderController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ElFinderController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Route("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            return await connector.ProcessAsync(Request);
        }

        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        // Action mới để render giao diện elFinder
        [Route("file-manager")]
        public IActionResult FileManager()
        {
            return View();
        }

        private elFinder.NetCore.Connector GetConnector()
        {
            string pathRoot = Path.Combine(_env.WebRootPath, "files");
            var driver = new FileSystemDriver();
            driver.AddRoot(new RootVolume(pathRoot, "/files/", "/admin/elfinder/thumb/"));
            return new elFinder.NetCore.Connector(driver);
        }
    }
}