using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.RequestFeatures
{

    public enum FilterOptions
    {
        [Description("Starts With")]
        StartsWith = 1,
        [Description("Ends With")]
        EndsWith,
        Contains,
        [Description("Does Not Contain")]
        DoesNotContain,
        [Description("IS Empty")]
        IsEmpty,
        [Description("IS Not Empty")]
        IsNotEmpty,
        [Description("IS Grater Than")]
        IsGreaterThan,
        [Description("IS Grater Than Or Equal To")]
        IsGreaterThanOrEqualTo,
        [Description("IS Less Than")]
        IsLessThan,
        [Description("IS Less Than Or Equal To")]
        IsLessThanOrEqualTo,
        [Description("Is Equal To")]
        IsEqualTo,
        [Description("Is Not Equal To")]
        IsNotEqualTo,
        [Description("In")]
        In,
        [Description("Not In")]
        NotIn,
        [Description("Is Null")]
        IsNull,
        [Description("Is Not Null")]
        IsNotNull
    }

}
