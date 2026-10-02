using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum ServiceSubCodeStatus
    {
        [Display(Name = "Active")] Active = 1,
        [Display(Name = "Retired")] Retired = 2
    }
}
