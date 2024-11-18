using System;
using System.Collections.Generic;
using System.Text;

namespace Minimal.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PropFlagAttribute : Attribute
    {
        public string Flag { get; set; } = "";

        public PropFlagAttribute(string flag)
        {
            this.Flag = flag;
        }

        public PropFlagAttribute()
        {
        }
    }

    public record PropFlags
    {
        public const string FK = "FK";
        public const string FKREF = "FK_REF";
        public const string FKREFCOL = "FK_REF_COL";
        public const string TRANSLATABLE = "TRANSLATABLE";
        public const string FILTER_TEMPLATE_IGNORE = "FILTER_TEMPLATE_IGNORE";
        public const string FIELD_TEMPLATE_IGNORE = "FIELD_TEMPLATE_IGNORE";
        public const string REFIGNORE = "REF_IGNORE";
        public const string RICHTEXT = "RICHTEXT";
        public const string QuickFilter = "QuickFilter";
        public const string UNIQUE = "UNIQUE";

        public const string REPORT_TEMPLATE_IGNORE = "REPORT_TEMPLATE_IGNORE";
        public const string REPORT_IMAGE_COL = "REPORT_IMAGE_COL";
        public const string REPORT_PRECENTAGE_COL = "REPORT_PRECENTAGE_COL";
        public const string REPORT_MONEY_COL = "REPORT_MONEY_COL";

        public const string KEY_VALUE_LIST = "KEY_VALUE_LIST";
        public const string HASH_KEY = "HASH_KEY";
        public const string HASH_VAL = "HASH_VAL";
        
    }
}
