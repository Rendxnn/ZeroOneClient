using System.Text.Json.Serialization;

namespace ZeroOneClient.DTOs
{
    public class ZeroOneModels
    {
        public record LoginRequest
        {
            [JsonPropertyName("email")]
            public string Email { get; set; } = null!;

            [JsonPropertyName("password")]
            public string Password { get; set; } = null!;

            [JsonPropertyName("empresaId")]
            public string CompanyId { get; set; } = null!;
        }

        public record LoginResponse
        {
            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("token")]
            public string? Token { get; set; }

            [JsonPropertyName("userName")]
            public string? Username { get; set; }
        }

        public record Activity
        {
            [JsonPropertyName("ActividadId")]
            public string ActivityId { get; set; } = null!;

            [JsonPropertyName("Nombre")]
            public string Name { get; set; } = null!;
        }

        public record Client
        {
            [JsonPropertyName("ClienteId")]
            public string ClientId { get; set; } = null!;

            [JsonPropertyName("Nombre")]
            public string Name { get; set; } = null!;
        }

        public record ListsResponse
        {
            [JsonPropertyName("Actividades")]
            public List<Activity> Activities { get; set; } = [];

            [JsonPropertyName("Clientes")]
            public List<Client> Clients { get; set; } = [];

            [JsonPropertyName("Proyectos")]
            public List<Project> Projects { get; set; } = [];
        }

        public class Project
        {
            [JsonPropertyName("ProyectoId")]
            public string ProjectId { get; set; } = null!;

            [JsonPropertyName("Nombre")]
            public string Name { get; set; } = null!;

            [JsonPropertyName("ClienteId")]
            public string ClientId { get; set; } = null!;
        }
    }
}
