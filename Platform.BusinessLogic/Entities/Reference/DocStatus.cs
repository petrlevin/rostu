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
        /// ��������
        /// </summary>
        public static int Draft = -2147483615;

        /// <summary>
        /// ������
        /// </summary>
        public static int Project = -2147483614;

        /// <summary>
        /// ������������
        /// </summary>
        public static int Checking = -1879048164;

        /// <summary>
        /// ���������
        /// </summary>
        public static int Approved = -2147483613;
        
        /// <summary>
        /// ���������
        /// </summary>
        public static int Terminated = -2147483612;

        /// <summary>
        /// �������
        /// </summary>
        public static int Denied = -2147483611;

        /// <summary>
        /// �������
        /// </summary>
        public static int Changed = -2147483610;

        /// <summary>
        /// �����
        /// </summary>
        public static int Archive = -2147483609;

        /// <summary>
        /// ������������ ����������
        /// </summary>
        public static int CreateDocs = -2147483608;

        /// <summary>
        /// ������� � ����
        /// </summary>
        public static int IncludedInSetOf = -2013265889;

        /// <summary>
        /// ���������
        /// </summary>
        public static int Processed = -2013265887;

        /// <summary>
        /// ��������
        /// </summary>
        public static int Completed = -2013265886;
    }
}

