using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.AirPrice.Enums;


public enum AirChargeJurisdictionLevel
{
    [Display(Name = "Country")] Country = 1,
    [Display(Name = "State")] State,
    [Display(Name = "Airport")] Airport
}
