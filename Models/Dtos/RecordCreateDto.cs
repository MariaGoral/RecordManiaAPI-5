namespace RecordManiaAPI.Models.Dtos
{
    public class RecordCreateDto
    {
        public int StudentId { get; set; }
        public int LanguageId { get; set; }
        public long ExecutionTime { get; set; }

        public int? ProjectTaskId { get; set; }

        public string? ProjectTaskName { get; set; }
        public string? ProjectTaskDescription { get; set; }
    }

       
    }
