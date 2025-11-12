using System;

namespace DatabaseLayer.RepositoriesDapper.Sql
{
	public static class DbQualifier
	{
		public static string Qualify(string? databaseName, string objectName, string schema = "dbo")
		{
			if (string.IsNullOrWhiteSpace(databaseName))
			{
				return objectName;
			}
			return $"[{databaseName}].{schema}.{objectName}";
		}
	}
}

