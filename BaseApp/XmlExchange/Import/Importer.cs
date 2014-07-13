using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Reference;
using System.Xml;
using Platform.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using IsolationLevel = System.Transactions.IsolationLevel;
using TransactionScope = Platform.BusinessLogic.DataAccess.TransactionScope;


namespace BaseApp.XmlExchange.Import
{
	public class Importer
	{
		private readonly TemplateImport _templateImport;
		private DataContext _dataContext;
		private XDocument _xDocument;

		public Importer(TemplateImport templateImport, XDocument xDocument, DbContext dbContext = null)
		{
			_templateImport = templateImport;
			_xDocument = xDocument;
			_dataContext = (dbContext ?? IoC.Resolve<DbContext>()).Cast<DataContext>();
		}

		public void Execute()
		{
			if (_xDocument == null || _xDocument.Root == null)
				return;
			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew,
											  new TransactionOptions() { IsolationLevel = IsolationLevel.Snapshot }))
			{
				List<string> listNameElements = _xDocument.Root.Elements().Select(a => a.Name.LocalName).Distinct().ToList();
				List<string> listNameEntities = listNameElements.Select(b => b.Split(new char[] {'.'})[1]).ToList();

				Dictionary<string, Entity> dictionaryEntities = new Dictionary<string, Entity>();
				List<Entity> listEntities =
					_dataContext.Entity.Where(a => listNameEntities.Contains(a.Name)).ToList();
				foreach (string nameElement in listNameElements)
				{
					string nameEntity = nameElement.Split(new char[] {'.'})[1];
					Entity entity = listEntities.SingleOrDefault(a => a.Name == nameEntity);
					if (entity!=null)
					{
						dictionaryEntities.Add(nameElement, entity);
					}
				}
				Dictionary<int, List<int>> insertedElement = new Dictionary<int, List<int>>();
				foreach (XElement element in _xDocument.Root.Elements())
				{
					Entity entity = dictionaryEntities[element.Name.LocalName];

				}
				scope.Complete();
			}
		}

	}
}