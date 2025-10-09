namespace FITON.Server.Models
{
    public class Avatar
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
