namespace DynamicLinqSearch
{
    public class FilterQuery
    {
        public string Column { get; set; }
        public string Condition { get; set; }
        public RuleRelation Relation { get; set; }
        public Statement Statement { get; set; }
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

    public enum Statement
    {
        And = 1,
        Or = 2
    }
}
