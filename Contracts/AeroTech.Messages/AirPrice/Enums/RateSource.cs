using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.AirPrice.Enums;

public enum RateSource
{
    [Display(Name = "Airline Manual")] AirlineManual = 1,
    [Display(Name = "IATA IROE")] IATA_IROE,
    [Display(Name = "IATA ICER")] IATA_ICER,
    [Display(Name = "IATA ICH FDR")] IATA_ICH_FDR,
    [Display(Name = "IATA ICH MMR")] IATA_ICH_MMR,
    [Display(Name = "IATA ICH CDR")] IATA_ICH_CDR,
    [Display(Name = "Central Bank")] CentralBank,
    [Display(Name = "Commercial Bank")] CommercialBank,
    [Display(Name = "Payment Provider")] PaymentProvider
}
