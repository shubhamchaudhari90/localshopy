using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace localshopyNew.Controllers
{
    public class SqlController : Controller
    {
        private readonly IConfiguration _configuration;

        public SqlController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SqlQueryViewModel
            {
                Query = @"  SELECT name
                            FROM sqlite_master
                            WHERE type = 'table'
                            ORDER BY name;"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(SqlQueryViewModel model)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = model.Query.Trim();

                if (model.Query.TrimStart().StartsWith("SELECT",
                    StringComparison.OrdinalIgnoreCase))
                {
                    using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.IsDBNull(i)
                                ? ""
                                : reader.GetValue(i);
                        }

                        model.Results.Add(row);
                    }

                    model.Message = $"{model.Results.Count} row(s) returned.";
                }
                else
                {
                    int affectedRows = await command.ExecuteNonQueryAsync();
                    model.Message = $"{affectedRows} row(s) affected.";
                }
            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
            }

            return View(model);
        }
    }
}
