#nullable disable warnings

using System.Text.Json.Serialization;
using LeanCode.Serialization;

namespace LeanCode.Kratos.Model;

public class RecoveryIdentityAddress
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("via")]
    public AddressType Via { get; set; }
}

public class VerifiableIdentityAddress
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("status")]
    public AddressStatus Status { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [JsonPropertyName("via")]
    public AddressType Via { get; set; }
}

[JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter<AddressType>))]
public enum AddressType
{
    Email,
    Phone,
}

[JsonConverter(typeof(JsonSnakeCaseLowerStringEnumConverter<AddressStatus>))]
public enum AddressStatus
{
    Pending,
    Sent,
    Completed,
}
