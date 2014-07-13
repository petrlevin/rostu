using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

using Resources = Capricorn.Properties.Resources;

namespace Capricorn.Configuration
{
    public class CDataConfigurationElement
        : ConfigurationElement
    {
        private readonly string _cDataConfigurationPropertyName;
        
        public CDataConfigurationElement()
        {
            PropertyInfo[] properties = GetType().GetProperties();
            int cDataConfigurationPropertyCount = 0;
            int configurationElementPropertyCount = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                ConfigurationPropertyAttribute[] configurationPropertyAttributes =
                    getAttributes<ConfigurationPropertyAttribute>(property);
                CDataConfigurationPropertyAttribute[] cDataConfigurationPropertyAttribute =
                    getAttributes<CDataConfigurationPropertyAttribute>(property);
                
                bool hasConfigurationPropertyAttribute = 
                    configurationPropertyAttributes.Length != 0;
                bool hasCDataConfigurationPropertyAttribute = 
                    cDataConfigurationPropertyAttribute.Length != 0;

                if (hasConfigurationPropertyAttribute &&
                    property.PropertyType.IsSubclassOf(
                        typeof(ConfigurationElement)))
                {
                    configurationElementPropertyCount++;
                }

                if (hasCDataConfigurationPropertyAttribute)
                {                    
                    cDataConfigurationPropertyCount++;
                    throwIf(
                        cDataConfigurationPropertyCount > 1,
                        Resources.ERROR_TOO_MANY_CDATA_CONFIGURATION_ELEMENTS);

                    throwIf(
                        !hasConfigurationPropertyAttribute,
                        Resources.ERROR_MISSING_CONFIGURATION_PROPERTY_ATTRIBUTE,
                        property.Name);

                    throwIf(
                        !property.PropertyType.Equals(typeof(string)),
                        Resources.ERROR_CDATA_CONFIGURATION_PROPERTY_MUST_BE_STRING,
                        property.Name);                    
                    
                    _cDataConfigurationPropertyName =
                        configurationPropertyAttributes[0].Name;
                }                
            }

            throwIf(
                configurationElementPropertyCount > 0 &&
                    cDataConfigurationPropertyCount > 0,
                Resources.ERROR_CLASS_CONTAINS_CONFIGURATION_PROPERTY,
                GetType().FullName);                
        }

        private T[] getAttributes<T>(PropertyInfo property)
            where T : Attribute
        {
            object[] objectAttributes = property.GetCustomAttributes(
                    typeof(T),
                    true);
            return Array.ConvertAll<object, T>(
                    objectAttributes,
                    delegate(object o)
                    {
                        return o as T;
                    });                
        }

        private void throwIf(
            bool condition,
            string formatString,
            params object[] values)
        {
            if (condition)
            {
                if (values.Length > 0)
                {
                    formatString = string.Format(formatString, values);
                }

                Trace.WriteLine(formatString);
                throw new ConfigurationErrorsException(
                    formatString);
            }
        }
        
        protected override bool SerializeElement(
            System.Xml.XmlWriter writer, 
            bool serializeCollectionKey)
        {
            bool returnValue;
            if (string.IsNullOrEmpty(
                _cDataConfigurationPropertyName))
            {
                returnValue = base.SerializeElement(
                    writer, serializeCollectionKey);
            }
            else
            {               
                foreach (ConfigurationProperty configurationProperty in 
                    Properties)
                {
                    string name = configurationProperty.Name;
                    TypeConverter converter = configurationProperty.Converter;
                    string propertyValue = converter.ConvertToString(
                            base[name]);

                    if (name == _cDataConfigurationPropertyName)
                    {
                        writer.WriteCData(propertyValue);
                    }
                    else
                    {
                        writer.WriteAttributeString("name", propertyValue);
                    }
                }                               
                returnValue = true;
            }
            return returnValue;
        }

        protected override void DeserializeElement(
            System.Xml.XmlReader reader, 
            bool serializeCollectionKey)
        {
            if (string.IsNullOrEmpty(
                _cDataConfigurationPropertyName))
            {
                base.DeserializeElement(
                    reader, serializeCollectionKey);
            }
            else
            {
                foreach (ConfigurationProperty configurationProperty in 
                    Properties)
                {
                    string name = configurationProperty.Name;
                    if (name == _cDataConfigurationPropertyName)
                    {
                        string contentString = reader.ReadString();
                        base[name] = contentString.Trim();
                    }
                    else
                    {
                        string attributeValue = reader.GetAttribute(name);
                        base[name] = attributeValue;
                    }
                }
                reader.ReadEndElement();
            }           
        }
    }
} 
