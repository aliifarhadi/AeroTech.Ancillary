using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroTech.Messages.AirPrice.Enums;

public enum FlightApplicationIncludeType
{
    [Display(Name = "Include")] Include = 1,
    [Display(Name = "Exclude")] Exclude
}