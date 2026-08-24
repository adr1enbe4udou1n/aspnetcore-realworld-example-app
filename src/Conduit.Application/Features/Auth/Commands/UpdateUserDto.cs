using System.Text.Json.Serialization;

namespace Conduit.Application.Features.Auth.Commands;

public class UpdateUserDto
{
    private string? _bio;
    private string? _image;

    public string? Username { get; set; }
    public string? Email { get; set; }

    public string? Bio
    {
        get => _bio;
        set
        {
            BioSpecified = true;
            _bio = value;
        }
    }

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
    public bool ImageSpecified { get; private set; }
}
