namespace WarHammer40K.Entities
{
    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }  // Ejemplo: Infantería, Tanque, etc.
        public int PowerLevel { get; set; }
        public string Imagen { get; set; }  // URL de la imagen de la unidad
        public int FactionId { get; set; }  // Relación con Faction
        public Faction Faction { get; set; }
    }
}
