/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.DateTime
* @extends Ext.form.field.Date
* @mixins App.components.mixins.FormField
* Поле с датой и временем
*/

Ext.define('App.components.fields.Date', {
	extend: 'Ext.form.field.Date',
	alias: 'widget.app_datefield',
	
	format: 'd.m.Y',

	startDay: 1,
	
	requires: [
		'App.components.mixins.FormField',
		'App.components.mixins.CommonField'
	],
	
	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
	    formfield: 'App.components.mixins.FormField',
	    commonfield: 'App.components.mixins.CommonField'
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Standard column constructor');
		}
		//</debug>

		this.callParent([config]);
	},

	set: function (data, defaults) {

		var fieldValues = data.result[0];
		var value = fieldValues[this.name.toLowerCase()] || fieldValues[this.name];
		if (!Ext.isDate(value)) {
		    value = Ext.Date.parse(value, "Y-m-dTH:i:s");
		}
		fieldValues[this.name.toLowerCase()] = value;
	    this.mixins.formfield.set.apply(this, [data, defaults]);
	}
});
