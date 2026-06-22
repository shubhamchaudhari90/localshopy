namespace localshopyNew.ViewModel
{
    public class SqlQueryViewModel
    {
        public string Query { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public List<Dictionary<string, object>> Results { get; set; } = new();
    }
}
