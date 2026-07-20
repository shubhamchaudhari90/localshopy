using localshopyNew.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace localshopyNew.Controllers
{
    public class BackupController : Controller
    {

        private readonly SqliteBackupService _backupService;

        public BackupController(SqliteBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpGet]
        public IActionResult DownloadSqlBackup()
        {
            string sql = _backupService.GenerateBackupScript();
            byte[] fileBytes = Encoding.UTF8.GetBytes(sql);
            string fileName = $"LocalShopy_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
            return File(fileBytes, "application/sql", fileName);
        }
    }
}
