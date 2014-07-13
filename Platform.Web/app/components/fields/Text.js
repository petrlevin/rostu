/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Text
* @extends Ext.form.field.Text
* @mixins App.components.mixins.FormField
* Строковое поле
*/

Ext.define('App.components.fields.Text', {
	extend: 'Ext.form.field.Text',
	alias: 'widget.app_textfield',
	
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
	    // <debug>
	    if (Ext.isDefined(Ext.global.console)) {
	        Ext.global.console.log('Standard field constructor');
	    }
	    // </debug> 

	    this.callParent([ config ]);
	}
});
