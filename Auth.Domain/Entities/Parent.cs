using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Entities
{
    public partial class Parent
    {
        public int Id { get; set; }

        public string Job { get; set; }

        public string RelationshipToChild { get; set; }

        public int? AccountId { get; set; }
    }
}
