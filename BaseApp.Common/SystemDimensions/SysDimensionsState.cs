using System;
using System.Collections.Generic;
using System.Linq;
using BaseApp.Common.Interfaces;
using BaseApp.DbEnums;

namespace BaseApp.SystemDimensions
{
	public class SysDimensionsState : Dictionary<SysDimension, Object>
	{
		public SysDimensionsState() : base()
		{
		}
        
	    public IPublicLegalFormation PublicLegalFormation
	    {
	        get
	        {
	            if (Keys.Contains(SysDimension.PublicLegalFormation))
	                return this[SysDimension.PublicLegalFormation] as IPublicLegalFormation;
	            return null;
	        }
            set { this[SysDimension.PublicLegalFormation] = value; }
        
	    }

		public IBudget Budget
		{
			get
			{
				if (Keys.Contains(SysDimension.Budget))
					return this[SysDimension.Budget] as IBudget;
				return null;
			}
			set { this[SysDimension.Budget] = value; }

		}

		public IVersion Version
        {
            get
            {
                if (Keys.Contains(SysDimension.Version))
                    return this[SysDimension.Version] as IVersion ;
                return null;
            }
            set {   
                this[SysDimension.Version] = value; }

        }

      
	    /// <summary>
        /// Если хотя бы одно системное измерение не имеет значение, считаем систменые измерения не заданными - пустыми
        /// </summary>
        public bool IsEmpty 
        {
            get
            {
                return PublicLegalFormation == null
                       || Budget == null
                       || Version == null;
            }
        }

	    /// <summary>
	    /// Получить текущие настройки в виде "ППО: %PPO%, Бюджет: %Budget%, Версия: %Version%"
	    /// </summary>
	    /// <returns></returns>
	    public string GetCaption()
	    {
	        return String.Format("ППО: {0}, Бюджет: {1}, Версия: {2}", PublicLegalFormation.Caption, Budget.Caption, Version.Caption);
	    }

	}
}
