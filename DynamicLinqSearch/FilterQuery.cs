using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicLinqSearch
{
    public class FilterQuery
    {
        public string Column { get; set; }
        public string Condition { get; set; }
        public RuleRelation Relation { get; set; }
        public string Statement { get; set; }
    }

    public enum RuleRelation
    {
        GreaterThan = 1,
        LessThan = 2,
        Equal = 3,
        NotEqual = 4,
        Contains = 5,
        NotContains = 6,
        StartsWith = 7,
        EndsWith = 8
    }
}
