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
        public IActionResult GetAllFeedback()
        {
            var feedbackList = _context.Feedbacks.ToList();
            return Ok(feedbackList);
        }

        // GET: api/feedback/{id}
        [HttpGet("{id}")]
        public IActionResult GetFeedbackById(int id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found.");
            }
            return Ok(feedback);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteFeedback(int id)
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
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetFeedbackById), new { id = feedback.Id }, feedback);
        }
    }
}
