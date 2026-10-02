using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum AncillaryWeightUnit
    {
        [Display(Name = "Kg")] Kg = 1,
        [Display(Name = "Lbs")] Lbs = 2
    }
}
