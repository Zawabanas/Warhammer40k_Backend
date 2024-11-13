namespace WarHammer40K.DTOs
{
    public class FactionDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
