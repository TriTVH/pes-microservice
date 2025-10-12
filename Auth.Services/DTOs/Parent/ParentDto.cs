using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs.Parent
{
    public record ParentDto(
        int Id,
        string Email,
        string Name,
        string Job,
        string RelationshipToChild,
        int AccountId
    );
}
