namespace RecordManiaAPI.Models
{
    // Klasa Task została przemianowana na ProjectTask,
    // aby uniknąć konfliktu z System.Threading.Tasks.Task
    public class ProjectTask
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ICollection<Record> Records { get; set; } = new List<Record>();
    }
}