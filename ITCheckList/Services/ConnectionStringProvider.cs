namespace ITCheckList.Services
{
    public class ConnectionStringProvider
    {
        private readonly IConfigurationRoot _configuration;

        public ConnectionStringProvider()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }
    }
}
