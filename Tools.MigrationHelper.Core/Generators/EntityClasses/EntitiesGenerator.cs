﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 11.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Tools.MigrationHelper.Core.Generators.EntityClasses
{
    using System.Collections.Generic;
    using Platform.PrimaryEntities.Reference;
    using Platform.PrimaryEntities.DbEnums;
    using Platform.PrimaryEntities.Common.DbEnums;
    using Platform.PrimaryEntities.Common.Interfaces;
    using System.Linq;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "11.0.0.0")]
    public partial class EntitiesGenerator : EntitiesGeneratorBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write(@"using System;
using System.Collections.Generic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
");
            
            #line 18 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetUseBaseAppInterfaces()));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n");
            
            #line 20 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 foreach(var str in GetUsings()) {
            
            #line default
            #line hidden
            
            #line 20 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(str));
            
            #line default
            #line hidden
            
            #line 20 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n\r\nnamespace ");
            
            #line 23 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_namespace));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n\t");
            
            #line 25 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации"));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 26 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// <summary>"));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 27 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// " + _entity.Caption));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 28 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// </summary>"));
            
            #line default
            #line hidden
            this.Write("\r\n\tpublic partial class ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GeneratorHelper.GetEntityName(_entity)));
            
            #line default
            #line hidden
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetEntityType(_entity)));
            
            #line default
            #line hidden
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIStatus()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIVersioning()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIHasRegistrator()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIHasTerminator()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIHierarhy()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIRegistryWithOperation()));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 29 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetIRegistryWithTermOperation()));
            
            #line default
            #line hidden
            this.Write("\r\n\t{\r\n\t");
            
            #line 31 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 if(_entity.Fields.All(a => a.Name.ToLower() != "id")) {
            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 32 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetPrivateId()));
            
            #line default
            #line hidden
            
            #line 32 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 34 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
			foreach (EntityField Field in _entity.Fields.Where(w => (w.IdCalculatedFieldType == null || w.IdCalculatedFieldType == (byte)CalculatedFieldType.DbComputed || w.IdCalculatedFieldType == (byte)CalculatedFieldType.DbComputedPersisted) && w.IdEntityFieldType != (byte)EntityFieldType.Multilink))
			{ 
            
            #line default
            #line hidden
            
            #line 34 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 if(!string.IsNullOrEmpty(Field.Caption)) {
            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 35 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// <summary>"));
            
            #line default
            #line hidden
            this.Write("\r\n\t\t");
            
            #line 36 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// " + Field.Caption));
            
            #line default
            #line hidden
            this.Write("\r\n\t\t");
            
            #line 37 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("/// </summary>"));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 38 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 } 
            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 39 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetProperty(Field)));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n\t");
            
            #line 41 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 } 
            
            #line default
            #line hidden
            
            #line 42 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 foreach(var str in GetTpChildCollection()){ 
            
            #line default
            #line hidden
            this.Write("\t\t");
            
            #line 43 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(str));
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 44 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 46 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
 foreach(var str in  GetMultiLinkRelationship()) {
            
            #line default
            #line hidden
            
            #line 47 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(str));
            
            #line default
            #line hidden
            this.Write("\r\n\t\t\t");
            
            #line 48 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n\t\t/// <summary>\r\n\t\t/// Конструктор по-умолчанию\r\n\t\t/// </summary>\r\n\t\tpublic ");
            
            #line 53 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GeneratorHelper.GetEntityName(_entity)));
            
            #line default
            #line hidden
            this.Write("()\r\n\t\t{\r\n\t\t}\r\n\r\n\t\t/// <summary>\r\n\t\t/// Идентификатор типа сущности\r\n\t\t/// </summa" +
                    "ry>\r\n\t\tpublic override int EntityId\r\n\t\t{\r\n\t\t\tget { return ");
            
            #line 62 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Id));
            
            #line default
            #line hidden
            this.Write("; }\r\n\t\t}\r\n\r\n\t\t/// <summary>\r\n\t\t/// Идентификатор типа сущности\r\n\t\t/// </summary>\r" +
                    "\n\t\tpublic static int EntityIdStatic\r\n\t\t{\r\n\t\t\tget { return ");
            
            #line 70 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Id));
            
            #line default
            #line hidden
            this.Write("; }\r\n\t\t}\r\n\r\n\t\t/// <summary>\r\n\t\t/// Русское наименование типа сущности\r\n\t\t/// </su" +
                    "mmary>\r\n\t\tpublic override string EntityCaption\r\n\t\t{\r\n\t\t\tget { return \"");
            
            #line 78 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Caption.Replace("\"", "\\\"")));
            
            #line default
            #line hidden
            this.Write("\"; }\r\n\t\t}\r\n\r\n\t\t");
            
            #line 81 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(DocumentCaptionOverride()));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n\t\t");
            
            #line 83 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PropertyAccessExpressions()));
            
            #line default
            #line hidden
            this.Write(@"

		/// <summary>
		/// Регистрация идентфикатора сущности
		/// </summary>
		public class EntityIdRegistrator:IBeforeAplicationStart
		{
			/// <summary>
			/// Зарегистрировать
			/// </summary>
			public void Execute()
			{
				Platform.BusinessLogic.EntityIds.Add(GetType().DeclaringType,");
            
            #line 95 "D:\Projects\Platform3\Tools.MigrationHelper.Core\Generators\EntityClasses\EntitiesGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Id));
            
            #line default
            #line hidden
            this.Write(");\r\n\t\t\t}\r\n\t\t}\r\n\r\n\r\n\t}\r\n}");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "11.0.0.0")]
    public class EntitiesGeneratorBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
