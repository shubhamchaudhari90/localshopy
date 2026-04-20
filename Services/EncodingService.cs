using localshopyNew.Services.Interfaces;

namespace localshopyNew.Services
{
    public class EncodingService : IEncodingService
    {
        private readonly IConfiguration _configuration;

        public EncodingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Encode(string value)
        {
            string key = _configuration["EncodingKey"] ?? "ComplexKey";

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < valueBytes.Length; i++)
            {
                valueBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return Convert.ToBase64String(valueBytes);
        }

        public string Decode(string value)
        {
            string key = _configuration["EncodingKey"] ?? "ComplexKey";
            var valueBytes = Convert.FromBase64String(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < valueBytes.Length; i++)
            {
                valueBytes[i] ^= keyBytes[i % keyBytes.Length];
            }
            return System.Text.Encoding.UTF8.GetString(valueBytes);
        }


    }
}
