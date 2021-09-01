using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class AuthorFollower
    {
        [Required]
        public int AuthorId { get; set; }
        public User Author { get; set; }

        [Required]
        public int FollowerId { get; set; }
        public User Follower { get; set; }
    }
}