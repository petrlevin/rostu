using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platform.Utils.Extensions;
using Platforms.Tests.Common;
using Sbor.DbEnums;
using Sbor.Document;
using Sbor.Reference;


namespace Platform.BusinessLogic.Tests
{
	[TestFixture]
	internal class CloneTest : SqlTests
	{
		[SetUp]
		public void SetUp()
		{
			IUnityContainer uc = new UnityContainer();
			DependencyInjection.RegisterIn(uc, true, false, connectionString);
			IoC.InitWith(new DependencyResolverBase(uc));
		}

		[Test]
		public void Test()
		{
			using (Sbor.DataContext db = new Sbor.DataContext())
			{
				/* Документ с вложенными ТЧ
				ElementTypeSystemGoal elementTypeSystemGoal = db.ElementTypeSystemGoal.Create();
				elementTypeSystemGoal.Caption = "caption";
				elementTypeSystemGoal.IdRefStatus = (byte) RefStatus.Work;
				db.ElementTypeSystemGoal.Add(elementTypeSystemGoal);
				db.SaveChanges();

				SystemGoal systemGoal = db.SystemGoal.Create();
				systemGoal.Code = "1";
				systemGoal.Caption = "caption1";
				systemGoal.DateStart = DateTime.Now;
				systemGoal.DateEnd = DateTime.Now.AddYears(1);
				systemGoal.IdElementTypeSystemGoal = elementTypeSystemGoal.Id;
				systemGoal.IdDocType_CommitDoc = 603979767;
				systemGoal.IdDocType_ImplementDoc = 603979767;
				systemGoal.IdRefStatus = (byte)RefStatus.Work;
				db.SystemGoal.Add(systemGoal);

				GoalIndicator goalIndicator=new GoalIndicator();
				goalIndicator.Code = "1";
				goalIndicator.Caption = "caption1";
				goalIndicator.IdRefStatus = (byte)RefStatus.Work;
				goalIndicator.IdUnitDimension = 268435452;
				goalIndicator.IdTermsOfPerception = (byte) TermsOfPerception.LessBetter;
				db.GoalIndicator.Add(goalIndicator);
				db.SaveChanges();

				DocumentsOfSED doc = db.DocumentsOfSED.Create();
				doc.IdDocType = 603979767;
				doc.Number = "1";
				doc.Date = DateTime.Now;
				doc.DateStart = DateTime.Now;
				doc.DateEnd = DateTime.Now.AddYears(3);
				doc.IdDocStatus = 1;
				doc.Caption = "qwerty";

				DocumentsOfSED_ItemsSystemGoal i1 = new DocumentsOfSED_ItemsSystemGoal();
				i1.IdSystemGoal = systemGoal.Id;
				i1.IsOtherDocSG = false;
				doc.ItemsSystemGoal.Add(i1);

				DocumentsOfSED_GoalIndicator i11 = new DocumentsOfSED_GoalIndicator();
				i11.IdGoalIndicator = goalIndicator.Id;
				i11.IdHierarchyPeriod = 268435452;
				i11.Value = (decimal)1.11;

				DocumentsOfSED_GoalIndicator i12 = new DocumentsOfSED_GoalIndicator();
				i12.IdGoalIndicator = goalIndicator.Id;
				i12.IdHierarchyPeriod = 268435452;
				i12.Value = (decimal)2.22;

				i1.TpDocumentsOfSED_GoalIndicator.Add(i11);
				i1.TpDocumentsOfSED_GoalIndicator.Add(i12);

				DocumentsOfSED_ItemsSystemGoal i2 = new DocumentsOfSED_ItemsSystemGoal();
				i2.IdSystemGoal = systemGoal.Id;
				i2.IsOtherDocSG = true;
				doc.ItemsSystemGoal.Add(i2);

				DocumentsOfSED_ItemsSystemGoal i3 = new DocumentsOfSED_ItemsSystemGoal();
				i3.IdSystemGoal = systemGoal.Id;
				i3.IsOtherDocSG = false;
				doc.ItemsSystemGoal.Add(i3);

				db.Entry(doc).State=EntityState.Added;
				db.SaveChanges();
				return;
				*/

				/*
				db.IndicatorActivity.Add(
					new IndicatorActivity
					{
						IdIndicatorActivityType = (byte)IndicatorActivityType.QualityIndicator,
						Caption = "caption1",
						IdUnitDimension = 268435452,
						IdTermsOfPerception = (byte)TermsOfPerception.LessBetter,
						IdRefStatus = (byte)RefStatus.Work
					});
				db.IndicatorActivity.Add(
					new IndicatorActivity
					{
						IdIndicatorActivityType = (byte)IndicatorActivityType.QualityIndicator,
						Caption = "caption2",
						IdUnitDimension = 268435452,
						IdTermsOfPerception = (byte)TermsOfPerception.LessBetter,
						IdRefStatus = (byte)RefStatus.Work
					});
				db.IndicatorActivity.Add(
					new IndicatorActivity
					{
						IdIndicatorActivityType = (byte)IndicatorActivityType.QualityIndicator,
						Caption = "caption3",
						IdUnitDimension = 268435452,
						IdTermsOfPerception = (byte)TermsOfPerception.LessBetter,
						IdRefStatus = (byte)RefStatus.Work
					});
				db.SaveChanges();
				IndicatorActivity[] indicatorActivities = db.IndicatorActivity.ToArray();

				RegisterActivity registerActivity = new RegisterActivity
					{IdDocStatus = 1, IdDocType = 603979767, Number = "1", Date = DateTime.Now};

				registerActivity.MlRegisterActivity_IndicatorActivity.Add(indicatorActivities[0]);
				registerActivity.MlRegisterActivity_IndicatorActivity.Add(indicatorActivities[1]);
				registerActivity.MlRegisterActivity_IndicatorActivity.Add(indicatorActivities[2]);
				db.Entry(registerActivity).State = EntityState.Added;
				db.SaveChanges();
				*/
				//StateProgram_SystemGoalElement ttt=new StateProgram_SystemGoalElement();
				RegisterActivity sourceItem = db.RegisterActivity.First();
				//var targetTest = Activator.CreateInstance(sourceItem.GetType());
				Entity entity = Objects.ById<Entity>(sourceItem.EntityId);
				Clone test = new Clone(sourceItem, entity);
				object targetTest = test.GetResult();
				db.Entry(targetTest).State=EntityState.Added;
				db.SaveChanges();
				return;
				DocumentsOfSED targetItem = new DocumentsOfSED
					{
						IdDocType = 603979767,
						Number = "2",
						Date = DateTime.Now,
						DateStart = DateTime.Now,
						DateEnd = DateTime.Now.AddYears(3),
						IdDocStatus = 1,
						Caption = "qwerty2"
					};
				//Entity entity = Objects.ById<Entity>(sourceItem.EntityId);
				List<EntityField> fieldsTp =
					entity.Fields.Cast<EntityField>().Where(
						a => a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue).ToList();
				List<int> entitiesId = fieldsTp.Select(a => a.IdEntityLink.Value).ToList();

				List<EntityField> fieldsTpStart =
					fieldsTp.Where(
						a =>
						!a.EntityLink.Fields.Any(
							b => b.IdEntityLink.HasValue && entitiesId.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value))).
						ToList();
				foreach (EntityField entityField in fieldsTpStart)
				{
					//имеем ТЧ, надо пробежаться по каждому элементу и скопировать, а если есть подчиненные ТЧ, то и их элементы
					//и еще надо учесть наличие иерархии
					List<EntityField> listCopyFields =
						entityField.EntityLink.RealFields.Where(a => !a.IdCalculatedFieldType.HasValue).Cast<EntityField>().ToList();
					//получили коллекцию, надо что то с ней сделать
					//1. отдать в другой метод с fieldsTp исключая fieldsTpStart и там разбираться с иерархие и подчиненными ТЧ
					IEnumerable sourceCollection = (IEnumerable) sourceItem.GetValue(entityField.Name);
					List<EntityField> childrensFieldsTp = fieldsTp.Where(a => !fieldsTpStart.Select(b => b.Name).Contains(a.Name)).ToList();

					List<object> targetCollection = _cloneCollection(sourceCollection, childrensFieldsTp, listCopyFields, new List<string> { "Id, IdOwner" });
					targetItem.SetValue(entityField.Name, targetCollection);
				}
			
				//db.DocumentsOfSED.Add(targetItem);
				//db.SaveChanges();

			}
		}

