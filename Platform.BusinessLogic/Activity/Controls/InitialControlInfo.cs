using System;
using System.Reflection;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls
{
    public class InitialControlInfo :IControlInfo
    {
        public bool Enabled
        {
            get { return true; }
        }

        public bool Skippable { get; internal set; }

        public string Caption
        {
            get;
            internal set;
        }

        public string UNK
        {
            get;
            internal set;
        }

        public Int32? IdEntity
        {
            get;
            internal set;
        }

        public bool HasDbEntry
        {
            get { return false; }
        }

        public InitialControlInfo(ControlInitialAttribute attr, MemberInfo control)
        {
            UNK = attr.InitialUNK;
            Caption = attr.InitialCaption ?? control.Name;
            Skippable = attr.InitialSkippable;
        }

        private InitialControlInfo(ControlInitialForAttribute attr, MemberInfo control, Type entityType)
            : this((ControlInitialAttribute)attr, control)
        {
            IdEntity = entityType.GetEntity().Id;
        }


        public static IControlInfo Merge(ControlInitialAttribute mainAttr, ControlInitialForAttribute attr, MemberInfo control, Type entityType)
        {
            if (attr == null)
                return new InitialControlInfo(mainAttr,control);
            if (mainAttr==null)
                return new InitialControlInfo(attr, control);

            var result = new InitialControlInfo
                             {
                                 UNK = attr.InitialUNK ?? mainAttr.InitialUNK,
                                 Caption = attr.InitialCaption ?? mainAttr.InitialCaption,
                                 Skippable =
                                     attr.ActualInitialSkippable.HasValue
                                         ? attr.ActualInitialSkippable.Value
                                         : mainAttr.InitialSkippable,
                                 IdEntity = entityType.GetEntity().Id
                             };
            return result;
        }

        public InitialControlInfo()
        {
            
        }

    }
}
