namespace WarHammer40K.DTOs
{
    public class UnitDTO
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int PowerLevel { get; set; }
        public IFormFile Image { get; set; }  // Para la carga de imágenes
        public int FactionId { get; set; }
    }
}
