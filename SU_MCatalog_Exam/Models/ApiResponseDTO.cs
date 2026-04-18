using System.Text.Json.Serialization;


namespace SU_MCatalog_Exam.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO Movie { get; set; } = new MovieDTO();
    }
}
