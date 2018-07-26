using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Blogger2Ghost
{
    public class FileHelper
    {
        protected static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                DefaultContractResolver contractResolver = new DefaultContractResolver();
                contractResolver.NamingStrategy = new SnakeCaseNamingStrategy();
                return new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = contractResolver
                };
            }
        }

        public static void WriteFile(string location, string name, object data, bool overwrite)
        {
            string path = Path.Combine(location, name + ".json");
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            using (var output = File.Open(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(output, utf8WithoutBom))
            {
                writer.Write(JsonConvert.SerializeObject(data, JsonSerializerSettings));
                Console.WriteLine("Written : " + path);
            }
        }

        public static T[] ReadFile<T>(string location, string name)
        {
            string path = Path.Combine(location, name + ".json");
            using (var input = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {
                Console.WriteLine("Reading : " + path);
                return JsonConvert.DeserializeObject<T[]>(reader.ReadToEnd(), JsonSerializerSettings);
            }
        }
    }
}