using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Application.DTOs.Request
{
    public record CreateAdmissionFormRequest( string ChildCharacteristicsFormImg, string CommitmentImg, string HouseholdRegistrationAddress, string Note, int StudentId, int TermItemId);
}
