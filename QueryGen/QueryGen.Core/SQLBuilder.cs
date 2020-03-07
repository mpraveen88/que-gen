using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace QueryGen.Core
{
    public sealed class SQLBuilder<T>
    {
        private static SQLBuilder<T> instance = null;
        internal StringBuilder sb;
        private SQLBuilder() { sb = new StringBuilder(); }

        public static SQLBuilder<T> GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new SQLBuilder<T>();
                return instance;
            }
        }

        public SQLBuilder<T> GroupConditions(Func<SQLBuilder<T>, SQLBuilder<T>> builder)
        {
            sb.Append("(");
            var dummy =(SQLBuilder<T>)builder.Invoke(this);
            sb.Append(")");
            return this;
        }

        public SQLBuilder<T> And()
        {
            sb.Append($" AND ");
            return this;
        }

        public SQLBuilder<T> Or()
        {
            sb.Append($" OR ");
            return this;
        }

        public SQLBuilder<T> EqualsTo<S>(Expression<Func<T, object>> obj, S value)
        {
            sb.Append($" ({GetPropertyName(obj)} = { FormatValue<S>(value)}) ");
            return this;
        }

        public SQLBuilder<T> NotEqualsTo<S>(Expression<Func<T, object>> obj, S value)
        {
            sb.Append($" ({GetPropertyName(obj)} <> { FormatValue<S>(value)}) ");
            return this;
        }

        public SQLBuilder<T> IsNull<S>(Expression<Func<T, object>> obj)
        {
            sb.Append($" ({GetPropertyName(obj)} IS NULL ) ");
            return this;
        }

        public SQLBuilder<T> IsNotNull<S>(Expression<Func<T, object>> obj)
        {
            sb.Append($" ({GetPropertyName(obj)} IS NOT NULL ) ");
            return this;
        }

        public SQLBuilder<T> Between<S>(Expression<Func<T, object>> obj, S lowerBound, S upperBound)
        {
            sb.Append($" ( {GetPropertyName(obj)} BETWEEN {FormatValue<S>(lowerBound)} AND {FormatValue<S>(upperBound)} ) ");
            return this;
        }

        public SQLBuilder<T> StartsWith(Expression<Func<T, object>> obj, string searchString)
        {
            sb.Append($" (LTRIM(RTRIM(LOWER({GetPropertyName(obj)}))) LIKE '{searchString.Trim().ToLower()}%' ) ");
            return this;
        }

        public SQLBuilder<T> EndsWith(Expression<Func<T, object>> obj, string searchString)
        {
            sb.Append($" (LTRIM(RTRIM(LOWER({GetPropertyName(obj)}))) LIKE '%{searchString.Trim().ToLower()}' ) ");
            return this;
        }

        public SQLBuilder<T> Contains(Expression<Func<T, object>> obj, string searchString)
        {
            sb.Append($" (LTRIM(RTRIM(LOWER({GetPropertyName(obj)}))) LIKE '%{searchString.Trim().ToLower()}%' ) ");
            return this;
        }


        public string GetWhereClause()
        {
            var result = sb.ToString().Trim();

            if (result.EndsWith("AND") || result.EndsWith("OR") || result.EndsWith("NOT"))
                throw new Exception("Query should not end with AND/OR/NOT");

            return $" {(string.IsNullOrEmpty(result) ? " 1=1 " : result )} ";
        }


        public string GetSQL(bool onlyWhereClause = true, params Expression<Func<T, object>>[] selectColumns)
        {
            if (onlyWhereClause)
                return $" WHERE {GetWhereClause()} ";

            string query = $"SELECT {GetSelectColums(selectColumns)} FROM {GetTableName()} WHERE {GetWhereClause()}";
            return query;
        }



        #region private methods

        private string GetPropertyName(Expression<Func<T, object>> expression)
        {
            try
            {
                var columnName = "";
                var body = expression.Body as MemberExpression;

                if (body == null)
                {
                    body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
                }
                if (body.Member.CustomAttributes.Any())
                    columnName = body.Member?.CustomAttributes?.Where(x => x.AttributeType == typeof(ColumnAttribute))?.FirstOrDefault()?.ConstructorArguments?.FirstOrDefault().Value?.ToString();

                columnName = body.Member.Name.Trim();
                columnName = columnName.StartsWith("[") ? columnName : "[" + columnName;
                columnName = columnName.EndsWith("]") ? columnName : columnName + "]";
                return columnName;
            }
            catch
            {
                throw;
            }
        }


        private string GetTableName()
        {
            Type table = typeof(T);
            
            string tableName = table.Name;

            if (table.CustomAttributes.Any())
            {
                tableName= table.CustomAttributes?.Where(x => x.AttributeType == typeof(TableAttribute))?.FirstOrDefault()?.ConstructorArguments?.FirstOrDefault().Value?.ToString().Trim();
            }
            tableName = tableName.StartsWith("[") ? tableName : "[" + tableName;
            tableName = tableName.EndsWith("]") ? tableName : tableName + "]";
            return tableName;
        }

        private string GetSelectColums(params Expression<Func<T, object>>[] expressions)
        {
            try
            {
                if (!expressions.Any())
                    return "*";

                List<string> columnNames = new List<string>();
                foreach (var item in expressions)
                {
                    columnNames.Add(GetPropertyName(item));
                }
                return String.Join(",", columnNames.ToArray());
            }
            catch
            {
                throw;
            }
        }



        private object FormatValue<S>(S value, string sqlDateFormat = "yyyy-MM-dd")
        {
            try
            {
                if (value is string || value is char)
                    return $"'{ value}'";
                else if (value is DateTime)
                    return $"{ DateTime.Parse(value.ToString()).ToString(sqlDateFormat.Trim()) }";
                else
                    return value;
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
