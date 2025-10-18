using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services.Validation
{
    public class RelationshipValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return false;
            
            var relationship = value.ToString();
            return relationship == "Cha" || relationship == "Mẹ";
        }

        public override string FormatErrorMessage(string name)
        {
            return $"The {name} field must be either 'Cha' or 'Mẹ'";
        }
    }
}

