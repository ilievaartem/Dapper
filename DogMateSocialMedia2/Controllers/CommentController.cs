using DapperProj.Entities;
using DapperProj.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DogMateSocialMedia2.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ILogger<CommentController> _logger;

    private ICommentRepository _commentRepository;

    public CommentController(ILogger<CommentController> logger, ICommentRepository commentRepository)
    {
        _logger = logger;
        _commentRepository = commentRepository;
    }

    [HttpGet("GetAllComments")]
    public async Task<ActionResult> GetAllCommentsAsync()
    {
        try
        {
            var results = await _commentRepository.GetAllCommentsAsync();
            _logger.LogInformation($"Reurned all comments from database.");
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Transaction Failed! Something went wrong inside GetAllCommentsAsync() action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet("GetById/{id}", Name = "GetCommentById")]
    public async Task<ActionResult> GetByIdAsync(int id)
    {
        try
        {
            var result = await _commentRepository.GetCommentByIdAsync(id);
            if (result == null)
            {
                _logger.LogError($"Comment with id: {id}, hasn't been found in db.");
                return NotFound();
            }
            else
            {
                _logger.LogInformation($"Returned comment with id: {id}");
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside GetAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPost("PostComment")]
    public async Task<ActionResult> PostCommentAsync([FromBody] Comments newComment)
    {
        try
        {
            if (newComment == null)
            {
                _logger.LogError("Comment object sent from client is null.");
                return BadRequest("Comment object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid Comment object sent from client.");
                return BadRequest("Invalid model object");
            }

            newComment.CreatedAt = DateTime.UtcNow;
            
            var created_id = await _commentRepository.AddCommentAsync(newComment.CommentText, newComment.UserId);
            var CreatedComment = await _commentRepository.GetCommentByIdAsync(created_id);
            return CreatedAtRoute("GetCommentById", new { id = created_id }, CreatedComment);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside PostCommentAsync action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("Put/{id}")]
    public async Task<ActionResult> PutCommentAsync(int id, [FromBody] Comments updateComment)
    {
        try
        {
            if (updateComment == null)
            {
                _logger.LogError("Comment object sent from client is null.");
                return BadRequest("Comment object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid comment object sent from client.");
                return BadRequest("Invalid comment object");
            }

            var Comments = await _commentRepository.GetCommentByIdAsync(id);
            if (Comments == null)
            {
                _logger.LogError($"comment with id: {id}, hasn't been found in db.");
                return NotFound();
            }

            updateComment.UpdatedAt = DateTime.UtcNow;
            
            await _commentRepository.UpdateCommentAsync(updateComment.Id, updateComment.CommentText);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside PutCommentAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("Delete/{id}")]
    public async Task<ActionResult> DeleteCommentAsync(int id)
    {
        try
        {
            var comments = await _commentRepository.GetCommentByIdAsync(id);
            if (comments == null)
            {
                _logger.LogError($"Comment with id: {id}, hasn't been found in db.");
                return NotFound();
            }

            await _commentRepository.DeleteCommentAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside DeleteCommentAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}