namespace WarHammer40K.DTOs
{
    public class CharacterDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public int FactionId { get; set; } // Id de la facción a la que pertenece el personaje
    }
}
