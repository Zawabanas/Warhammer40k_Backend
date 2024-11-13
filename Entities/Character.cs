namespace WarHammer40K.Entities
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Imagen { get; set; }

        // Relación con la facción
        public int FactionId { get; set; }
        public Faction Faction { get; set; }

    }
}
