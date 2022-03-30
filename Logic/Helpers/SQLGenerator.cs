using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Helpers
{
    #region JOIN SQL constructor class

    public class JOINConstructor
    {
        /// <summary>Name of table being joined.</summary>
        private readonly string table;

        /// <summary>Contains JOIN string contents.</summary>
        private readonly StringBuilder joinStr;

        /// <summary>Count of JOIN criteria.</summary>
        public int CriteriaCount { private set; get; }

        /// <summary>
        /// Creates a JOIN constructor object that will put together a SQL string containing JOIN content.
        /// </summary>
        /// <param name="table">Table being joined.</param>
        /// <param name="joinType">Type of JOIN (INNER, LEFT, RIGHT, OUTER). It is INNER by default.</param>
        public JOINConstructor(string table, string joinType = "INNER")
        {
            this.table = table;
            joinStr = new StringBuilder(string.Format("{0} JOIN {1} ON ", joinType, table));
            CriteriaCount = 0;
        }

        /// <summary>
        /// Adds JOIN criteria based on a given target table or parameterized value that the current table is 
        /// being joined to.
        /// </summary>
        /// <param name="tableParam">Joined table parameter being used as the JOIN point.</param>
        /// <param name="targetParam">
        /// Target table parameter being used as the JOIN point. If there is no associated target table 
        /// passed, then it is treated like a parameterized value instead.
        /// </param>
        /// <param name="targetTable">
        /// Table that the current table is being joined to (null if the target parameter is a parameterized value).
        /// </param>
        public void AddJoinCriteria(string tableParam, string targetParam, string targetTable = null)
        {
            if (CriteriaCount > 0)
                joinStr.Append(" AND ");
            if (targetTable != null)
                joinStr.AppendFormat("{0}.{1}={2}.{3}", table, tableParam, targetTable, targetParam);
            else
                joinStr.AppendFormat("{0}.{1}=@{2}", table, tableParam, targetParam);
            CriteriaCount++;
        }

        /// <summary>Returns the final SQL content of the JOIN constructor.</summary>
        /// <returns>SQL content representing a JOIN.</returns>
        public override string ToString()
        {
            return joinStr.ToString();
        }
    }

    #endregion

    /// <summary>Used to generate basic SQL queries with parameterize values that Dapper can use.</summary>
    public static class SQLGenerator
    {
        #region Helper functions

        /// <summary>Generates a SQL string with a column filter (typically for WHERE clauses).</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="col">Name of column.</param>
        /// <param name="param">Parameter being used to filter the column.</param>
        /// <param name="comparator">Comparator type (uses equal sign by default).</param>
        /// <returns>SQL string containing a column filter.</returns>
        private static string GenerateColumnFilterSQL(string table, string col, string param, string comparator = "=")
        {
            return string.Format("{0}.{1} {2} @{3}", table, col, comparator, param);
        }

        /// <summary>Generates a WHERE clause based on the given parameters.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="parameters">Parameter pairings for SQL query (column name to parameter).</param>
        /// <param name="listParameters">Set of parameters that are mapped to lists (if any).</param>
        /// <returns>WHERE clause SQL string.</returns>
        private static string GenerateWhereClause(string table, IDictionary<string, string> parameters,
            IEnumerable<string> listParameters = null)
        {
            return "WHERE " + string.Join(" AND ", parameters.Keys.Select(
                column => {
                    string comparator = listParameters != null && listParameters.Contains(parameters[column]) ? "IN" : "=";
                    return GenerateColumnFilterSQL(table, column, parameters[column], comparator);
                }));
        }

        /// <summary>Generates SQL string containing list items separated by commas.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="list">List of items to add to the SQL string</param>
        /// <param name="parameterizeItems">Flag for prepending values with "@" (false by default).</param>
        /// <returns>SQL string with comma-separated items.</returns>
        private static string GenerateItemListSQL(string table, IEnumerable<string> list, bool parameterizeItems = false)
        {
            return string.Join(", ", list.Select(item => parameterizeItems ? "@" + item : table + "." + item));
        }

        /// <summary>Generates SQL string responsible for ordering a SQL query's results.</summary>
        /// <param name="orderBy">Column names to order a query's results by, in list order.</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <returns>ORDER BY SQL string.</returns>
        private static string GenerateOrderBySQL(IEnumerable<string> orderBy, IEnumerable<bool> orderByASC = null)
        {
            var orderByASCStrs = orderByASC == null 
                ? orderBy.Select(_ => " ASC").ToList() 
                : orderByASC.Select(asc => asc ? " ASC" : " DESC").ToList();
            return "ORDER BY " + string.Join(",", orderBy.Select((o, i) => o + orderByASCStrs[i]));
        }

        #endregion

        /// <summary>Generates a "get-all" SQL query based on the given table.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="selectParam">Parameters used for select statement (uses * by default)</param>
        /// <param name="joinSQL">SQL string containing JOIN clauses, if any (null by default).</param>
        /// <param name="orderBy">Column names to order a query's results by, in list order.</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <returns>SQL for "get-all" query.</returns>
        public static string GenerateGetAllSQL(string table, string selectParam = "*", string joinSQL = null, IEnumerable<string> orderBy = null,
            IEnumerable<bool> orderByASC = null)
        {
            StringBuilder sql = new StringBuilder();

            // Applies the SQL string associated with JOINs
            if (joinSQL != null)
                sql.Append(string.Format("SELECT {0} FROM {1} {2}", selectParam, table, joinSQL));
            else
                sql.Append(string.Format("SELECT {0} FROM {1}", selectParam, table));

            // Applies ORDER BY sql, if any
            if (orderBy != null)
                sql.AppendFormat(" {0}", GenerateOrderBySQL(orderBy, orderByASC));
            sql.Append(";");

            return sql.ToString();
        }

        /// <summary>Generates a "get" SQL query based on the given table and SQL parameters.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="selectParam">Parameters used for select statement (uses * by default)</param>
        /// <param name="parameters">Parameter pairings for SQL query (column name to parameter), can be null.</param>
        /// <param name="distinct">Flag for performing a DISTINCT query.</param>
        /// <param name="topItems">Top number of items for SELECT statement (if any).</param>
        /// <param name="joinSQL">SQL string containing JOIN clauses, if any (null by default).</param>
        /// <param name="orderBy">Column names to order a query's results by, in list order.</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <param name="listParameters">Set of parameters that are mapped to lists (if any).</param>
        /// <returns>SQL for "get" query.</returns>
        public static string GenerateGetSQL(string table, string selectParam = "*",
            IDictionary<string, string> parameters = null, bool distinct = false, int? topItems = null, string joinSQL = null,
            IEnumerable<string> orderBy = null, IEnumerable<bool> orderByASC = null, IEnumerable<string> listParameters = null)
        {
            // Generates basic SELECT statement of query
            StringBuilder sql = new StringBuilder("SELECT ");
            if (distinct)
                sql.Append("DISTINCT ");
            if (topItems.HasValue)
                sql.Append($"TOP {topItems.Value} ");
            sql.AppendFormat("{0} FROM {1}", selectParam, table);

            // Adds JOIN clause(s) contained in the given JOIN SQL variable, if any
            if (joinSQL != null)
                sql.AppendFormat(" {0}", joinSQL);

            // Adds WHERE clause depending on the parameters passed
            if (parameters != null && parameters.Count > 0)
                sql.AppendFormat(" {0}", GenerateWhereClause(table, parameters, listParameters));

            // Applies ORDER BY sql, if any
            if (orderBy != null)
                sql.AppendFormat(" {0}", GenerateOrderBySQL(orderBy, orderByASC));

            sql.Append(";");

            return sql.ToString();
        }

        /// <summary>Generates an "insert" SQL query based on the given table and SQL parameters.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="parameters">Parameter pairings for SQL query (column name to parameter).</param>
        /// <param name="outputResults">Adds OUTPUT INSERTED to query if true (won't work well with tables containing triggers).</param>
        /// <returns>SQL for "insert" query.</returns>
        public static string GenerateInsertSQL(string table, IDictionary<string, string> parameters, bool outputResults = true)
        {
            // Generates basic INSERT INTO statement of query
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(table);

            // Inserts column names enclosed by parentheses
            sql.AppendFormat(" ({0}) ", GenerateItemListSQL(table, parameters.Keys));

            // Used to output the results of the insert
            if (outputResults)
            {
                sql.Append("OUTPUT INSERTED.* ");
            }

            // Inserts values enclosed by parentheses
            sql.AppendFormat("VALUES ({0});", GenerateItemListSQL(table, parameters.Values, true));

            return sql.ToString();
        }

        /// <summary>Generates an "update" SQL query based on the given table and SQL parameters.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="parameters">Parameter pairings for SQL query (column name to parameter).</param>
        /// <param name="whereColumns">Parameters that will be used for WHERE clause.</param>
        /// <param name="listParameters">Set of parameters that are mapped to lists (if any).</param>
        /// <returns>SQL for "update" query.</returns>
        public static string GenerateUpdateSQL(string table, IDictionary<string, string> parameters,
            IEnumerable<string> whereColumns, IEnumerable<string> listParameters = null)
        {
            // Generates basic UPDATE statement of query
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(table);

            // Sets up the WHERE parameters
            Dictionary<string, string> whereParams = new Dictionary<string, string>();
            foreach (string whereCol in whereColumns)
                whereParams[whereCol] = parameters[whereCol];

            // Sets up SET clause with remaining parameters
            sql.Append(" SET ");
            sql.Append(string.Join(", ",
                parameters.Keys
                    .Where(column => !whereColumns.Contains(column))
                    .Select(column => GenerateColumnFilterSQL(table, column, parameters[column]))));

            // Sets up WHERE clause with the WHERE parameters
            string whereClause = GenerateWhereClause(table, whereParams, listParameters);
            sql.AppendFormat(" {0};", whereClause);

            // Adds SELECT query afterwards to retrieve data
            sql.AppendFormat(" SELECT TOP 1 * FROM {0} {1};", table, whereClause);

            return sql.ToString();
        }

        /// <summary>Generates a "delete" SQL query based on the given table and SQL parameters.</summary>
        /// <param name="table">Name of database table.</param>
        /// <param name="parameters">Parameter pairings for SQL query (column name to parameter).</param>
        /// <param name="listParameters">Set of parameters that are mapped to lists (if any).</param>
        /// <returns>SQL for "delete" query.</returns>
        public static string GenerateDeleteSQL(string table, IDictionary<string, string> parameters,
            IEnumerable<string> listParameters = null)
        {
            return string.Format("DELETE FROM {0} {1};", table, GenerateWhereClause(table, parameters, listParameters));
        }
    }
}