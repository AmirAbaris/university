namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessorController : ControllerBase
{
    const string _collectionName = "professors";
    private readonly IMongoCollection<Professor> _collection;

    // constructor - dependency injections
    public ProfessorController(IMongoClient client, IMongoDbSettings dbSettings)
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Professor>(_collectionName);
    }

    [HttpPost("register-professor")]
    public async Task<ActionResult<Professor>> Create([FromBody] Professor request, [FromQuery] string department)
    {
        try
        {
            // Validate user payload
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);

                return BadRequest(new { Message = "User registration failed", Errors = errors });
            }

            // Check if student already exists
            var existingProf = await _collection.FindAsync<Professor>(prof => prof.Email == request.Email.ToLower().Trim());

            if (await existingProf.AnyAsync())
            {
                return Conflict("User already exists");
            }

            // Create a Professor
            Professor newProf = new Professor(
                Id: null,
                Name: request.Name.ToLower().Trim(),
                Email: request.Email.ToLower().Trim(),
                Password: request.Password,
                Department: Enum.Parse<Department>(department)
            );

            // Insert Created User into Database
            await _collection.InsertOneAsync(newProf);

            // return ok satus for inserted prof
            return Ok(newProf);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine(ex.ToString());

            // Return a 500 Internal Server Error response with an informative error message
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
        }
    }

    [HttpGet("get-professors")]
    public async Task<ActionResult<List<Professor>>> GetAll(int pageNumber = 1, int pageSize = 10, string orderBy = "Name")
    {
        var skip = (pageNumber - 1) * pageSize;
        var filter = new BsonDocument();
        var sort = Builders<Professor>.Sort.Ascending(orderBy);

        List<Professor> professors = await _collection.Find<Professor>(filter).Skip(skip).Limit(pageSize).Sort(sort).ToListAsync();

        if (!professors.Any())
        {
            return NotFound("No professors found");
        }

        return Ok(professors);
    }

    [HttpGet("get-professor/{id}")]
    public async Task<ActionResult<Professor>> Get(string id)
    {
        // Find professor by id
        var professor = await _collection.Find<Professor>(prof => prof.Id == id).FirstOrDefaultAsync();

        // Check if professor was not found
        if (professor == null)
        {
            return NotFound("professor not found");
        }

        return Ok(professor);
    }

    [HttpPut("update-professor/{profId}")]
    public async Task<ActionResult<UpdateResult>> UpdateById(string profId, Professor profIn)
    {
        // Define an update operation to set the email and password fields for the object
        var updatedProf = Builders<Professor>.Update
        .Set(p => p.Name, profIn.Name.ToLower().Trim())
        .Set(p => p.Email, profIn.Email.ToLower().Trim())
        .Set(u => u.Password, profIn.Password)
        .Set(p => p.Department, profIn.Department);

        // Update the specified professor in the database by its ID using the update operation
        UpdateResult result = await _collection.UpdateOneAsync<Professor>(st => st.Id == profId, updatedProf);

        // Return a NoContent response indicating that the update was successful
        return NoContent();
    }

    [HttpDelete("delete-professor/{id}")]
    public async Task<DeleteResult> Delete(string id)
    {
        DeleteResult result = await _collection.DeleteOneAsync(Builders<Professor>.Filter.Eq(prof => prof.Id, id));

        return result;
    }
}
