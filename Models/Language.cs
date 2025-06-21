namespace RecordManiaAPI.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Record> Records { get; set; } = new List<Record>();
    }
}