﻿using System;
using Newtonsoft.Json;

namespace Platform.Client.Converters
{
	/// <summary>
	/// Конвертор функций ExtJS
	/// </summary>
	public class ExtFunctionConverter: JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is ExtFunction)
			{
				writer.WriteRawValue(string.Concat(
					"function(",
					string.Join(", ", ((ExtFunction)value).Params.ToArray()),
					") {",
					((ExtFunction)value).FunctionText,
					"}"
				));
			}
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
		/// <returns>
		/// The object value.
		/// </returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Функция возвращает, может ли конвертироваться данный тип
		/// </summary>
		/// <param name="objectType">Тип объекта</param>
		/// <returns>Булевое выражение, может или нет</returns>
		public override bool CanConvert(Type objectType)
		{
			return typeof(ExtFunction).IsAssignableFrom(objectType);
		}
	}
}

