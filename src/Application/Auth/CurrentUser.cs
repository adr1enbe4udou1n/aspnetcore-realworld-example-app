namespace Application.Auth
{
    public class CurrentUser
    {
        public string Email { get; set; }

        public string Username { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }

        public string Token { get; set; }
    }

    public record UserEnvelope(CurrentUser User);
}