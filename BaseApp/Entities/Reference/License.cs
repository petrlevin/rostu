using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Platform.Application.Common;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Crypto;
using Platform.BusinessLogic.EntityTypes;
using Platform.PrimaryEntities.DbEnums;
using Platform.Utils.Common;

namespace BaseApp.Reference
{
	public partial class License : ReferenceEntity
	{

		/// <summary>
		/// Идентификатор
		/// </summary>
		public override Int32 Id { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Ключ
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// Количество пользователей
		/// </summary>
		public Int16? UserCount
		{
			get
			{
				if (_userCount == -1)
					_setUserCount();
				return _userCount;
			}
		    set { }
		}

		private Int16 _userCount=-1;

		private void _setUserCount()
		{
			if (string.IsNullOrEmpty(Key) || !Int16.TryParse(Key.Decrypt().Split(new char[] { ',' })[1], out _userCount))
				_userCount = -1;
		}

		/// <summary>
		/// Дата окончения действия лицензии
		/// </summary>
		public DateTime? EndDate
		{
			get
			{
				if (_endDate == DateTime.MinValue)
					_setEndDate();
				return _endDate;
			}
		    set { }
		}

		private DateTime _endDate = DateTime.MinValue;

		private void _setEndDate()
		{
			if (string.IsNullOrEmpty(Key) || !DateTime.TryParse(Key.Decrypt().Split(new char[] { ',' })[2], out _endDate))
				_endDate = DateTime.MinValue;
		}

		/// <summary>
		/// ППО
		/// </summary>
		public string PublicLegalFormation
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_publicLegalFormation))
					_setPublicLegalFormation();
				return _publicLegalFormation;
			}
			set { ; }
		}

		private string _publicLegalFormation;

		private void _setPublicLegalFormation()
		{
			if (!string.IsNullOrEmpty(Key))
				_publicLegalFormation = Key.Decrypt().Split(new char[] {','})[0];
		}

		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public License()
		{
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public override int EntityId
		{
			get { return -2080374741; }
		}

		/// <summary>
		/// Идентификатор типа сущности
		/// </summary>
		public new static int EntityIdStatic
		{
			get { return -2080374741; }
		}

		/// <summary>
		/// Русское наименование типа сущности
		/// </summary>
		public override string EntityCaption
		{
			get { return "Лицензии"; }
		}

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator : IBeforeAplicationStart
		{
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType, -2080374741);
			}
		}

		/// <summary>
		/// Статус
		/// </summary>
		public byte IdRefStatus { get; set; }
		[NotMapped]
		public virtual RefStatus RefStatus
		{
			get { return (RefStatus)this.IdRefStatus; }
			set { this.IdRefStatus = (byte)value; }
		}

		private ICollection<BaseApp.Reference.User> _mlUsers;
		[JsonIgnoreForException]
		public virtual ICollection<BaseApp.Reference.User> Users
		{
			get { return _mlUsers ?? (_mlUsers = new List<User>()); }
			set { _mlUsers = value; }
		}

		[Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 100)]
		[ControlInitial(ExcludeFromSetup = true)]
		public void ControlAlways(DataContext context)
		{
			bool valid = this.Users.Count() <= UserCount;
			if (!valid)
				Controls.Throw("Для Лицензии '" + Caption + "' максимально возможно " + UserCount + " пользователей");
		}

	}
}
