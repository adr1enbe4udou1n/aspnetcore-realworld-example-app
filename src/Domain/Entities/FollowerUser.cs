using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class FollowerUser
    {
        [Required]
        public int FollowingId { get; set; }
        public User Following { get; set; }

        [Required]
        public int FollowerId { get; set; }
        public User Follower { get; set; }
    }
}