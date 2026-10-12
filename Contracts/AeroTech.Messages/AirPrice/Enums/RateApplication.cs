using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.AirPrice.Enums;

public enum RateApplication
{
    [Display(Name = "Fare Construction")] FareConstruction = 1,
    [Display(Name = "Alternate Payment Currency")] AlternatePaymentCurrency,
    [Display(Name = "Payment Conversion")] PaymentConversion,
    [Display(Name = "Wallet Conversion")] WalletConversion,
    [Display(Name = "Interline Billing")] InterlineBilling,
    [Display(Name = "Interline Settlement")] InterlineSettlement,
    [Display(Name = "Accounting Initial Recognition")] AccountingInitialRecognition,
    [Display(Name = "Accounting Closing")] AccountingClosing
}
