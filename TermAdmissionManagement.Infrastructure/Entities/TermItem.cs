using System;
using System.Collections.Generic;

namespace TermAdmissionManagement.Infrastructure.Entities;

public partial class TermItem
{
    public int Id { get; set; }

    public int? MaxNumberRegistration { get; set; }

    public int? CurrentRegisteredStudents { get; set; }

    public int? ExpectedClasses { get; set; }

    public string Status { get; set; }


    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int Version { get; set; }

    public int? AdmissionTermId { get; set; }

    public bool IsCurrent { get; set; }

    public virtual AdmissionTerm AdmissionTerm { get; set; }

    public virtual ICollection<AdmissionForm> AdmissionForms { get; set; } = new List<AdmissionForm>();

}
