/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* author: Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*/
/**
* @class App.common.Guid
* @singleton
* Класс для работы с объектами Guid
*/

Ext.define('App.common.Guid', {

	/**
	* @private
	*/
	singleton: true,

	/**
	* @private
	* Используемые данным объектом классы
	*/
	requires: [
		'Ext.data.IdGenerator',
		'Ext.data.UuidGenerator'
	],

	/**
	* Пустой идентификатор(Guid.Empty)
	* @property
	* @type String
	*/
	Empty: "00000000-0000-0000-0000-000000000000",

	/**
	* Функция, возвращающая уникальный идентификатор(Guid)
	* @return {String} Сгенерированный Guid
	*/
	NewGuid: function () {

		return Ext.data.IdGenerator.get('uuid').generate();
	},

	/**
	* Проверка того, что это пустой Guid (Guid.Empty)
	* @param {String} Guid который нужно проверить
	* @return {Boolean} Является ли переданный Guid пустым
	*/
	isEmpty: function (source) {

		return this.Equals(source, this.Empty);
	},

	/**
	* Проверка того, что этот Guid валидный
	* @param {String} Guid который нужно проверить
	* @return {Boolean} Является ли переданный Guid валидным
	*/
	isValid: function (source) {

		return String(source).length === String(Guid.Empty).length &&
			!Guid.isEmpty(source);
	},

	/**
	* Проверка двух Guid'ов на равенство
	* @param {String} Исходный Guid который нужно проверить
	* @param {String} Guid с которым нужно проверять
	* @return {Boolean} Являются ли переданные Guid'ы идентичными
	*/
	Equals: function (source, dest) {

		return Ext.isEmpty(source) || Ext.isEmpty(dest) ?
			false : String(source).toLowerCase() == String(dest).toLowerCase();
	}
}, function () {

	Guid = this;
});