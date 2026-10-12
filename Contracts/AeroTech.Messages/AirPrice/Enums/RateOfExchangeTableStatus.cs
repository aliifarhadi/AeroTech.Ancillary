using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.AirPrice.Enums;

public enum RateOfExchangeTableStatus
{
    [Display(Name = "Draft")] Draft = 1,
    [Display(Name = "Published")] Published,
    [Display(Name = "Expired")] Expired,
    [Display(Name = "Cancelled")] Cancelled
}
