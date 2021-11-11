namespace Domain.Entities;

public class FollowerUser
{
    public int FollowingId { get; set; }
    public User Following { get; set; } = null!;

    public int FollowerId { get; set; }
    public User Follower { get; set; } = null!;
}