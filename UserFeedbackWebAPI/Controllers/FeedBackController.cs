using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserFeedbackWebAPI.Data;
using UserFeedbackWebAPI.Models;
using UserFeedbackWebAPI.Services;

namespace UserFeedbackWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedBackController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFeedbackService _service;

        public FeedBackController(IFeedbackService service)
        {
            _service = service;
        }

        // GET: api/feedback
        [HttpGet]
        public async Task<IActionResult> GetAllFeedback(
            [FromQuery] int? rating = null,
            [FromQuery] string? email = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var feedbackList = await _service.GetAllAsync(rating, email, pageNumber, pageSize);
            return Ok(feedbackList);
        }

        // GET: api/feedback/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid feedback ID.");
            }
            var feedback = await _service.GetByIdAsync(id);
            if (feedback == null)
            {
                return NotFound($"Feedback with ID {id} not found.");
            }
            return Ok(feedback);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeedback(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid feedback ID.");
            }
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound($"Feedback with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> SubmitFeedBack([FromBody] FeedBack feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback cannot be null.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdFeedback = await _service.CreateAsync(feedback);
                return CreatedAtAction(nameof(GetFeedbackById), new { id = createdFeedback.Id }, createdFeedback);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
