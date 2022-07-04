using System.Text.Json.Serialization;

namespace IntegrationTestExample.Web.Models
{
    public class GitHubRepository
    {
        public string Name { get; set; }

        public string Language { get; set; }

        [JsonPropertyName("stargazers_count")]
        public int Stars { get; set; }
    }
}
