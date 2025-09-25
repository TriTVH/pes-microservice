using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Domain.DTOs
{
    public class ParentAccountDto
    {
        // nếu Auth service trả "id" nhưng bạn không cần, vẫn ok.
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("address")]
        public string Address { get; set; } = "";
    }
}
