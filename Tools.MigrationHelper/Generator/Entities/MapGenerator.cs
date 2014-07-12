﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 11.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Tools.MigrationHelper.Generator.Entities
{
    using System.Collections.Generic;
    using Tools.MigrationHelper.EntitySerializer;
    using Platform.PrimaryEntities.Reference;
    using Platform.PrimaryEntities.Common.DbEnums;
    using Platform.PrimaryEntities.Common.Interfaces;
    using System.Linq;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "11.0.0.0")]
    public partial class MapGenerator : MapGeneratorBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("using System.Data.Entity.ModelConfiguration;\r\n\r\nnamespace ");
            
            #line 12 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_namespace));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n    public class ");
            
            #line 14 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Name));
            
            #line default
            #line hidden
            this.Write("Map : EntityTypeConfiguration<");
            
            #line 14 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetEntityName()));
            
            #line default
            #line hidden
            this.Write(">\r\n    {\r\n        public ");
            
            #line 16 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Name));
            
            #line default
            #line hidden
            this.Write("Map()\r\n        {\r\n            // Primary Key\r\n            this.HasKey(t => ");
            
            #line 19 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 if(_entity.EntityType != EntityType.Multilink) {
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 19 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture("t.Id"));
            
            #line default
            #line hidden
            
            #line 19 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 } else { 
            
            #line default
            #line hidden
            
            #line 19 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(" new { t." + FirstToUp(_fields[0].Name) + ", t." + FirstToUp(_fields[1].Name) + "}"));
            
            #line default
            #line hidden
            
            #line 19 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 } 
            
            #line default
            #line hidden
            this.Write(");\r\n\r\n            // Properties\r\n            // Table & Column Mappings\r\n        " +
                    "    this.ToTable(\"");
            
            #line 23 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Name));
            
            #line default
            #line hidden
            this.Write("\", \"");
            
            #line 23 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(_entity.Schema));
            
            #line default
            #line hidden
            this.Write("\");\r\n\t\t\t");
            
            #line 24 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 foreach(EntityField Field in _fields.Where(w =>w.IdCalculatedFieldType == null && !new int[] { 8, 9, 13, 18}.Contains(w.IdEntityFieldType))){ 
            
            #line default
            #line hidden
            this.Write("            this.Property(t => t.");
            
            #line 25 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FirstToUp(Field.Name)));
            
            #line default
            #line hidden
            this.Write(").HasColumnName(\"");
            
            #line 25 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Field.Name));
            
            #line default
            #line hidden
            this.Write("\");\r\n\t\t\t");
            
            #line 26 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n            // Relationships\r\n\t\t\t");
            
            #line 29 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 foreach(EntityField Field in _fields.Where(w => w.IdCalculatedFieldType == null && new int[] {7}.Contains(w.IdEntityFieldType) && _entities.Any(a=> a.Id == w.IdEntityLink && a.IdEntityType != 1))){ 
            
            #line default
            #line hidden
            this.Write("            this.");
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
if(Field.AllowNull){
            
            #line default
            #line hidden
            this.Write("HasOptional");
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
}else{
            
            #line default
            #line hidden
            this.Write("HasRequired");
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
}
            
            #line default
            #line hidden
            this.Write("(t => t.");
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 if(!new int[]{ 8, 9, 13, 18}.Contains(Field.IdEntityFieldType)){ 
            
            #line default
            #line hidden
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ModifyName(Field.Name)));
            
            #line default
            #line hidden
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 } else { 
            
            #line default
            #line hidden
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Field.Name));
            
            #line default
            #line hidden
            
            #line 30 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
 } 
            
            #line default
            #line hidden
            this.Write(")\r\n                .WithMany(");
            
            #line 31 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(GetLinkedFieldName(Field)));
            
            #line default
            #line hidden
            this.Write(")\r\n                .HasForeignKey(d => d.");
            
            #line 32 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(FirstToUp(Field.Name)));
            
            #line default
            #line hidden
            this.Write(");\r\n\t\t\t");
            
            #line 33 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
}
            
            #line default
            #line hidden
            this.Write("        }\r\n    }\r\n}\r\n\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 38 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public string GetEntityName()
{
	return _entity.IdEntityType == (byte)EntityType.Multilink ? _entity.Schema + _entity.Name : _entity.Name;
}
        
        #line default
        #line hidden
        
        #line 43 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public string FirstCharToUp(string str)
{
	return char.ToUpper(str[0]) + str.Substring(1);
}
        
        #line default
        #line hidden
        
        #line 48 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public string ModifyName(string str)
{
	return char.ToUpper(str[2]) + str.Substring(3);
}
        
        #line default
        #line hidden
        
        #line 53 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public string GetLinkedFieldName(EntityField field)
{
	var linkedField = _entities.FirstOrDefault(w=>w.Id == field.IdEntityLink).Fields.FirstOrDefault(f=>f.IdEntityLink == field.IdEntity && f.IdEntityFieldType != 7 && ((f.IdEntityFieldType == 9 && f.IdOwnerField == field.Id) || f.IdEntityFieldType != 9));
	return linkedField == null ? "" : "t => t." + FirstToUp(linkedField.Name);
}
        
        #line default
        #line hidden
        
        #line 59 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public Entity GetEntity(int? idEntity)
{
	if(idEntity == null)
		return null;
	var entity = _entities.FirstOrDefault(w => w.Id == idEntity);
	return entity;
}
        
        #line default
        #line hidden
        
        #line 67 "D:\SBOR\Platform3\sbor3\Tools.MigrationHelper\Generator\Entities\MapGenerator.tt"
public string FirstToUp(string str)
{
	//if(str == "idDocStatus" || str == "id")
		return FirstCharToUp(str);
	//else
		//return str;
}
        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "11.0.0.0")]
    public class MapGeneratorBase
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