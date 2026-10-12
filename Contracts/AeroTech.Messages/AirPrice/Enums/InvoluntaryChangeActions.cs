using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroTech.Messages.AirPrice.Enums;

public enum InvoluntaryChangeActions
{
    [Display(Name = "No action needed")] NoActionNeeded=1,
    [Display(Name = "Action needed")] ActionNeeded=2,
    [Display(Name = "No action needed, included notify")] ActionNeededIncludedNotify = 3
}