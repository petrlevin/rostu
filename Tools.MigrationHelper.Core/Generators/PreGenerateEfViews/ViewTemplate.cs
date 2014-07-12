﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 11.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Tools.MigrationHelper.Core.Generators.PreGenerateEfViews
{
    using System.IO;
    using System.Data.Entity;
    using System.Data.Entity.Design;
    using System.Data.Entity.Infrastructure;
    using System.Data.Metadata.Edm;
    using System.Data.Mapping;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "D:\SBOR\Platform3\original\Tools.MigrationHelper.Core\Generators\PreGenerateEfViews\ViewTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "11.0.0.0")]
    public partial class ViewTemplate : ViewTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            
            #line 1 "D:\SBOR\Platform3\original\Tools.MigrationHelper.Core\Generators\PreGenerateEfViews\ViewTemplate.tt"

/*
Copyright (c) Pawel Kadluczka. All rights reserved.

AS IS, NO WARRANTY, USE ON YOUR OWN RISK

T4 template for creating pre-generated views for Entity Framework Code First application

Usage: 
1. Add this template to your project
2. Rename this template so that its name contains the name of your DbContext derived class (e.g. MyContext.Views.tt)
3. Right-click the template file in the Solution Explorer and select "Run Custom Tool"
4. If you have more than one DbContext derived class in your project you need to add a separate template for each
DbContext derived class you want to create pre-generated views for. 
*/

            
            #line default
            #line hidden
            
            #line 36 "D:\SBOR\Platform3\original\Tools.MigrationHelper.Core\Generators\PreGenerateEfViews\ViewTemplate.tt"
 
	var configFilePath = GetConfigFilePath();
	if (configFilePath != null)
	{
		AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFilePath);
	}
 
	var contextTypeName = GetContextTypeName();
	WriteLine(GenerateViews(contextTypeName));

            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        private global::Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost hostValue;
        /// <summary>
        /// The current host for the text templating engine
        /// </summary>
        public virtual global::Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost Host
        {
            get
            {
                return this.hostValue;
            }
            set
            {
                this.hostValue = value;
            }
        }
        
        #line 46 "D:\SBOR\Platform3\original\Tools.MigrationHelper.Core\Generators\PreGenerateEfViews\ViewTemplate.tt"


	private string GenerateViews(string contextTypeName)
	{
		// Get Edmx for the context
		var edmx = GetEdmx(GetContextType(contextTypeName));

		// extract csdl, ssdl and msl artifacts from the Edmx
		XmlReader csdlReader, ssdlReader, mslReader;
		SplitEdmx(edmx, out csdlReader, out ssdlReader, out mslReader);

		// Initialize item collections
		var edmItemCollection = new EdmItemCollection(new XmlReader[] { csdlReader });
		var storeItemCollection = new StoreItemCollection(new XmlReader[] { ssdlReader });
		var mappingItemCollection = new StorageMappingItemCollection(edmItemCollection, storeItemCollection, new XmlReader[] { mslReader });

		// Initialize the view generator to generate views in C#
		var viewGenerator = new EntityViewGenerator() { LanguageOption = LanguageOption.GenerateCSharpCode };

		// generate views
		using(var memoryStream = new MemoryStream())
		{
			var writer = new StreamWriter(memoryStream);
			
			var errors = viewGenerator.GenerateViews(mappingItemCollection, writer, GetEdmxSchemaVersion(edmx)).ToList();

			if (errors.Any())
			{
				foreach (var error in errors)
				{
					Error(error.Message);
				}

				return string.Empty;
			}

			memoryStream.Position = 0;
			var reader = new StreamReader(memoryStream);
			return reader.ReadToEnd();
		}
	}

	private Type GetContextType(string contextTypeName)
	{
		foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
		{
			var contextType = assembly
								.GetTypes()
								.AsEnumerable()
								.FirstOrDefault(
									t => t.Name == contextTypeName && 
									typeof(DbContext).IsAssignableFrom(t));

			if(contextType != null )
			{
				return contextType;
			}
		}

		throw new InvalidOperationException(
			"Could not find the context type. Make sure the template name is using convention '{ContextTypeName}.Views.tt'");
	}

	private string GetContextTypeName()
	{
		var templateFileName = Path.GetFileNameWithoutExtension(Host.TemplateFile);

		var dotPosition = templateFileName.IndexOf('.');
		if(dotPosition <= 0)
		{
			throw new InvalidOperationException(
				string.Format("The template name '{0}' has an unexpected format.", Host.TemplateFile));
		}

		return templateFileName.Substring(0, dotPosition);
	}

	private XDocument GetEdmx(Type contextType)
	{		
		var ms = new MemoryStream();

		using(var writer = XmlWriter.Create(ms))
		{
			EdmxWriter.WriteEdmx((DbContext)Activator.CreateInstance(contextType), writer);
		}

		ms.Position = 0;

		return XDocument.Load(ms);
	}

	private void SplitEdmx(XDocument edmx, out XmlReader csdlReader, out XmlReader ssdlReader, out XmlReader mslReader)
	{
		// xml namespace agnostic to make it work with any version of Entity Framework
        var edmxNs = edmx.Root.Name.Namespace;

        var storageModels = edmx.Descendants(edmxNs + "StorageModels").Single();
        var conceptualModels = edmx.Descendants(edmxNs + "ConceptualModels").Single();
        var mappings = edmx.Descendants(edmxNs + "Mappings").Single();

        ssdlReader = storageModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
        csdlReader = conceptualModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
        mslReader = mappings.Elements().Single(e => e.Name.LocalName == "Mapping").CreateReader();
	}

	private string GetConfigFilePath()
	{
		var dte = (EnvDTE.DTE)((IServiceProvider)Host).GetService(typeof(EnvDTE.DTE));
		var project = dte.Solution.FindProjectItem(Host.TemplateFile).ContainingProject;
		var configFile = 
			project
			.ProjectItems
			.Cast<EnvDTE.ProjectItem>()
			.FirstOrDefault(i => string.Compare("web.config", i.Name, true) == 0 || string.Compare("app.config", i.Name, true) == 0);

		return configFile == null ? null : configFile.FileNames[0];
	}

	private Version GetEdmxSchemaVersion(XDocument edmx)
	{
		var edmxNs = edmx.Root.GetDefaultNamespace();

		if (edmxNs == "http://schemas.microsoft.com/ado/2009/11/edmx")
		{
			return EntityFrameworkVersions.Version3;
		}
		else if (edmxNs == "http://schemas.microsoft.com/ado/2008/10/edmx")
		{
			return EntityFrameworkVersions.Version2;
		}

		// V1, greater than V3 or non-edmx edmxNs
		throw new ArgumentException("Unsupported edmx version");
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
    public class ViewTemplateBase
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
