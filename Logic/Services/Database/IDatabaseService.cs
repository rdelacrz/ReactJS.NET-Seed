using Logic.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Logic.Services
{
    public interface IDatabaseService
    {

        /// <summary>
        /// Gets database connection.
        /// </summary>
        /// <returns>Database connection.</returns>
        IDbConnection GetDbConnection();

        /// <summary>Gets all data of the given model type from the given table.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="orderByProperties">Model properties to order a query's results by, in list order (may be null).</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <returns>All model objects with data queried from the database table.</returns>
        Task<IEnumerable<M>> GetAll<M>(IEnumerable<string> orderByProperties = null, IEnumerable<bool> orderByASC = null) where M : IModel;

        /// <summary>
        /// Gets a single item of the given model type based on the parameters used to filter
        /// the results of the query.
        /// </summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to filter results for a single item.</param>
        /// <param name="orderByProperties">Determines ordering of items from which to retrieve first item.</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <returns>Model object with data queried from the database table.</returns>
        Task<M> GetItem<M>(object parameters, IEnumerable<string> orderByProperties = null, IEnumerable<bool> orderByASC = null) where M : IModel;

        /// <summary>
        /// Gets items of the given model type based on the parameters used to filter the results 
        /// of the query, as well as the various ORDER BY properties used to sort the data.
        /// </summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to filter results for a given set of items (can be null).</param>
        /// <param name="orderByProperties">Model properties to order a query's results by, in list order (may be null).</param>
        /// <param name="orderByASC">List of boolean values corresponding to each orderBy value. 
        /// True if ORDER BY ascending, false if ORDER BY descending (will be true by default if nothing is provided).</param>
        /// <param name="topItems">Top number of items for SELECT statement (if any).</param>
        /// <returns>List of model objects with data queried from the database table.</returns>
        Task<IEnumerable<M>> GetItems<M>(object parameters = null, IEnumerable<string> orderByProperties = null,
            IEnumerable<bool> orderByASC = null, int? topItems = null) where M : IModel;

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
        Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, MResult>(string sql, Func<M1, M2, MResult> transformFn,
           object parameters = null, string splitOn = "Id");

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
        Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, MResult>(string sql, Func<M1, M2, M3, MResult> transformFn,
            object parameters = null, string splitOn = "Id");

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
        Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, M4, MResult>(string sql, Func<M1, M2, M3, M4, MResult> transformFn,
            object parameters = null, string splitOn = "Id");

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
        Task<IEnumerable<MResult>> GetTransformedItems<M1, M2, M3, M4, M5, MResult>(string sql, Func<M1, M2, M3, M4, M5, MResult> transformFn,
            object parameters = null, string splitOn = "Id");

        /// <summary>Inserts a single item of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters of the item being inserted into the given model's associated table.</param>
        /// <param name="outputResults">Adds OUTPUT INSERTED to query if true (won't work well with tables containing triggers).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Model containing the item that was inserted into the database.</returns>
        Task<M> Insert<M>(object parameters, bool outputResults = true, IDbTransaction transaction = null) where M : IModel;

        /// <summary>Updates a single item of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Updated parameters of the given model type (must contain selector parameter values).</param>
        /// <param name="selectorParam">Parameters used to determine which database item to update (usually ids).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// /// <returns>Model containing the item that was updated within the database.</returns>
        Task<M> Update<M>(object parameters, IEnumerable<string> selectorParam, IDbTransaction transaction = null) where M : IModel;

        /// <summary>Deletes item(s) of the given model type with the given parameter values.</summary>
        /// <typeparam name="M">Type of model used as the basis for the SQL string.</typeparam>
        /// <param name="parameters">Parameters used to determine which database item to delete (usually ids).</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        Task<int> Delete<M>(object parameters, IDbTransaction transaction = null) where M : IModel;

        /// <summary>Passes raw SQL to the database and returns the resulting data.</summary>
        /// <typeparam name="M">Model type to contain query results.</typeparam>
        /// <param name="sql">Raw SQL being executed.</param>
        /// <param name="parameters">Parameters to pass within the SQL query.</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Results of raw SQL query.</returns>
        Task<IEnumerable<M>> RawQuery<M>(string sql, object parameters = null, IDbTransaction transaction = null);

        /// <summary>Executes database stored procedure.</summary>
        /// <typeparam name="M">Model type to contain stored procedure results.</typeparam>
        /// <param name="procedureName">Name of the stored procedure within the database.</param>
        /// <param name="parameters">Parameters to pass to the stored procedure.</param>
        /// <param name="transaction">Personal transaction object that won't be closed automatically by this function.</param>
        /// <returns>Results of the stored procedure, encapsulated within the given model type.</returns>
        Task<IEnumerable<M>> ExecuteStoredProcedure<M>(string procedureName, object parameters, IDbTransaction transaction = null);
    }
}
