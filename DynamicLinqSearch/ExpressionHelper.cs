using System.Linq.Expressions;
using System.Reflection;

namespace DynamicLinqSearch
{
    public static class ExpressionHelper
    {
        public static Expression<Func<TEntity, bool>> BuildDynamicFilter<TEntity>(List<FilterQuery> rules)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            Expression filter = null;

            foreach (var rule in rules)
            {
                Expression property = GetPropertyExpression(parameterExpression, rule.Column);

                Type propertyType = property.Type;
                Type underlyingType = Nullable.GetUnderlyingType(propertyType);

                object convertedValue = Convert.ChangeType(rule.Condition, underlyingType ?? propertyType);
                ConstantExpression constant = Expression.Constant(convertedValue, property.Type);
                Expression binaryExpression = BuildBinaryExpression(rule.Relation, property, constant);

                if (rule.Statement == "And")
                    filter = filter != null ? Expression.AndAlso(filter, binaryExpression) : binaryExpression;
                else if (rule.Statement == "Or")
                    filter = filter != null ? Expression.OrElse(filter, binaryExpression) : binaryExpression;
                else
                    throw new ArgumentException("Invalid statement type, only 'And' and 'Or'");
            }

            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(filter, parameterExpression);
            return lambda;
        }

        public static Expression<Func<TEntity, bool>> BuildDynamicSearch<TEntity>(string searchValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity));
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodInfo toUpperMethod = typeof(string).GetMethod("ToUpper", new Type[] { });
            Type expressionHelper = typeof(ExpressionHelper);
            MethodInfo turkishCharacterFix = expressionHelper.GetMethod("ConvertTurkishCharactersToEnglish", BindingFlags.Static | BindingFlags.NonPublic);

            Expression filter = null;

            foreach (var propertyInfo in typeof(TEntity).GetProperties())
            {
                Expression property = Expression.Property(parameterExpression, propertyInfo);

                if (property.Type == typeof(string))
                {
                    ConstantExpression constant = Expression.Constant(searchValue.ToUpper(), typeof(string));

                    Expression toUpperExpression = Expression.Call(property, toUpperMethod);

                    Expression turkishFixExpression = Expression.Call(null, turkishCharacterFix, toUpperExpression);

                    Expression containsExpression = Expression.Call(turkishFixExpression, containsMethod, Expression.Call(constant, toUpperMethod));

                    filter = filter != null
                        ? Expression.OrElse(filter, containsExpression)
                        : containsExpression;
                }
            }

            Expression<Func<TEntity, bool>> lambda = Expression.Lambda<Func<TEntity, bool>>(filter, parameterExpression);
            return lambda;
        }

        private static Expression GetPropertyExpression(Expression parameterExpression, string column)
        {
            if (column.Contains("."))
            {
                string[] columnParts = column.Split('.');
                Expression property = parameterExpression;

                foreach (var part in columnParts)
                {
                    property = Expression.PropertyOrField(property, part);
                }

                return property;
            }
            else
            {
                return Expression.PropertyOrField(parameterExpression, column);
            }
        }

        private static Expression BuildBinaryExpression(RuleRelation relation, Expression property, ConstantExpression constant)
        {
            MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

            switch (relation)
            {
                case RuleRelation.GreaterThan:
                    return Expression.GreaterThanOrEqual(property, constant);
                case RuleRelation.LessThan:
                    return Expression.LessThanOrEqual(property, constant);
                case RuleRelation.Equal:
                    return Expression.Equal(property, constant);
                case RuleRelation.NotEqual:
                    return Expression.NotEqual(property, constant);
                case RuleRelation.Contains:
                    return Expression.Call(property, containsMethod, constant);
                case RuleRelation.NotContains:
                    return Expression.Not(Expression.Call(property, containsMethod, constant));
                case RuleRelation.StartsWith:
                    return Expression.Call(property, startsWithMethod, constant);
                case RuleRelation.EndsWith:
                    return Expression.Call(property, endsWithMethod, constant);
                default:
                    throw new ArgumentException("Invalid rule relation.");
            }
        }

        private static string ConvertTurkishCharactersToEnglish(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            input = input.Replace("ı", "i");
            input = input.Replace("ğ", "g");
            input = input.Replace("ü", "u");
            input = input.Replace("ş", "s");
            input = input.Replace("ö", "o");
            input = input.Replace("ç", "c");
            input = input.Replace("İ", "I");
            input = input.Replace("Ğ", "G");
            input = input.Replace("Ü", "U");
            input = input.Replace("Ş", "S");
            input = input.Replace("Ö", "O");
            input = input.Replace("Ç", "C");

            return input;
        }
    }

}
