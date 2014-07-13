using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Platform.BusinessLogic.Tests
{
	[ExcludeFromCodeCoverage]
	public class SomeContext : DbContext
    {
        public SomeContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
            
        }
    }
}