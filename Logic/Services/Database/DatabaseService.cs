using Dapper;
using Logic.Helpers;
using Logic.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Logic.Services
{
    public class DatabaseService : IDatabaseService
    {
        private const string DEFAULT_CONNECTION = "DefaultConnection";

        #region Data Members 

        /// <summary>Database connection string.</summary>
        private readonly string dbConnectionString;

        /// <summary>Maps all property names to their associated database columns (for each type).</summary>
        private Dictionary<Type, Dictionary<string, string>> propertyToColumnMap;

        /// <summary>Maps all model types to their associated database table names.</summary>
        private Dictionary<Type, string> tableMap;

        #endregion


        #region Service Basics

        /// <summary>
        /// Initializes the database service, which will be injected with the appropriate settings.
        /// </summary>
        public DatabaseService(string connectionName = DEFAULT_CONNECTION)
        {
            // Sets database connection string (if default is requested, gets default connection name from app settings) 
            if (connectionName == DEFAULT_CONNECTION)
            {
                connectionName = ConfigurationManager.AppSettings[DEFAULT_CONNECTION]?.ToString();
            }
            dbConnectionString = ConfigurationManager.ConnectionStrings[connectionName ?? ""]?.ConnectionString;

            // Sets various mappings for each of the database model classes
            _InitalizeTypeMappings();
        }

        /// <summary>
        /// Sets the mapping between every database model's corresponding class properties and database table columns.
        /// </summary>
        private void _InitalizeTypeMappings()
        {
            // Gets every Type within the next level's Models namespace
            var types = from type in Assembly.GetAssembly(typeof(DatabaseService)).GetTypes()
                        where type.Namespace == "Logic.Models" && type.Name != "IModel"
                        select type;

            // Maps every Type within the Models namespace accordingly
            propertyToColumnMap = new Dictionary<Type, Dictionary<string, string>>();
            tableMap = new Dictionary<Type, string>();
            foreach (Type model in types)
            {
                // Sets type mapper for Dapper
                SqlMapper.SetTypeMap(
                    model,
                    new CustomPropertyTypeMap(
                        model,
                        (type, columnName) =>
                            type.GetProperties().FirstOrDefault(prop =>
                                prop.GetCustomAttributes(false)
                                    .OfType<ColumnAttribute>()
                                    .Any(attr => attr.Name == columnName))
                    )
                );

                // Gets property name => table column name mapping for given type
                propertyToColumnMap[model] = new Dictionary<string, string>();
                foreach (PropertyInfo prop in model.GetProperties())
                {
                    ColumnAttribute attr = prop.GetCustomAttribute<ColumnAttribute>();
                    propertyToColumnMap[model][prop.Name] = attr.Name;
                }

                // Maps every model to its associated table name
                var tableAttr = model.GetTypeInfo().GetCustomAttribute<TableAttribute>();
                tableMap[model] = tableAttr.Name;
            }
        }

        /// <summary>
        /// Gets a dictionary mapping the appropriate column names to their parameter names, using 
        /// the given parameters dictionary.
        /// </summary>
        /// <typeparam name="M">Type of model used as the basis for the parameter mappings.</typeparam>
        /// <param name="parameters">Parameters being used for a SQL query.</param>
        /// <param name="listParameters">Parameters mapped to a list (if any), which are to be outputted on return.</param>
        /// <returns>Mapping associating database columns with parameter names.</returns>
        private Dictionary<string, string> GetParameterMappings<M>(object parameters,
            out HashSet<string> listParameters) where M : IModel
        {
            Dictionary<string, string> mapping = new Dictionary<string, string>();

            // Distinguishes between dictionaries and non-dictionary objects
            var properties = new Dictionary<string, object>();
            if (parameters is Dictionary<string, object> parameterDict)
            {
                properties = parameterDict;
            }
            else
            {
                properties = parameters.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => propertyToColumnMap[typeof(M)].ContainsKey(p.Name))
                    .ToDictionary(p => p.Name, p => p.GetValue(parameters));
            }

            // Iterates through parameters and sets appropriate mappings based on parameter type
            listParameters = new HashSet<string>();
            foreach (var propPair in properties)
            {
                string column = propertyToColumnMap[typeof(M)][propPair.Key];
                mapping[column] = propPair.Key;

                // If property value type is an array, it is added to the list
                if (propPair.Value is IEnumerable && !(propPair.Value is string))
                {
                    listParameters.Add(propPair.Key);
                }
            }

            return mapping;
        }

        /// <summary>
        /// Gets database connection.
        /// </summary>
        /// <returns>Database connection.</returns>
        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(dbConnectionString);
        }

        #endregion


        #region Query Database 

        /// <summary>Gets all data of the given model type from the given table.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="orderByProperties">Model properties to order a query's results by, in list order (may be null).</param>
        /// <param name="orderByASC">True if ORDER BY ascending, false if ORDER BY descending.</param>
        /// <returns>All model objects with data queried from the database table.</returns>
        public async Task<IEnumerable<M>> GetAll<M>(IEnumerable<string> orderByProperties = null, bool orderByASC = true) where M : IModel
        {
            // Transforms the order by properties list into an order by columns list
            IEnumerable<string> orderByColumns = null;
            if (orderByProperties != null)
                orderByColumns = orderByProperties.Select(e => propertyToColumnMap[typeof(M)][e]);

            string sql = SQLGenerator.GenerateGetAllSQL(tableMap[typeof(M)], orderBy: orderByColumns, orderByASC: orderByASC);

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync<M>(sql);
            }
        }

        /// <summary>
        /// Gets a single item of the given model type based on the parameters used to filter
        /// the results of the query.
        /// </summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to filter results for a single item.</param>
        /// <param name="orderByProperties">Determines ordering of items from which to retrieve first item.</param>
        /// <param name="orderByASC">True if ORDER BY ascending, false if ORDER BY descending.</param>
        /// <returns>Model object with data queried from the database table.</returns>
        public async Task<M> GetItem<M>(object parameters, IEnumerable<string> orderByProperties = null,
            bool orderByASC = true) where M : IModel
        {
            // Initializes the column => property mappings
            Dictionary<string, string> mapping = GetParameterMappings<M>(parameters, out HashSet<string> listParameters);

            // Performs query
            string sql = SQLGenerator.GenerateGetSQL(tableMap[typeof(M)], parameters: mapping, topItems: 1, listParameters: listParameters,
                orderBy: orderByProperties, orderByASC: orderByASC);

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryFirstOrDefaultAsync<M>(sql, parameters);
            }
        }

        /// <summary>
        /// Gets items of the given model type based on the parameters used to filter the results 
        /// of the query, as well as the various ORDER BY properties used to sort the data.
        /// </summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to filter results for a given set of items (can be null).</param>
        /// <param name="orderByProperties">Model properties to order a query's results by, in list order (may be null).</param>
        /// <param name="orderByASC">True if ORDER BY ascending, false if ORDER BY descending.</param>
        /// <param name="topItems">Top number of items for SELECT statement (if any).</param>
        /// <returns>List of model objects with data queried from the database table.</returns>
        public async Task<IEnumerable<M>> GetItems<M>(object parameters = null, IEnumerable<string> orderByProperties = null,
            bool orderByASC = true, int? topItems = null) where M : IModel
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            // Initializes the column => property mappings
            Dictionary<string, string> mapping = GetParameterMappings<M>(parameters, out HashSet<string> listParameters);

            // Gets column values of given model type only
            string tableName = tableMap[typeof(M)];
            string selectColumns = string.Join(", ", propertyToColumnMap[typeof(M)].Select(columnPair => $"{tableName}.{columnPair.Value}"));

            // Transforms the order by properties list into an order by columns list
            IEnumerable<string> orderByColumns = null;
            if (orderByProperties != null)
                orderByColumns = orderByProperties.Select(e => propertyToColumnMap[typeof(M)][e]);

            // Performs query
            string sql = SQLGenerator.GenerateGetSQL(tableMap[typeof(M)], selectColumns, mapping, true, topItems,
                orderBy: orderByColumns, orderByASC: orderByASC, listParameters: listParameters);

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync<M>(sql, parameters);
            }
        }

        /// <summary>
        /// Gets items of the given MResult model, which is the result of transforming the SQL query results using the transform 
        /// function, or transformFn.
        /// </summary>
        /// <typeparam name="M1">Primary model type within the SQL query.</typeparam>
        /// <typeparam name="M2">Secondary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="MResult">Model type which corresponds to the final return type of the query (in enumerable form).</typeparam>
        /// <param name="sql">Raw SQL being executed. Note that the column names in the SELECT statement should be ordered so that the columns
        /// associated with M1 are listed first before the column names for M2. Otherwise, the splitOn parameter will not be able to split the 
        /// data into two separate models properly.</param>
        /// <param name="transformFn">Function with two parameters that takes the initial query results and transforms it into a list
        /// of MResult items.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="splitOn">The cutoff point within the SQL query between M1's columns and M2's columns within the SQL query's SELECT 
        /// statement.</param>
        /// <returns>List of MResult items.</returns>
        public async Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, MResult>(string sql, Func<M1, M2, MResult> transformFn,
            object parameters = null, string splitOn = "Id")
        {
            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync(sql, transformFn, parameters, splitOn: splitOn);
            }
        }

        /// <summary>
        /// Gets items of the given MResult model, which is the result of transforming the SQL query results using the transform 
        /// function, or transformFn.
        /// </summary>
        /// <typeparam name="M1">Primary model type within the SQL query.</typeparam>
        /// <typeparam name="M2">Secondary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M3">Tertiary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="MResult">Model type which corresponds to the final return type of the query (in enumerable form).</typeparam>
        /// <param name="sql">Raw SQL being executed. Note that the column names in the SELECT statement should be ordered so that the columns
        /// associated with M1 are listed first before the column names for M2, then M3. Otherwise, the splitOn parameter will not be able to 
        /// split the data into three separate models properly.</param>
        /// <param name="transformFn">Function with three parameters that takes the initial query results and transforms it into a list
        /// of MResult items.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="splitOn">The cutoff point within the SQL query between M1's, M2's, and M3's columns within the SQL query's SELECT 
        /// statement.</param>
        /// <returns>List of MResult items.</returns>
        public async Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, MResult>(string sql, Func<M1, M2, M3, MResult> transformFn,
            object parameters = null, string splitOn = "Id")
        {
            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync(sql, transformFn, parameters, splitOn: splitOn);
            }
        }

        /// <summary>
        /// Gets items of the given MResult model, which is the result of transforming the SQL query results using the transform 
        /// function, or transformFn.
        /// </summary>
        /// <typeparam name="M1">Primary model type within the SQL query.</typeparam>
        /// <typeparam name="M2">Secondary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M3">Tertiary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M4">Quarternary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="MResult">Model type which corresponds to the final return type of the query (in enumerable form).</typeparam>
        /// <param name="sql">Raw SQL being executed. Note that the column names in the SELECT statement should be ordered so that the columns
        /// associated with M1 are listed first before the column names for M2, then M3, then M4. Otherwise, the splitOn parameter will not be 
        /// able to split the data into four separate models properly.</param>
        /// <param name="transformFn">Function with four parameters that takes the initial query results and transforms it into a list
        /// of MResult items.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="splitOn">The cutoff point within the SQL query between M1's, M2's, M3's, and M4's columns within the SQL query's SELECT 
        /// statement.</param>
        /// <returns>List of MResult items.</returns>
        public async Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, M4, MResult>(string sql, Func<M1, M2, M3, M4, MResult> transformFn,
            object parameters = null, string splitOn = "Id")
        {
            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync(sql, transformFn, parameters, splitOn: splitOn);
            }
        }

        /// <summary>
        /// Gets items of the given MResult model, which is the result of transforming the SQL query results using the transform 
        /// function, or transformFn.
        /// </summary>
        /// <typeparam name="M1">Primary model type within the SQL query.</typeparam>
        /// <typeparam name="M2">Secondary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M3">Tertiary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M4">Quarternary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="M5">Quinary model type within the SQL query (typically a joined table).</typeparam>
        /// <typeparam name="MResult">Model type which corresponds to the final return type of the query (in enumerable form).</typeparam>
        /// <param name="sql">Raw SQL being executed. Note that the column names in the SELECT statement should be ordered so that the columns
        /// associated with M1 are listed first before the column names for M2, then M3, then M4, then M5. Otherwise, the splitOn parameter will not be 
        /// able to split the data into five separate models properly.</param>
        /// <param name="transformFn">Function with four parameters that takes the initial query results and transforms it into a list
        /// of MResult items.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="splitOn">The cutoff point within the SQL query between M1's, M2's, M3's, M4's, and M5's columns within the SQL query's SELECT 
        /// statement.</param>
        /// <returns>List of MResult items.</returns>
        public async Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, M4, M5, MResult>(string sql, Func<M1, M2, M3, M4, M5, MResult> transformFn,
            object parameters = null, string splitOn = "Id")
        {
            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync(sql, transformFn, parameters, splitOn: splitOn);
            }
        }

        /// <summary>Passes raw SQL to the database and returns the resulting data.</summary>
        /// <typeparam name="M">Model type to contain query results.</typeparam>
        /// <param name="sql">Raw SQL being executed.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Results of raw SQL query.</returns>
        public async Task<IEnumerable<M>> RawQuery<M>(string sql, object parameters = null, IDbTransaction transaction = null)
        {
            // Doesn't close connection automatically
            if (transaction != null)
            {
                return await transaction.Connection.QueryAsync<M>(sql, parameters, transaction: transaction);
            }

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync<M>(sql, parameters);
            }
        }

        #endregion


        #region Change Database

        /// <summary>Inserts a single item of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters of the item being inserted into the given model's associated table.</param>
        /// <param name="outputResults">Adds OUTPUT INSERTED to query if true (won't work well with tables containing triggers).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Model containing the item that was inserted into the database.</returns>
        public async Task<M> Insert<M>(object parameters, bool outputResults = true, IDbTransaction transaction = null) where M : IModel
        {
            // Initializes the column => property mappings and Dapper parameters
            Dictionary<string, string> mapping = GetParameterMappings<M>(parameters, out HashSet<string> listParameters);

            string sql = SQLGenerator.GenerateInsertSQL(tableMap[typeof(M)], mapping, outputResults);

            // Doesn't close connection automatically
            if (transaction != null)
            {
                if (outputResults)
                {
                    return await transaction.Connection.QuerySingleAsync<M>(sql, parameters, transaction: transaction);
                }
                else
                {
                    await transaction.Connection.ExecuteAsync(sql, parameters, transaction: transaction);
                    return default;
                }
            }

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                if (outputResults)
                {
                    return await db.QuerySingleAsync<M>(sql, parameters);
                }
                else
                {
                    await db.ExecuteAsync(sql, parameters);
                    return default;
                }
            }
        }

        /// <summary>Updates a single item of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Updated parameters of the given model type (must contain selector parameter values).</param>
        /// <param name="selectorParam">Parameters used to determine which database item to update (usually ids).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Model containing the item that was updated within the database.</returns>
        public async Task<M> Update<M>(object parameters, IEnumerable<string> selectorParam, IDbTransaction transaction = null) where M : IModel
        {
            // Initializes the column => property mappings and Dapper parameters
            Dictionary<string, string> mapping = GetParameterMappings<M>(parameters, out HashSet<string> listParameters);

            // Determines database columns to use for WHERE clause
            var columns = selectorParam.Select(prop => propertyToColumnMap[typeof(M)][prop]);

            string sql = SQLGenerator.GenerateUpdateSQL(tableMap[typeof(M)], mapping, columns, listParameters);

            // Doesn't close connection automatically
            if (transaction != null)
            {
                return await transaction.Connection.QuerySingleOrDefaultAsync<M>(sql, parameters, transaction: transaction);
            }

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QuerySingleOrDefaultAsync<M>(sql, parameters);
            }
        }

        /// <summary>Deletes item(s) of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to determine which database item to delete (usually ids).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Number of rows deleted.</returns>
        public async Task<int> Delete<M>(object parameters, IDbTransaction transaction = null) where M : IModel
        {
            // Initializes the column => property mappings and Dapper parameters
            Dictionary<string, string> mapping = GetParameterMappings<M>(parameters, out HashSet<string> listParameters);

            string sql = SQLGenerator.GenerateDeleteSQL(tableMap[typeof(M)], mapping, listParameters);

            // Doesn't close connection automatically
            if (transaction != null)
            {
                return await transaction.Connection.ExecuteAsync(sql, parameters, transaction: transaction);
            }

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.ExecuteAsync(sql, parameters);
            }
        }

        /// <summary>Executes database stored procedure.</summary>
        /// <typeparam name="M">Model type to contain stored procedure results.</typeparam>
        /// <param name="procedureName">Name of the stored procedure within the database.</param>
        /// <param name="parameters">Parameters to pass to the stored procedure.</param>
        /// <param name="tran">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Results of the stored procedure, encapsulated within the given model type.</returns>
        public async Task<IEnumerable<M>> ExecuteStoredProcedure<M>(string procedureName, object parameters, IDbTransaction transaction = null)
        {
            // Doesn't close connection automatically
            if (transaction != null)
            {
                return await transaction.Connection.QueryAsync<M>(procedureName, parameters, commandType: CommandType.StoredProcedure,
                    transaction: transaction);
            }

            using (IDbConnection db = new SqlConnection(dbConnectionString))
            {
                return await db.QueryAsync<M>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        #endregion
    }
}
