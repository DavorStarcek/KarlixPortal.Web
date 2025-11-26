namespace KarlixPortal.Web.Models
{
    public class SsoAppViewModel
    {
        public string Key { get; set; } = string.Empty;       // npr. "karlixid-admin"
        public string Name { get; set; } = string.Empty;      // naslov
        public string? Description { get; set; }              // opis
            = string.Empty;
        public string? Icon { get; set; }                     // emoji ili CSS ikonice
            = "🔗";
        public string Url { get; set; } = string.Empty;       // target URL aplikacije
        public bool IsEnabled { get; set; } = true;           // ako je false → "uskoro"
        public string? RequiredRole { get; set; }             // npr. "GlobalAdmin" ili null za sve
    }
}
