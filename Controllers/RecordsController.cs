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
        public async Task<ActionResult<IEnumerable<object>>> GetRecords(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? languageId,
            [FromQuery] int? taskId)
        {
            var query = _context.Records
                .Include(r => r.Student)
                .Include(r => r.Language)
                .Include(r => r.ProjectTask)
                .AsQueryable();

            if (from.HasValue) query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(r => r.CreatedAt <= to.Value);
            if (languageId.HasValue) query = query.Where(r => r.LanguageId == languageId.Value);
            if (taskId.HasValue) query = query.Where(r => r.TaskId == taskId.Value);

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

            if (dto.TaskId.HasValue)
            {
                task = await _context.ProjectTasks.FindAsync(dto.TaskId.Value);
                if (task == null)
                    return NotFound($"Task with ID {dto.TaskId.Value} not found.");
            }
            else if (!string.IsNullOrWhiteSpace(dto.TaskName) && !string.IsNullOrWhiteSpace(dto.TaskDescription))
            {
                task = new ProjectTask { Name = dto.TaskName, Description = dto.TaskDescription };
                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Either TaskId or both TaskName and TaskDescription must be provided.");
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
