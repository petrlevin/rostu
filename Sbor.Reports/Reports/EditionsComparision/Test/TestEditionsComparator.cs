using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.GenericTree;
using Sbor.Reports.EditionsComparision.Test.Framework;
using Sbor.Reports.Reports.EditionsComparision.Test;
using Sbor.Reports.Reports.EditionsComparision.Test.Framework;

namespace Sbor.Reports.EditionsComparision.Test
{
    public class TestEditionsComparator: EditionsComparator
    {
        public static class Doc
        {
            [Tablefield(FieldName = "Document", CaptionField = "", MasterField = "")]
            public class Structure
            {
                public int Id { get; set; }
                public string Caption { get; set; }
                public string Value { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Caption = "Заголовок 1", Value = "Сумма 1" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 2, Caption = "Заголовок 2", Value = "Сумма 2" },
            };
        }


        public static class Table1
        {
            [Tablefield(FieldName = "Table1", CaptionField = "Caption", MasterField = "")]
            public class Structure
            {
                public int Id { get; set; }
                public string Caption { get; set; }
                public string SomeField { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Caption = "a", SomeField = "a" },
                new Structure { Id = 2, Caption = "b", SomeField = "bbb" },
                new Structure { Id = 3, Caption = "c", SomeField = "aaa" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 11, Caption = "a", SomeField = "aa" },
                new Structure { Id = 12, Caption = "b", SomeField = "bbb" },
                new Structure { Id = 14, Caption = "y", SomeField = "yyy" },
            };
        }

        /// <summary>
        /// Неизмененные данные с измененными подчиненными
        /// </summary>
        public static class Table2
        {
            [Tablefield(FieldName = "Table2", CaptionField = "Caption", MasterField = "")]
            public class Structure
            {
                public int Id { get; set; }
                public string Caption { get; set; }
                public string SomeField { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Caption = "a", SomeField = "aaa" },
                new Structure { Id = 2, Caption = "b", SomeField = "bbb" },
                new Structure { Id = 3, Caption = "c", SomeField = "ccc" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 11, Caption = "a", SomeField = "aaa" },
                new Structure { Id = 12, Caption = "b", SomeField = "bbb" },
                new Structure { Id = 13, Caption = "c", SomeField = "ccc" },
            };
        }

        public static class Table21
        {
            [Tablefield(FieldName = "Table21", CaptionField = "Caption", MasterField = "Master")]
            public class Structure
            {
                public int Id { get; set; }
                public int Master { get; set; }
                public string Caption { get; set; }
                public string SomeField { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Master = 2, Caption = "a", SomeField = "aaa" },
                new Structure { Id = 2, Master = 2, Caption = "b", SomeField = "bbb" },
                new Structure { Id = 3, Master = 2, Caption = "c", SomeField = "ccc" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 11, Master = 12, Caption = "a", SomeField = "aaa" },
                new Structure { Id = 12, Master = 12, Caption = "b", SomeField = "bbb diff" },
                new Structure { Id = 14, Master = 12, Caption = "d", SomeField = "ddd" },
            };
        }

        /// <summary>
        /// Измененные данные с измененными подчиненными
        /// </summary>
        public static class Table3
        {
            [Tablefield(FieldName = "Table3", CaptionField = "Caption", MasterField = "")]
            public class Structure
            {
                public int Id { get; set; }
                public string Caption { get; set; }
                public string SomeField { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Caption = "a", SomeField = "unchanged 1" },
                new Structure { Id = 2, Caption = "b", SomeField = "changed" },
                new Structure { Id = 3, Caption = "c", SomeField = "deleted" },
                new Structure { Id = 4, Caption = "unchanged 2", SomeField = "unchanged 2" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 11, Caption = "a", SomeField = "unchanged 1" },
                new Structure { Id = 12, Caption = "b", SomeField = "changed la la la" },
                new Structure { Id = 14, Caption = "d", SomeField = "added" },
                new Structure { Id = 15, Caption = "unchanged 2", SomeField = "unchanged 2" },
            };
        }

        public static class Table31
        {
            [Tablefield(FieldName = "Table31", CaptionField = "Caption", MasterField = "Master")]
            public class Structure
            {
                public int Id { get; set; }
                public int Master { get; set; }
                public string Caption { get; set; }
                public string SomeField { get; set; }
            }

            public static List<Structure> data1 = new List<Structure>()
            {
                new Structure { Id = 1, Master = 3, Caption = "g", SomeField = "deleted a1" },
                new Structure { Id = 2, Master = 1, Caption = "b", SomeField = "changed in unchanged" },
                new Structure { Id = 3, Master = 4, Caption = "c", SomeField = "unchanged in unchanged" },
            };

            public static List<Structure> data2 = new List<Structure>()
            {
                new Structure { Id = 11, Master = 14, Caption = "a", SomeField = "added a1" },
                new Structure { Id = 12, Master = 11, Caption = "b", SomeField = "changed in unchanged la-la" },
                new Structure { Id = 13, Master = 15, Caption = "c", SomeField = "unchanged in unchanged" },
            };
        }


        protected override void loadData()
        {
            processMaintable();
            processTableparts();
        }

        private void processMaintable()
        {
            MainTableInfo = new TableInfoDumb()
                {
                };
        }

        private void processTableparts()
        {
            HoldersTree = new Tree<TpDataHolder>();

            var tp1 = TpHolderFactory<Table1.Structure>.Get(Table1.data1, Table1.data2);

            var tp2 = TpHolderFactory<Table2.Structure>.Get(Table2.data1, Table2.data2);
            var tp21 = TpHolderFactory<Table21.Structure>.Get(Table21.data1, Table21.data2);
            tp2.Children.Add(tp21);

            var tp3 = TpHolderFactory<Table3.Structure>.Get(Table3.data1, Table3.data2);
            var tp31 = TpHolderFactory<Table31.Structure>.Get(Table31.data1, Table31.data2);
            tp3.Children.Add(tp31);

            //HoldersTree.Add(tp1);
            //HoldersTree.Add(tp2);
            HoldersTree.Add(tp3);
        }
    }
}
