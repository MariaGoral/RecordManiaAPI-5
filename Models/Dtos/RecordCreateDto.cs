namespace RecordManiaAPI.Models.Dtos
{
    public class RecordCreateDto
    {
        public int StudentId { get; set; }
        public int LanguageId { get; set; }

        public int? TaskId { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }

        public long ExecutionTime { get; set; }
    }
}