		private object _clone(object item, List<EntityField> copyProperty, List<string> excludeProperty = null)
		{
			object result = new object();
			var list =
				item.GetType().GetProperties().Where(
					a => copyProperty.Select(b => b.Name).Contains(a.Name, StringComparer.OrdinalIgnoreCase));
			foreach (PropertyInfo property in list)
			{
				if (excludeProperty==null || (excludeProperty!=null && excludeProperty.All(a => a != property.Name)))
				{
					result.SetValue(property.Name, item.GetValue(property.Name));
				}
			}
			return result;
		}

		private List<object> _cloneCollection(IEnumerable items, List<EntityField> childrensFieldTp, List<EntityField> copyProperty, List<string> excludeProperty = null)
		{
			//для иерархических копировать только те, у кого поле иерархии==null, остальные рекурсивно вставлять в коллекцию childrens	
			List<object> result = new List<object>();
			if (copyProperty.Any(a => a.IdEntityLink.HasValue && a.IdEntity == a.IdEntityLink.Value))
			{
				//иерархия
			}
			else
			{
				//не иерархия
				foreach (object item in items)
				{
					result.Add(_clone(item, copyProperty, excludeProperty));
				}
			}
			return result;
		}
		
	}

	/// <summary>
	/// 
	/// </summary>
	public class Clone
	{
		private readonly Entity _entity;

		private readonly object _source;

		private object _target;

		public Clone(object source, Entity entity)
		{
			_source = source;
			_entity = entity;
			_target = Activator.CreateInstance(source.GetType());
		}

		public object GetResult()
		{
			_cloneBaseObject();
			_cloneAllTp();
			_cloneAllMl();
			return _target;
		}

		private void _cloneBaseObject()
		{
			List<string> fields=_entity.RealFields.Where(a => !a.IdCalculatedFieldType.HasValue && a.Name!="id").Select(a=> a.Name).ToList();
			PropertyInfo[] propertyInfos = _source.GetType().GetProperties().Where(a=> fields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToArray();
			foreach (PropertyInfo property in propertyInfos)
			{
				_target.SetValue(property.Name, _source.GetValue(property.Name));
			}
		}

		private void _cloneAllTp()
		{
			List<EntityField> fieldsTp =
				_entity.Fields.Cast<EntityField>().Where(
					a => a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue).ToList();
			List<int> entitiesId = fieldsTp.Select(a => a.IdEntityLink.Value).ToList();

			List<EntityField> fieldsTpStart =
				fieldsTp.Where(
					a =>
					!a.EntityLink.Fields.Any(
						b => b.IdEntityLink.HasValue && entitiesId.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value))).
					ToList();
			foreach (EntityField entityField in fieldsTpStart)
			{
				string propertyName = entityField.Name.FirstUpper();
				PropertyInfo propertyInfo = _source.GetType().GetProperty(propertyName);
				IEnumerable source = (IEnumerable)_source.GetValue(propertyName);
				IEnumerable target = (IEnumerable)_target.GetValue(propertyName);
				EntityField parentField =
					entityField.EntityLink.Fields.Cast<EntityField>().FirstOrDefault(
						a => a.IdEntityLink.HasValue && a.IdEntityLink == a.IdEntity);
				List<string> excludedField = new List<string> {"id", "idOwner"};
				if (parentField!=null)
				{
					source = _filterByParent(source, parentField, null);
					excludedField.Clear();
					_cloneHierarchyTp(source, target, propertyInfo, entityField, excludedField, parentField);
				}
				else
				{
					_cloneTp(source, target, propertyInfo, entityField, excludedField);
				}
			}
		}

		private IEnumerable _filterByParent(IEnumerable value, EntityField parentField, Int64? idParent)
		{
			List<object> result = new List<object>();
			PropertyInfo parentProperty = null;
			foreach (var item in value)
			{
				if (parentProperty == null)
					parentProperty = item.GetType().GetProperty(parentField.Name.FirstUpper());
				if ((Int64?)item.GetValue(parentField.Name.FirstUpper()) == idParent)
					result.Add(item);
			}
			return result;
		}

		private void _cloneHierarchyTp(IEnumerable source, IEnumerable target, PropertyInfo propertyInfo, EntityField entityField, IEnumerable<string> excludedField, EntityField parentField)
		{
			List<string> listFields =
				entityField.EntityLink.RealFields.Where(
					a => !a.IdCalculatedFieldType.HasValue && !excludedField.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).Select
					(a => a.Name).ToList();
			List<PropertyInfo> copyProperties = null;
			foreach (var item in source)
			{
				if (copyProperties == null)
					copyProperties =
						item.GetType().GetProperties().Where(a => listFields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToList();
				object newItem = _cloneItem(item, copyProperties);
				propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] {newItem});
				IEnumerable internalSource = (IEnumerable) item.GetValue("ChildrenBy" + parentField.Name);
				if (internalSource!=null && (internalSource as IList).Count>0)
				{
					IEnumerable internalTarget = (IEnumerable)newItem.GetValue("ChildrenBy" + parentField.Name);
					PropertyInfo internalPropertyInfo = item.GetType().GetProperty("ChildrenBy" + parentField.Name);
					List<string> internalExcludedField = new List<string>();
					internalExcludedField.AddRange(excludedField);
					if (!excludedField.Contains(parentField.Name))
					{
						internalExcludedField.Add(parentField.Name);
					}
					_cloneHierarchyTp(internalSource, internalTarget, internalPropertyInfo, entityField, internalExcludedField, parentField);
				}
			}
		}


		private void _cloneTp(IEnumerable source, IEnumerable target, PropertyInfo propertyInfo, EntityField entityField, IEnumerable<string> excludedField)
		{
			List<string> listFields =
				entityField.EntityLink.RealFields.Where(
					a => !a.IdCalculatedFieldType.HasValue && !excludedField.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).Select
					(a => a.Name).ToList();
			
			List<EntityField> linkTp =
				_entity.Fields.Cast<EntityField>().Where(
					a =>
					a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue &&
					a.EntityLink.Fields.Any(b => b.IdEntityLink.HasValue && b.IdEntityLink.Value == entityField.IdEntityLink.Value)).
					ToList();
			List<PropertyInfo> copyProperties = null;
			foreach (var item in source)
			{
				if (copyProperties==null)
					copyProperties =
						item.GetType().GetProperties().Where(a => listFields.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToList();
				object newItem = _cloneItem(item, copyProperties);
				propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] {newItem});
				if (linkTp.Count>0)
				{
					foreach (EntityField internalField in linkTp)
					{
						PropertyInfo internalPropertyInfo = item.GetType().GetProperty("Tp" + internalField.EntityLink.Name);
						if (internalPropertyInfo != null)
						{
							IEnumerable internalSource = (IEnumerable)item.GetValue(internalPropertyInfo.Name);

							if (internalSource != null && (internalSource as IList).Count>0)
							{
								IEnumerable internalTarget = (IEnumerable)newItem.GetValue(internalPropertyInfo.Name);
								List<string> internalExcludedField = new List<string>();
								internalExcludedField.AddRange(excludedField);
								internalExcludedField.Add("idMaster");
								_cloneTp(internalSource, internalTarget, internalPropertyInfo, internalField, internalExcludedField);
							}
						}
					}
					
				}
			}
		}

		private object _cloneItem(object item, IEnumerable<PropertyInfo> propertyInfos)
		{
			object result = Activator.CreateInstance(item.GetType());
			foreach (PropertyInfo property in propertyInfos)
			{
				result.SetValue(property.Name, item.GetValue(property.Name));
			}
			return result;
		}


		private void _cloneAllMl()
		{
			List<EntityField> fieldsMl =
				_entity.Fields.Cast<EntityField>().Where(
					a => a.EntityFieldType == EntityFieldType.Multilink && a.IdEntityLink.HasValue).ToList();
			foreach (EntityField entityField in fieldsMl)
			{
				string propertyName = entityField.Name.FirstUpper();
				IEnumerable source = (IEnumerable)_source.GetValue(propertyName);
				if (source != null && (source as IList).Count > 0)
				{
					IEnumerable target = (IEnumerable)_target.GetValue(propertyName);
					PropertyInfo propertyInfo = _source.GetType().GetProperty(propertyName);
					foreach (var item in source)
					{
						propertyInfo.PropertyType.GetMethod("Add").Invoke(target, new[] {item});
					}
				}
			}
		}
	}
}
