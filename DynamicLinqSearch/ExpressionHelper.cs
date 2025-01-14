using System.Linq.Expressions;
using System.Reflection;

namespace DynamicLinqSearch
{
    public static class ExpressionHelper
    {
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        public static Expression<Func<TEntity, bool>> BuildDynamicFilter<TEntity>(List<FilterQuery> rules)
        {
            if (rules == null || rules.Count == 0)
                throw new ArgumentException("Rules cannot be null or empty.");

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression filterExpression = null;

            foreach (var rule in rules)
            {
                var property = GetPropertyExpression(parameter, rule.Column);
                var constant = GetConstantExpression(property.Type, rule.Condition);

                var binaryExpression = BuildBinaryExpression(rule.Relation, property, constant);

                filterExpression = filterExpression == null
                    ? binaryExpression
                    : CombineExpressions(filterExpression, binaryExpression, rule.Statement);
            }

            return Expression.Lambda<Func<TEntity, bool>>(filterExpression, parameter);
        }

        public static Expression<Func<TEntity, bool>> BuildDynamicSearch<TEntity>(string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchValue))
                throw new ArgumentException("Search value cannot be null or empty.");

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
            Expression searchExpression = null;

            foreach (var propertyInfo in typeof(TEntity).GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    var property = Expression.Property(parameter, propertyInfo);
                    var constant = Expression.Constant(searchValue, typeof(string));
                    var containsExpression = Expression.Call(property, ContainsMethod, constant);

                    searchExpression = searchExpression == null
                        ? containsExpression
                        : Expression.OrElse(searchExpression, containsExpression);
                }
            }

            return searchExpression == null
                ? x => false
                : Expression.Lambda<Func<TEntity, bool>>(searchExpression, parameter);
        }

        private static Expression GetPropertyExpression(Expression parameter, string column)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("Column name cannot be null or empty.");

            return column.Split('.').Aggregate(parameter, Expression.PropertyOrField);
        }

        private static ConstantExpression GetConstantExpression(Type propertyType, string value)
        {
            if (propertyType == null)
                throw new ArgumentException("Property type cannot be null.");

            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var convertedValue = Convert.ChangeType(value, underlyingType);

            return Expression.Constant(convertedValue, propertyType);
        }

        private static Expression BuildBinaryExpression(RuleRelation relation, Expression property, Expression constant)
        {
            return relation switch
            {
                RuleRelation.GreaterThan => Expression.GreaterThan(property, constant),
                RuleRelation.LessThan => Expression.LessThan(property, constant),
                RuleRelation.Equal => Expression.Equal(property, constant),
                RuleRelation.NotEqual => Expression.NotEqual(property, constant),
                RuleRelation.Contains => Expression.Call(property, ContainsMethod, constant),
                RuleRelation.NotContains => Expression.Not(Expression.Call(property, ContainsMethod, constant)),
                RuleRelation.StartsWith => Expression.Call(property, StartsWithMethod, constant),
                RuleRelation.EndsWith => Expression.Call(property, EndsWithMethod, constant),
                _ => throw new NotSupportedException($"Unsupported relation: {relation}.")
            };
        }

        private static Expression CombineExpressions(Expression left, Expression right, Statement statement)
        {
            return statement switch
            {
                Statement.And => Expression.AndAlso(left, right),
                Statement.Or => Expression.OrElse(left, right),
                _ => throw new ArgumentException($"Unsupported statement type: {statement}.")
            };
        }
    }

}
