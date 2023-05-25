using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platinum.Areas.Identity.Data
{
    public enum AccountType
    {
        [Display(Name = "Checking account")]
        Checking = 1,
        [Display(Name = "Savings account")]
        savings = 2,
        [Display(Name = "Investment account")]
        Investment = 3
    }
}
