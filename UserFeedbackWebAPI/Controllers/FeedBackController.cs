using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserFeedbackWebAPI.Data;
using UserFeedbackWebAPI.Models;

namespace UserFeedbackWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedBackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedBackController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/feedback
        [HttpGet]
        public IActionResult GetAllFeedback(
            [FromQuery] int? rating = null,
            [FromQuery] string? email = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
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
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var feedbacks = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Ok(new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Feedbacks = feedbacks
            });
        }

        // GET: api/feedback/{id}
        [HttpGet("{id}")]
        public IActionResult GetFeedbackById(Guid id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found.");
            }
            return Ok(feedback);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteFeedback(Guid id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found.");
            }
            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpPost]
        [Authorize]
        public IActionResult SubmitFeedBack([FromBody] FeedBack feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback cannot be null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            feedback.Id = Guid.NewGuid();
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
        }
    }
}
