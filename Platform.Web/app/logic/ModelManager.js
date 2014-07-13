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
* Основной менеджер моделей
*/
Ext.define("App.logic.ModelManager", {

	requires: [
		'App.logic.mixins.Cache',
		'Ext.util.Observable',
		'Ext.data.Model'
	],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
		/**
		* Реализация событийной модели
		*/
		observable : 'Ext.util.Observable',
		cache: 'App.logic.mixins.Cache'
	},

	getModel: function (id, idOwner, callback, scope) {
		var fetcher = Ext.bind(ModelService.getEntityModel, ModelService, [id, idOwner, this.onModel, this]);

		return this.fromCache(id, fetcher, callback, scope);
	},

	onModel: function (value, response) {

		this.registerInCache(value.entityId, value, response);
	},

	/**
	* По конфигурационному объекту модели возвращает сформированный класс Ext.data.Model
	*/
	get: function (model) {

	    return Ext.define(Guid.NewGuid(), {
	        extend: 'Ext.data.Model',
	        fields: this.getFieldsConfig(model)
	    });
	},

	getFieldsConfig: function (model) {
	    var result = [];
	    Ext.each(model.result, function (item) {
	        result.push(item.name.toLowerCase());
	        if (this.isLink(item)) {
	            result.push(item.name.toLowerCase() + '_caption');
	        }
	    }, this);

	    if (!Ext.isEmpty(model.hierarchyFields)) {
	        result.push('isselectable');
	    }
	    return result;
	},

     /**
     * ToDo: этот метод не должен находиться в этом классе. Более подходящее место для него - Field.js, однако к объекту данного класса отсюда также нет доступа.
     */
	isLink: function (fieldCfg) {
	    return [
	        App.enums.EntityFieldType.Link,
	        App.enums.EntityFieldType.FileLink,
	        App.enums.EntityFieldType.ReferenceEntity,
	        App.enums.EntityFieldType.ToolEntity,
	        App.enums.EntityFieldType.TablepartEntity,
	        App.enums.EntityFieldType.DocumentEntity
	    ].indexOf(fieldCfg.identityfieldtype) > -1;
	},

   /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function(config) {
		this.mixins.observable.constructor.call(this, config);
		this.mixins.cache.constructor.call(this, config);
	},
	
	getKey: function (transaction) {
	    return transaction.args[0];
	}
});