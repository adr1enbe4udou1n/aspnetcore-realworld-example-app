using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Conduit.Application.Features.Auth.Commands;

public class UpdateUserDto
{
    private string? _bio;
    private string? _email;
    private string? _image;
    private string? _password;
    private string? _username;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DisallowNull]
    public string? Username
    {
        get => _username;
        set
        {
            UsernameSpecified = true;
            _username = value;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DisallowNull]
    public string? Email
    {
        get => _email;
        set
        {
            EmailSpecified = true;
            _email = value;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [DisallowNull]
    public string? Password
    {
        get => _password;
        set
        {
            PasswordSpecified = true;
            _password = value;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Bio
    {
        get => _bio;
        set
        {
            BioSpecified = true;
            _bio = value;
        }
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Image
    {
        get => _image;
        set
        {
            ImageSpecified = true;
            _image = value;
        }
    }

    [JsonIgnore]
    public bool BioSpecified { get; private set; }

    [JsonIgnore]
    public bool EmailSpecified { get; private set; }

    [JsonIgnore]
    public bool ImageSpecified { get; private set; }

    [JsonIgnore]
    public bool PasswordSpecified { get; private set; }

    [JsonIgnore]
    public bool UsernameSpecified { get; private set; }
}
