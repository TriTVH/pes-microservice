using System;
using System.Collections.Generic;

namespace TermAdmissionManagement.Infrastructure.Entities;

public partial class AdmissionTerm
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Year { get; set; }

    public virtual ICollection<TermItem> TermItems { get; set; } = new List<TermItem>();
}
