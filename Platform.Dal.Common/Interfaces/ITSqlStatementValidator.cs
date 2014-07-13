using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.Dal.Common.Interfaces
{
	public interface ITSqlStatementValidator
	{
		void Validate(TSqlStatement query);
	}
}
