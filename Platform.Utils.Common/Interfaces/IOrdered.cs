using System;
using System.Collections.Generic;

namespace Platform.Utils.Common.Interfaces
{
	/// <summary>
	/// Декоратор, для которого важен порядок применения
	/// </summary>
	public interface IOrdered
	{
		/// <summary>
		/// Список декораторов, до которых должен быть применен данный
		/// </summary>
		IEnumerable<Type> Before { get; }

		/// <summary>
		/// Список декораторов, после которых должен быть применен данный
		/// </summary>
		IEnumerable<Type> After { get; }

        /// <summary>
        /// Если декоратор хочет быть первым после указанных в списке <see cref="After"/> он должен вернуть 'true'
        /// </summary>
        Order WantBe { get; }

	}
}
