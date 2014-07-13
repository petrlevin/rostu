/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Boolean
* @extends Ext.form.field.Checkbox
* @mixins App.components.mixins.FormField
* Булевое поле
*/

Ext.define('App.components.fields.Boolean', {
	extend: 'Ext.form.field.Checkbox',
	alias: 'widget.app_booleanfield',

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
	        Ext.global.console.log('Boolean field constructor');
	    }
	    //</debug>

	    this.callParent([ config ]);
	},
	
	set: function (data, defaults) {

		var fieldValues = data.result[0];
		var value = fieldValues[this.name.toLowerCase()];
		if (!Ext.isDefined(value))
			value = fieldValues[this.name];

		if (Ext.isNumber(value)) {
			value = value == 0 ? false : true;
		}
		
		fieldValues[this.name.toLowerCase()] = value;
		this.mixins.formfield.set.apply(this, [data, defaults]);
	}
});
