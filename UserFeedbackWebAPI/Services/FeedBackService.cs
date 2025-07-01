using Microsoft.EntityFrameworkCore;
using UserFeedbackWebAPI.Data;
using UserFeedbackWebAPI.Models;

namespace UserFeedbackWebAPI.Services
{
    public interface IFeedbackService
    {
        Task<List<FeedBack>> GetAllAsync(int? rating, string? email, int page, int pageSize);
        Task<FeedBack?> GetByIdAsync(Guid id);
        Task<FeedBack> CreateAsync(FeedBack feedback);
        Task<bool> DeleteAsync(Guid id);
    }

    public class FeedBackService : IFeedbackService
    {
        private readonly AppDbContext _context;

        public FeedBackService( AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeedBack> CreateAsync(FeedBack feedback)
        {
            feedback.Id = Guid.NewGuid();
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return false;
            }
            _context.Feedbacks.Remove(feedback);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<FeedBack>> GetAllAsync(int? rating, string? email, int page, int pageSize)
        {
            var query = _context.Feedbacks.AsQueryable();
            if (rating.HasValue)
            {
                query = query.Where(f => f.Rating == rating.Value);
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(f => f.Email.Contains(email));
            }
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<FeedBack?> GetByIdAsync(Guid id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");
            }
            return feedback;
        }
    }
}
