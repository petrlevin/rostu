/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Hidden
* @extends Ext.form.field.Hidden
* @mixins App.components.mixins.FormField
* Скрытое, системное поле
*/

Ext.define('App.components.fields.Hidden', {
	extend: 'Ext.form.field.Hidden',
	alias: 'widget.app_hiddenfield',

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
	        Ext.global.console.log('Hidden field constructor');
	    }
	    //</debug> 

	    this.callParent([ config ]);
	},

	getValue: function() {

		return this.value;
	},

	/**
	* Sets a data value into the field and runs the change detection and validation. To set the value directly
	* without these inspections see {@link #setRawValue}.
	* @param {Object} value The value to set
	* @return {Ext.form.field.Field} this
	*/
	setValue: function(value) {
		var me = this;
		return me.mixins.field.setValue.call(me, value);
	},

	isEqual: function(value1, value2) {
		return value1 == value2;
	}
});
