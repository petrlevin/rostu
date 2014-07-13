/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.ModelManager
* @mixins Ext.util.Observable
* Основной менеджер форм
*/
Ext.define("App.logic.FormManager", {

	requires: [
		'Ext.util.Observable',
		'App.logic.mixins.Cache'
	],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
		/**
		* Реализация событийной модели
		*/
		observable: 'Ext.util.Observable',
		cache: 'App.logic.mixins.Cache'
	},

	getForm: function (idEntity, formContext, callback, scope) {
	    //CORE-802,803,808,809: отключение кеша форм решает данные проблемы.
	    //var fetcher = Ext.bind(FormService.getForm, FormService, [idEntity, formContext, this.onForm, this]);
	    //return this.fromCache(idEntity + '-' + formContext, fetcher, callback, scope);
	    var cb = function (value) {
	        callback.call(this, this, value);
	    };
	    FormService.getForm(idEntity, formContext, cb, scope);
	    return false;
	},

	onForm: function (value, response) {
	    //<debug>
	    if (response.type !== 'rpc') {
	        Ext.Error.raise('Exception occured while trying to get form for the entity!!!');
	    }
	    //</debug>

	    Ext.each(value.tablefields, function(tablefieldForm) {
	        // <debug>
	        if (Ext.isDefined(Ext.global.console)) {
	            Ext.global.console.log('registerInCache: ' + tablefieldForm.entityId + '-' + tablefieldForm.formType);
	        }
	        // </debug>
	        this.registerInCache(tablefieldForm.entityId + '-' + tablefieldForm.formType, tablefieldForm);
	    }, this);

	    this.registerInCache(value.entityId + '-' + value.formType, value, response);
	},

	getFormByName: function (formName, formContext, callback, scope) {
	    // ToDo: отсутствует кеширование. Использовать метод onForm в качестве callback'а нельзя, т.к. полученная по имени форма будет закеширована под неправильным ключем (value.entityId + '-' + value.formType. В то время как должна кешироваться по имени)
	    var fetcher = Ext.bind(FormService.getFormByName, FormService, [formName, formContext, this.onForm, this]);
	    return this.fromCache(formName, fetcher, callback, scope);
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		this.mixins.observable.constructor.call(this, config);
		this.mixins.cache.constructor.call(this, config);
	},
	
	getKey: function (transaction) {
	    return transaction.args[0] + '-' + transaction.args[1];
	}
});