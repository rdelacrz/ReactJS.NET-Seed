using Logic.Models;
using Logic.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Services
{
    public class ConfigService: IConfigService
    {
        private readonly IDatabaseService _databaseService;

        /// <summary>
        /// Initializes the services associated with getting global application configuration data.
        /// </summary>
        /// <param name="databaseService">Database service responsible for accessing database.</param>
        public ConfigService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<IEnumerable<OptionValueSet>> GetLookup(string fieldName, bool activeOnly = true)
        {
            var lookup = await _databaseService.GetItems<Lookup>(
                new { FieldName = fieldName },
                new string[] { "ShortDesc" },
                selectParameters: new string[] { "LookupKey", "ShortDesc", "DelFlag" }
            );
            var options = lookup
                .Where(l => !activeOnly || l.DelFlag.GetValueOrDefault(false) == false)
                .Select((l, i) => new OptionValueSet { ValueCode = l.LookupKey, ValueName = l.ShortDesc, OrderSequence = i + 1 });
            return options;
        }
    }
}
