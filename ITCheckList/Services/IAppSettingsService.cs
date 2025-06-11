using Newtonsoft.Json.Linq;

namespace ITCheckList.Services
{
    public interface IAppSettingsService
    {
        string GetConnectionString(string key);
        void UpdateConnectionString(string key, string newConnectionString);
    }

    public class AppSettingsService : IAppSettingsService
    {
        private readonly string _filePath;

        public AppSettingsService()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        }

        public string GetConnectionString(string key)
        {
            var json = System.IO.File.ReadAllText(_filePath);
            var jObject = JObject.Parse(json);
            return jObject["ConnectionStrings"]?[key]?.ToString() ?? "";
        }

        public void UpdateConnectionString(string key, string newConnectionString)
        {
            var json = System.IO.File.ReadAllText(_filePath);
            var jObject = JObject.Parse(json);

            jObject["ConnectionStrings"][key] = newConnectionString;

            System.IO.File.WriteAllText(_filePath, jObject.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }

}
