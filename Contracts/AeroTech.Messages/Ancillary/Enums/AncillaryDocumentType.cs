using System.ComponentModel.DataAnnotations;

namespace AeroTech.Messages.Ancillary.Enums
{
    public enum AncillaryDocumentType
    {
        [Display(Name = "EMD Associated")] EmdAssociated = 2,
        [Display(Name = "EMD Standalone")] EmdStandalone = 3
    }
}
