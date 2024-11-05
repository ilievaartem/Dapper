using DapperProj.Entities;
using DapperProj.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogMateSocialMedia2.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private IUnitOfWork _unitOfWork;

    public UserController(ILogger<UserController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("GetAllUsers")]
    public async Task<ActionResult<IEnumerable<Users>>> GetAllUsersAsync()
    {
        try
        {
            var results = await _unitOfWork._userRepository.GetAllAsync();
            _unitOfWork.Commit();
            _logger.LogInformation($"Reurned all users from database.");
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Transaction Failed! Something went wrong inside GetAllUsersAsync() action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet("GetById/{id}", Name = "GetUserById")]
    public async Task<ActionResult<IEnumerable<Users>>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _unitOfWork._userRepository.GetAsync(id);
            _unitOfWork.Commit();
            if (result == null)
            {
                _logger.LogError($"User with id: {id}, hasn't been found in db.");
                return NotFound();
            }
            else
            {
                _logger.LogInformation($"Returned user with id: {id}");
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside GetAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet("GetUsersWithComments")]
    public async Task<ActionResult<IEnumerable<Users>>> GetUsersWithComments()
    {
        try
        {
            var results = await _unitOfWork._userRepository.GetUsersWithComments();
            _unitOfWork.Commit();
            _logger.LogInformation($"Returned top five users from database.");
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Transaction Failed! Something went wrong inside GetUsersWithComments action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpPost("PostUser")]
    public async Task<ActionResult> PostUserAsync([FromBody] Users newUser)
    {
        try
        {
            if (newUser == null)
            {
                _logger.LogError("User object sent from client is null.");
                return BadRequest("User object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid model object");
            }

            newUser.CreatedAt = DateTime.UtcNow;

            var created_id = await _unitOfWork._userRepository.AddAsync(newUser);
            var CreatedUser = await _unitOfWork._userRepository.GetAsync(created_id);
            return CreatedAtRoute("GetUserById", new { id = created_id }, CreatedUser);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside PostUserAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }


    [HttpPut("Put/{id}")]
    public async Task<ActionResult> PutAsync(int id, [FromBody] Users updateUser)
    {
        try
        {
            if (updateUser == null)
            {
                _logger.LogError("User object sent from client is null.");
                return BadRequest("User object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid user object sent from client.");
                return BadRequest("Invalid user object");
            }

            var Users = await _unitOfWork._userRepository.GetAsync(id);
            if (Users == null)
            {
                _logger.LogError($"user with id: {id}, hasn't been found in db.");
                return NotFound();
            }
                
            updateUser.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork._userRepository.ReplaceAsync(updateUser);
            _unitOfWork.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside PutAsync action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete("Delete/{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        try
        {
            var users = await _unitOfWork._userRepository.GetAsync(id);
            if (users == null)
            {
                _logger.LogError($"User with id: {id}, hasn't been found in db.");
                return NotFound();
            }

            await _unitOfWork._userRepository.DeleteAsync(id);
            _unitOfWork.Commit();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong inside User action: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}