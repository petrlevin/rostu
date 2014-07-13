using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference
{

    public partial class DocStatus : ReferenceEntity
    {

        public override string ToString()
        {
            return Caption;
        }


        /// <summary>
        /// Черновик
        /// </summary>
        public static int Draft = -2147483615;

        /// <summary>
        /// Проект
        /// </summary>
        public static int Project = -2147483614;

        /// <summary>
        /// Согласование
        /// </summary>
        public static int Checking = -1879048164;

        /// <summary>
        /// Утвержден
        /// </summary>
        public static int Approved = -2147483613;
        
        /// <summary>
        /// Прекращен
        /// </summary>
        public static int Terminated = -2147483612;

        /// <summary>
        /// Отказан
        /// </summary>
        public static int Denied = -2147483611;

        /// <summary>
        /// Изменен
        /// </summary>
        public static int Changed = -2147483610;

        /// <summary>
        /// Архив
        /// </summary>
        public static int Archive = -2147483609;

        /// <summary>
        /// Формирование документов
        /// </summary>
        public static int CreateDocs = -2147483608;

        /// <summary>
        /// Включен в свод
        /// </summary>
        public static int IncludedInSetOf = -2013265889;

        /// <summary>
        /// Обработан
        /// </summary>
        public static int Processed = -2013265887;

        /// <summary>
        /// Завершен
        /// </summary>
        public static int Completed = -2013265886;
    }
}

