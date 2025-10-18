using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Enums
{
    public enum RelationshipType
    {
        Cha,
        Mẹ
    }

    public static class RelationshipTypeExtensions
    {
        public static string GetDisplayName(this RelationshipType relationship)
        {
            return relationship switch
            {
                RelationshipType.Cha => "Cha",
                RelationshipType.Mẹ => "Mẹ",
                _ => throw new ArgumentException("Invalid relationship type")
            };
        }

        public static bool IsValid(string relationship)
        {
            return relationship == "Cha" || relationship == "Mẹ";
        }
    }
}

