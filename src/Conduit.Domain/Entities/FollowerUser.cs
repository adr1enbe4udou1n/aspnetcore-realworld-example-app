namespace Domain.Entities;

public class FollowerUser
{
    public int FollowingId { get; set; }
    public virtual User Following { get; set; } = null!;

    public int FollowerId { get; set; }
    public virtual User Follower { get; set; } = null!;
}