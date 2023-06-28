#nullable disable warnings

using System.Text.Json;
using System.Text.Json.Serialization;
using LeanCode.Serialization;

namespace LeanCode.Kratos.Model;

[System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA2227")]
public class Identity
{
    [JsonPropertyName("state")]
    public IdentityState State { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("recovery_addresses")]
    public List<RecoveryIdentityAddress>? RecoveryAddresses { get; set; }

    [JsonPropertyName("schema_id")]
    public string SchemaId { get; set; }

    [JsonPropertyName("schema_url")]
    public Uri SchemaUrl { get; set; }

    [JsonPropertyName("state_changed_at")]
    public DateTime? StateChangedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("verifiable_addresses")]
    public List<VerifiableIdentityAddress>? VerifiableAddresses { get; set; }

    [JsonPropertyName("metadata_admin")]
    public JsonElement? MetadataAdmin { get; set; }

    [JsonPropertyName("metadata_public")]
    public JsonElement? MetadataPublic { get; set; }

    [JsonPropertyName("traits")]
    public JsonElement Traits { get; set; }
}

[JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter))]
public enum IdentityState
{
    Active,
    Inactive,
}
