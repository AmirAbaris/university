namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    const string _collectionName = "students";
    private readonly IMongoCollection<Student> _collection;

    // constructor - dependency injections
    public StudentController(IMongoClient client, IMongoDbSettings dbSettings)
    {
        var dbName = client.GetDatabase(dbSettings.DatabaseName);
        _collection = dbName.GetCollection<Student>(_collectionName);
    }

    [HttpPost("register-student")]
    public async Task<ActionResult<Student>> Create([FromBody] Student request)
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
            var existingStudent = await _collection.FindAsync<Student>(st => st.Email == request.Email.ToLower().Trim());

            if (await existingStudent.AnyAsync())
            {
                return Conflict("User already exists");
            }

            // Create a student
            Student newStudent = new Student(
                Id: null,
                Name: request.Name.ToLower().Trim(),
                Age: request.Age,
                Email: request.Email.ToLower().Trim(),
                Password: request.Password
            );

            // Insert Created Student into Database
            await _collection.InsertOneAsync(newStudent);

            // return ok satus for inserted user
            return Ok(newStudent);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine(ex.ToString());

            // Return a 500 Internal Server Error response with an informative error message
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing your request." });
        }
    }

    [HttpGet("get-students")]
    public async Task<ActionResult<List<Student>>> GetAll(int pageNumber = 1, int pageSize = 10, string orderBy = "Name")
    {
        var skip = (pageNumber - 1) * pageSize;
        var filter = new BsonDocument();
        var sort = Builders<Student>.Sort.Ascending(orderBy);

        List<Student> students = await _collection.Find<Student>(filter).Skip(skip).Limit(pageSize).Sort(sort).ToListAsync();

        if (!students.Any())
        {
            return NotFound("No students found");
        }

        return Ok(students);
    }

    [HttpGet("get-student/{id}")]
    public async Task<ActionResult<Student>> Get(string id)
    {
        // Find student by id
        var student = await _collection.Find<Student>(st => st.Id == id).FirstOrDefaultAsync();

        // Check if student was not found
        if (student == null)
        {
            return NotFound("Student not found");
        }

        return Ok(student);
    }

    [HttpPut("update-student/{studentId}")]
    public async Task<ActionResult<UpdateResult>> UpdateById(string studentId, Student studentIn)
    {
        // Define an update operation to set the email and password fields for the object
        var updatedStudent = Builders<Student>.Update
        .Set(s => s.Name, studentIn.Name.ToLower().Trim())
        .Set(s => s.Age, studentIn.Age)
        .Set(s => s.Email, studentIn.Email.ToLower().Trim())
        .Set(s => s.Password, studentIn.Password);

        // Update the specified student in the database by its ID using the update operation
        await _collection.UpdateOneAsync<Student>(st => st.Id == studentId, updatedStudent);

        // Return a NoContent response indicating that the update was successful
        return NoContent();
    }

    [HttpDelete("delete-student/{id}")]
    public async Task<DeleteResult> Delete(string id)
    {
        DeleteResult result = await _collection.DeleteOneAsync(Builders<Student>.Filter.Eq(st => st.Id, id));

        return result;
    }
}
