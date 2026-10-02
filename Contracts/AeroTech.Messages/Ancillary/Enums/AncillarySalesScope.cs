using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum AncillarySalesScope
    {
        [Display(Name = "Traveller Bound")] TravellerBound = 1,
        [Display(Name = "Traveller Segment")] TravellerSegment = 2
    }
}
