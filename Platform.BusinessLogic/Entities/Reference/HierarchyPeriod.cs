﻿using System;

namespace Platform.BusinessLogic.Reference
{
    public partial class HierarchyPeriod
    {
        /// <summary>
        /// проверка - текущий период пересекается с указанным
        /// </summary>
        public bool HasIntersection(HierarchyPeriod Period)
        {
            return
                (this.DateStart <= Period.DateEnd &&
                 this.DateStart >= Period.DateStart) ||
                (this.DateEnd <= Period.DateEnd &&
                 this.DateEnd >= Period.DateStart) ||
                (Period.DateStart <= this.DateEnd &&
                 Period.DateStart >= this.DateStart) ||
                (Period.DateEnd <= this.DateEnd &&
                 Period.DateEnd >= this.DateStart);
        }

        /// <summary>
        /// проверка - текущий период входит в указанные даты
        /// </summary>
        public bool HasEntrance(DateTime vDateStart, DateTime vDateEnd)
        {
            return 
                (this.DateStart <= vDateEnd &&
                     this.DateStart >= vDateStart) &&
                    (this.DateEnd <= vDateEnd &&
                     this.DateEnd >= vDateStart);
        }

        /// <summary>
        /// проверка - текущий период входит в указанные даты
        /// </summary>
        public bool HasEntrance(HierarchyPeriod Period)
        {
            return this.HasEntrance(Period.DateStart, this.DateEnd);
        }
    }
}
