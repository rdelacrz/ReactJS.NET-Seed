using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Logic.Models
{
	[Table(TABLE_NAME)]
	public class Lookup : IModel
	{
		public const string TABLE_NAME = "lookupValueSet";

		#region Instance Properties

		[Column("fieldName")]
		public string FieldName { get; set; }

		[Column("lookupGroup")]
		public string LookupGroup { get; set; }

		[Column("lookupKey")]
		public string LookupKey { get; set; }

		[Column("shortDesc")]
		public string ShortDesc { get; set; }

		[Column("longDesc")]
		public string LongDesc { get; set; }

		[Column("delFlag")]
		public bool? DelFlag { get; set; }

		[Column("modDate")]
		public DateTime? ModDate { get; set; }

		[Column("modID")]
		public int? ModId { get; set; }

		#endregion

		#region Bulk Properties getter

		public Dictionary<string, object> GetProperties()
		{
			return new Dictionary<string, object>()
			{
				{"FieldName", FieldName},
				{"LookupGroup", LookupGroup},
				{"LookupKey", LookupKey},
				{"ShortDesc", ShortDesc},
				{"LongDesc", LongDesc},
				{"DelFlag", DelFlag},
				{"ModDate", ModDate},
				{"ModId", ModId}
			};
		}

		#endregion

		#region Bulk Properties setter

		public void SetProperties(Dictionary<string, object> parameters)
		{
			if (parameters.ContainsKey("FieldName"))
				FieldName = (string)parameters["FieldName"];
			if (parameters.ContainsKey("LookupGroup"))
				LookupGroup = (string)parameters["LookupGroup"];
			if (parameters.ContainsKey("LookupKey"))
				LookupKey = (string)parameters["LookupKey"];
			if (parameters.ContainsKey("ShortDesc"))
				ShortDesc = (string)parameters["ShortDesc"];
			if (parameters.ContainsKey("LongDesc"))
				LongDesc = (string)parameters["LongDesc"];
			if (parameters.ContainsKey("DelFlag"))
				DelFlag = (bool?)parameters["DelFlag"];
			if (parameters.ContainsKey("ModDate"))
				ModDate = (DateTime?)parameters["ModDate"];
			if (parameters.ContainsKey("ModId"))
				ModId = (int?)parameters["ModId"];
		}

		#endregion
	}
}
