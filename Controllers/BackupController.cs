using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = "Admin")]

    public class BackupController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public BackupController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet]
        public IActionResult DownloadBackup()
        {
            var tempPath = Path.Combine(Path.GetTempPath(),
                $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

            using (var zip = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                // App_Data
                var appDataPath = Path.Combine(_env.ContentRootPath, "App_Data");
                if (Directory.Exists(appDataPath))
                {
                    AddFolderToZip(zip, appDataPath, "App_Data");
                }

                // wwwroot
                var wwwrootPath = _env.WebRootPath;
                if (Directory.Exists(wwwrootPath))
                {
                    AddFolderToZip(zip, wwwrootPath, "wwwroot");
                }
            }

            var fileBytes = System.IO.File.ReadAllBytes(tempPath);
            System.IO.File.Delete(tempPath);

            return File(fileBytes, "application/zip", $"Localshopy_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
        }

        private void AddFolderToZip(ZipArchive zip, string sourceDir, string entryName)
        {
            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                zip.CreateEntryFromFile(file, Path.Combine(entryName, relativePath));
            }
        }
    }
}
