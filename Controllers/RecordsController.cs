using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecordManiaAPI.Data;
using RecordManiaAPI.Models;
using RecordManiaAPI.Models.Dtos;

namespace RecordManiaAPI.Controllers
{
    [ApiController]
    [Route("records")]
    public class RecordsController : ControllerBase
    {
        private readonly RecordManiaDbContext _context;

        public RecordsController(RecordManiaDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<object>>> GetRecords(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? languageId,
            [FromQuery] int? projectTaskId)
        {
            var query = _context.Records
                .Include(r => r.Student)
                .Include(r => r.Language)
                .Include(r => r.ProjectTask)
                .AsQueryable();

            if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value);
            if (languageId.HasValue) query = query.Where(r => r.LanguageId == languageId.Value);
            if (projectTaskId.HasValue) query = query.Where(r => r.TaskId == projectTaskId.Value);

            var result = await query
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.Student.LastName)
                .Select(r => new
                {
                    id = r.Id,
                    createdAt = r.CreatedAt,
                    student = $"{r.Student.FirstName} {r.Student.LastName}",
                    language = r.Language.Name,
                    taskName = r.ProjectTask.Name,
                    executionTime = r.ExecutionTime
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddRecord([FromBody] RecordCreateDto dto)
        {
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
                return NotFound($"Student with ID {dto.StudentId} not found.");

            var language = await _context.Languages.FindAsync(dto.LanguageId);
            if (language == null)
                return NotFound($"Language with ID {dto.LanguageId} not found.");

            // Klasa Task została przemianowana na ProjectTask,
            // aby uniknąć konfliktu z System.Threading.Tasks.Task
            ProjectTask task;

            if (dto.ProjectTaskId.HasValue)
            {
                task = await _context.ProjectTasks.FindAsync(dto.ProjectTaskId.Value);
                if (task == null)
                    return NotFound($"ProjectTask with ID {dto.ProjectTaskId.Value} not found.");
            }
            else if (!string.IsNullOrWhiteSpace(dto.ProjectTaskName) && !string.IsNullOrWhiteSpace(dto.ProjectTaskDescription))
            {
                task = new ProjectTask { Name = dto.ProjectTaskName, Description = dto.ProjectTaskDescription };
                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Either ProjectTaskId or both ProjectTaskName and ProjectTaskDescription must be provided.");
            }

            var record = new Record
            {
                CreatedAt = DateTime.UtcNow,
                StudentId = dto.StudentId,
                LanguageId = dto.LanguageId,
                TaskId = task.Id,
                ExecutionTime = dto.ExecutionTime
            };

            _context.Records.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecords), new { id = record.Id }, record);
        }
    }
    
}
