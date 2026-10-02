using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum AncillaryProductType
    {
        [Display(Name = "Extra Baggage")] ExtraBaggage = 1,
        [Display(Name = "Lounge Access")] LoungeAccess = 2
    }
}
