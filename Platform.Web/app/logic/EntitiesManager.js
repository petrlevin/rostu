/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.EntitiesManager
* @mixins Ext.util.Observable
* Основной менеджер справочников
*/
Ext.define("App.logic.EntitiesManager", {

	requires: [
		'App.common.Ids',
        'Ext.util.Observable'
	],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
		/**
		* Реализация событийной модели
		*/
		observable : 'Ext.util.Observable'
	},

	lastResult : [],

	onEntitiesUpdate : function(result) {

		// <debug>
		App.Logs.log('Entities updated', App.Logs.INFO);
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.dir(result);
		}
		// </debug>

	    result.result = result.result.concat(this.getAdditionalItems());
		this.lastResult = result;

		/*this.ef = this.getEntityByName('EntityField');
		App.ModelMgr.getModel(this.ef.id, null, function (sender, model) {
		    this.ef_model = model;
		}, this);*/

		this.fireEvent('entities', this, Ext.applyIf({

			result: this.lastResult.result //this.getEntitiesByType(3)
		}, this.lastResult));
	},

	onGroupsUpdate: function (result) {

	    // <debug>
	    App.Logs.log('Groups updated', App.Logs.INFO);
	    if (Ext.isDefined(Ext.global.console)) {
	        Ext.global.console.dir(result);
	    }
	    // </debug>

	    result.result = result.result.concat(this.getAdditionalGroups());

	    this.fireEvent('groups', this, result.result);
	},

	getEntitiesByType: function (type_id) {

	    var result = [];
	    Ext.each(this.lastResult.result, function (entity) {
	        if (entity.identitytype === type_id) {
	            result.push(Ext.apply({}, entity));
	        }
	    });
	    return result;
	},

	getEntityById: function (id) {

	    var result = null;
	    Ext.each(this.lastResult.result, function (entity) {
	        if (entity.id === id) {
	            result = Ext.apply({}, entity);
	            return false;
	        }
	    });
	    return result;
	},

	getEntityByName: function (name) {

	    var result = null;
	    Ext.each(this.lastResult.result, function (entity) {
	        if (entity.name === name) {
	            result = Ext.apply({}, entity);
	            return false;
	        }
	    });
	    return result;
	},

    /**
	* Регистрация изменений в справочнике. Сгенерирует событие, которое может известить всех
	* подписчиков (открытые списки/формы) о том, что произошли изменения.
	*/
	registerUpdate: function (entityId, docId) {

	    this.fireEvent('updates', this, entityId, docId);
	    if (entityId === App.common.Ids.EntityGroup)
	        this.groupUpdate();
	    if (entityId === App.common.Ids.Entity) {
	        this.update();
	    }
	},

	getEntities: function() {

		return this.lastResult;
	},

	onLoggedIn: function() {

	    this.update();
	    this.groupUpdate();
	},

	update: function() {

		DataService.getEntities(this.onEntitiesUpdate, this);
	},

	groupUpdate: function () {
        DataService.getEntitiesTree(App.common.Ids.EntityGroup, this.onGroupsUpdate, this);
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function(config) {
		this.mixins.observable.constructor.call(this, config);
		this.addEvents('entities', 'updates' , 'groups');

		App.Profile.on('logged_in', this.onLoggedIn, this);
		this.callParent(arguments);
	},
	
    //==========================================================================================================================

	getAdditionalGroups: function () {
	    return [
            {
                id: -1,
                idparent: null,
                caption: "Диаграммы",
                description: "",
                order: null
            }
	    ];
	},

	getAdditionalItems: function () {
	    return [
            {
                id: -1,
                caption: "Отчет - диаграмма",
                description: '',
                identitygroup: -1,
                identitytype: 9,
                order: null,
                iconCls: 'chart-bar',
                action: 'App.components.Chart',
                img: 'Resources/charts/bar-chart.png' //'http://habr.habrastorage.org/comment_images/41d/1b4/925/41d1b49255673ab576194f7dd16d8dec.gif'
            },
	        {
	            id: -2,
	            caption: "Отчет в виде файла",
	            identitygroup: -1,
	            identitytype: 9,
	            iconCls: 'icon-excel',
	            action: 'link',
	            link: 'Resources/files/tableReport.xls'
	        }
	    ];
	}
});