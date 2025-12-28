namespace localshopyNew.Services
{
    public class EncodingService : IEncodingService
    {
        private readonly string _key;

        public EncodingService(IConfiguration configuration)
        {
            _key = configuration["EncodingKey"]!;
        }

        public string Encode(string value)
        {
            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(_key);

            for (int i = 0; i < valueBytes.Length; i++)
            {
                valueBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return Convert.ToBase64String(valueBytes);
        }

        public string Decode(string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(_key);

            for (int i = 0; i < valueBytes.Length; i++)
            {
                valueBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return System.Text.Encoding.UTF8.GetString(valueBytes);
        }
    }
}
