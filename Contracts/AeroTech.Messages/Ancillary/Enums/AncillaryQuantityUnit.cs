using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum AncillaryQuantityUnit
    {
        [Display(Name = "Piece")] Piece = 1,
        [Display(Name = "Each")] Each = 3
    }
}
