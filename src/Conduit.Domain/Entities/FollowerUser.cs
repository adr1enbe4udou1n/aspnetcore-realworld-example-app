namespace Conduit.Domain.Entities;

public class FollowerUser
{
    public int FollowingId { get; set; }
    public virtual required User Following { get; set; }

    public int FollowerId { get; set; }
    public virtual required User Follower { get; set; }
}