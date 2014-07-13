/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @author Pavel Dovgalenko
* @docauthor Pavel Dovgalenko
*
* Простой грид для отображения "плоской" сущности
*
* ##Конфигурация
*
*  Нужно передать конфигурацию в виде объекта с необходимыми свойствами, вида :
*
*  var config = {
*  		actions: this.getActions(),
*  		directFn : this.direct_function,
*  		model: entity_model,
*  		extraParams: extra_parameters,
*  		gridListeners: grid_listeners,
*  		customGrid: custom_grid_configuration
*  }
* 
* @class App.components.EntityGrid
* @extends Ext.grid.Panel
*/

Ext.define("App.components.EntityGrid", {
    extend: "Ext.grid.Panel",

    uses: [
        'App.logic.factory.Store',
        'App.logic.factory.Columns',
        'App.components.FiltersFeature',
        'Ext.grid.plugin.BufferedRenderer',
        'Ext.toolbar.Paging'
    ],

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function(config) {
    	var store, colMgr, cfg, factory;

    	//<debug>
    	if (Ext.isDefined(Ext.global.console)) {
    		Ext.global.console.log('Конструктор обыкновенного грида');
    	}
    	//</debug>

        // Хранилище
    	factory = Ext.create('App.logic.factory.Store', config);

    	// Модель колонок грида
        colMgr = Ext.create('App.logic.factory.Columns', {
            model: config.model
        });

        store = factory.plain();

        var searchField = config.searchField;

        // Конфигурация для создания грида
        cfg = {
            border: true,
            selModel: {
                preventFocus: true
            },
            tbar: config.actions,
            searchField: searchField,
            store: store,
            multiSelect: true,
            columns: colMgr.get(),
            plugins: {
                ptype: 'bufferedrenderer',
                trailingBufferZone: 20,
                leadingBufferZone: 20
            },
            listeners: config.gridListeners,
            dockedItems:[{
                xtype: 'pagingtoolbar',
                store: store,
                dock: 'bottom',
                displayInfo: true
            }]
        };

        if (Ext.isDefined(config.customGrid)) {

            Ext.apply(cfg, config.customGrid);
        };

        // Добавление фильтров в конфигурацию объекта
        if (!cfg.features) {
            cfg.features = [];
        }
        cfg.features.push(Ext.create('App.components.FiltersFeature', {}));

        this.callParent([ cfg ]);
    }
});