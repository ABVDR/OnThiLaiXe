using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using elFinder.NetCore;

namespace OnThiLaiXe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/elfinder")]//Dih tuyen path
    [Authorize(Roles = "Admin")]
    public class ElFinderController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ElFinderController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Route("connector")]
        public async Task<IActionResult> Connector()//xu ly yeu cau elfinder
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

        [Route("file-manager")]
        public IActionResult FileManager()
        {
            return View();
        }

        private elFinder.NetCore.Connector GetConnector()
        {
            string pathRoot = Path.Combine(_env.WebRootPath, "files");
            //cau hinh dẻ quan ly file trong thu muc cụ the
            var driver = new FileSystemDriver();
            driver.AddRoot(new RootVolume(pathRoot, "/files/", "/admin/elfinder/thumb/"));
            return new elFinder.NetCore.Connector(driver);
        }
    }
}