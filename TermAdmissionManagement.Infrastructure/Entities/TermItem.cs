using System;
using System.Collections.Generic;

namespace TermAdmissionManagement.Infrastructure.Entities;

public partial class TermItem
{
    public int Id { get; set; }

    public int AdmissionTermId { get; set; }

    public int? MaxNumberRegistration { get; set; }

    public int? CurrentRegisteredStudents { get; set; }

    public int? ExpectedClasses { get; set; }

    public string? Grade { get; set; }

    public string? Status { get; set; }

    public int DefaultFee { get; set; }

    public virtual AdmissionTerm AdmissionTerm { get; set; } = null!;

    public virtual ICollection<AdmissionForm> AdmissionForms { get; set; } = new List<AdmissionForm>();

}
