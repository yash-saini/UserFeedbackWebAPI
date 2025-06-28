namespace UserFeedbackWebAPI.Models
{
    public class FeedBack
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public int Rating { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
