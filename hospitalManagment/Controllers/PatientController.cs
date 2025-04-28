using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/patient")]
public class PatientController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;

    public PatientController(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddPatient([FromBody] Patient Patient)
    {
        if (Patient == null)
        {
            return BadRequest(new { message = "Invalid patient data" });
        }
        await _mongoDbService.AddPatientAsync(Patient);
        return Ok(new { message = "Patient added successfully" });
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetPatients()
    {
        try
        {

            var patients = await _mongoDbService.GetPatientsAsync();
        
            return Ok(patients); 
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpGet("getSortedPatients")]
    public async Task<IActionResult> GetSortedPatients(
    [FromQuery] string? searchName = null,
    [FromQuery] string? room = null,
    [FromQuery] string? floor = null, 
    [FromQuery] string? block = null,
    [FromQuery] string? condition = null,
    [FromQuery] string? gender = null,
    [FromQuery] int? minAge = null,
    [FromQuery] int? maxAge = null,
    [FromQuery] string sortBy = "Name",
    [FromQuery] string order = "asc")
    {
        var patients = await _mongoDbService.GetSortedPatientsAsync(searchName, room, condition, gender, minAge, maxAge, sortBy, order);
        return Ok(patients);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeletePatient(string id)
    {
        var result = await _mongoDbService.DeletePatientAsync(id);

        if (!result)
            return NotFound("Patient not found.");

        return Ok(new { message = "Patient discharged successfully." });
    }



}
