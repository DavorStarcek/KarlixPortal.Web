namespace KarlixPortal.Web.Models
{
    public class UserProfileViewModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Tenant { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();

        public IList<ClaimItem> Claims { get; set; } = new List<ClaimItem>();
    }

    public class ClaimItem
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
