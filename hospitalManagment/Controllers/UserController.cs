using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;
    private readonly IMongoCollection<User> _usersCollection;
    private readonly ILogger<UserController> _logger;



    public UserController(MongoDbService mongoDbService, ILogger<UserController> logger)
    {
        _mongoDbService = mongoDbService;
        _usersCollection = _mongoDbService.GetUserCollection();
        _logger = logger;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddUser([FromBody] User User)
    {
        try
        {
            if (User == null)
                return BadRequest(new { message = "Invalid patient data." });

            await _mongoDbService.AddUserAsync(User);
            return Ok(new { message = "USer added successfully." });

        }
        catch (Exception ex)
        {

            return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
        }
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _mongoDbService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    // ✅ Update Exit Time API
    [HttpPut("updateExitTime")]
    public async Task<IActionResult> UpdateExitTime([FromBody] ExitTimeUpdateModel model)
    {
        try
        {
            if (model == null || string.IsNullOrEmpty(model.VisitId) || string.IsNullOrEmpty(model.ExitTime))
            {
                return BadRequest(new { message = "Invalid request data." });
            }
            await _mongoDbService.UpdateExitTimeAsync(model.VisitId, model.ExitTime);
            return Ok(new { message = "Exit time updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
        }
    }

    [HttpGet("getPending")]
    public async Task<IActionResult> GetPendingVisits([FromQuery] string fullName)
    {
        try
        {
            if (string.IsNullOrEmpty(fullName))
                return BadRequest(new { message = "User name is required." });

            var pendingVisits = await _mongoDbService.GetPendingVisitsAsync(fullName);
            return Ok(pendingVisits);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
        }


    }
    [HttpGet("getSortedUsers")]
    public async Task<IActionResult> GetSortedUsers(
    [FromQuery] string? searchName = null,
    [FromQuery] string? exitTimeStatus = null,
    [FromQuery] string? floor = null,
    [FromQuery] string? room = null,
    [FromQuery] string? sortBy = "entryTime",
    [FromQuery] string? order = "asc")
    {

        _logger.LogInformation($"Received Params - Search: {searchName}, ExitTimeStatus: {exitTimeStatus}, Floor: {floor}, Room: {room}, SortBy: {sortBy}, Order: {order}");


        var filter = Builders<User>.Filter.Empty;

        if (!string.IsNullOrEmpty(searchName))
        {
            filter &= Builders<User>.Filter.Regex("fullName", new BsonRegularExpression(searchName, "i"));
        }

        if (!string.IsNullOrEmpty(floor))
        {
            filter &= Builders<User>.Filter.Regex("floor", new BsonRegularExpression(floor, "i"));
        }

        if (!string.IsNullOrEmpty(room))
        {
            filter &= Builders<User>.Filter.Regex("room", new BsonRegularExpression(room, "i"));
        }


        if (exitTimeStatus == "pending")
        {
            filter &= Builders<User>.Filter.Eq<DateTime?>("exitTime", null);
        }
        else if (exitTimeStatus == "completed")
        {
            filter &= Builders<User>.Filter.Ne<DateTime?>("exitTime", null);
        }

        var sortDefinition = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase)
            ? Builders<User>.Sort.Descending(sortBy)
            : Builders<User>.Sort.Ascending(sortBy);

        var users = await _usersCollection.Find(filter).Sort(sortDefinition).ToListAsync();

        return Ok(users);
    }


    [HttpPost("submit-feedback/{userId}")]
    public async Task<IActionResult> SubmitFeedback(string userId, [FromBody] Feedback feedback)
    {
        if ((feedback.StarRating == null || feedback.StarRating < 1 || feedback.StarRating > 5)
            && string.IsNullOrWhiteSpace(feedback.Message))
        {
            return BadRequest("At least a star rating (1-5) or a message is required.");
        }

        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Set(u => u.Feedback, feedback);
        var result = await _usersCollection.UpdateOneAsync(filter, update);

        if (result.ModifiedCount == 0)
            return NotFound("User not found.");

        return Ok("Feedback submitted successfully.");
    }

    [HttpGet("user-details/{userId}")]
    public async Task<IActionResult> GetUserDetails(string userId)
    {
        var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return NotFound("User not found.");

        return Ok(user); // Includes Feedback
    }


    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var result = await _usersCollection.DeleteOneAsync(filter);

        if (result.DeletedCount == 0)
            return NotFound("User not found.");

        return Ok("User deleted successfully.");
    }




}















