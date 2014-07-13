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
* Простой грид для отображения "иерархической" сущности
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
* @class App.components.EntityTreeGrid
* @extends Ext.tree.Panel
*/

Ext.define("App.components.EntityTreeGrid", {
    extend: "Ext.tree.Panel",

    uses: [
        'App.logic.factory.Store',
        'App.logic.factory.Columns',
        'App.components.FiltersFeature',
        'Ext.button.Split',
        'Ext.toolbar.Paging'
    ],

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function(config) {
    	var colMgr, store, cfg, factory;

    	//<debug>
    	if (Ext.isDefined(Ext.global.console)) {
    		Ext.global.console.log('Конструктор иерархического грида');
    	}
    	//</debug>

        // Хранилище
    	factory = Ext.create('App.logic.factory.Store', config);

        // Модель колонок грида
        colMgr = Ext.create('App.logic.factory.Columns', {
            model: config.model
        });

        var actions = [];
        Ext.each(config.actions, function (btn) {
            if (btn === '->') {
                actions.push({
                    xtype: 'splitbutton',
                    text: 'Развернуть',
                    scope: this,
                    handler: this.expandSelectedOneLevel,
                    setAvailability: Ext.emptyFn,
                    menu: new Ext.menu.Menu({
                        items: [
                            { text: 'Развернуть выделенное на 1 уровень', setAvailability: Ext.emptyFn, handler: this.expandSelectedOneLevel, scope: this },
                            { text: 'Развернуть выделенное до конечного элемента', setAvailability: Ext.emptyFn, handler: this.expandSelectedAll, scope: this },
                            { text: 'Развернуть всё на 1 уровень', setAvailability: Ext.emptyFn, handler: this.expandAllOneLevel, scope: this },
                            { text: 'Развернуть всё до конечного элемента', setAvailability: Ext.emptyFn, handler: this.expandAll, scope: this }
                        ]
                    })
                });
            }
            actions.push(btn);
        }, this);
        config.actions = actions;
        store = factory.hierarchy();
        // Конфигурация для создания грида
        cfg = {
            border: true,
            useArrows: true,
            rootVisible: false,
            tbar: config.actions,
            multiSelect: true,
            store: store,
            columns: colMgr.get(config.hierarchyViewField || config.model.captionField),
            listeners: config.gridListeners,
            // Чтобы отключить пейджинг - надо убрать этот код
            dockedItems: [{
                xtype: 'pagingtoolbar',
                store: store,
                dock: 'bottom',
                displayInfo: true
            }]
            // до сих вырезать
        };

        if (Ext.isDefined(config.customGrid)) {

            Ext.apply(cfg, config.customGrid);
        }

        // Добавление фильтров в конфигурацию объекта
        if (!cfg.features) {
            cfg.features = [];
        }
        cfg.features.push(Ext.create('App.components.FiltersFeature', {}));

        this.callParent([cfg]);
    },

    
    expandSelectedOneLevel: function(sender) {
        var selected = this.getSelectionModel().getSelection();
        Ext.each(selected, function(select) {
            select.expand(false);
        });
    },
    
    expandSelectedAll: function (sender) {
        var selected = this.getSelectionModel().getSelection();
        Ext.each(selected, function (select) {
            select.expand(true);
        });
    },

    expandAllOneLevel: function (sender) {
        var me = this;

        Ext.Msg.show({
            title: 'Развернуть все элементы?',
            msg: 'Операция может занять продолжительное время. Продолжить?',
            buttons: Ext.MessageBox.YESNO,
            icon: Ext.Msg.QUESTION,
            fn: function (btn) {
                if (btn == 'yes') {
                    var temp = me.getRootNode();
                    temp.expandChildren(false);
                }
            }
        });

        
    },

    expandAll: function (sender) {
        var me = this;

        Ext.Msg.show({
            title: 'Развернуть все элементы?',
            msg: 'Операция может занять продолжительное время. Продолжить?',
            buttons: Ext.MessageBox.YESNO,
            icon: Ext.Msg.QUESTION,
            fn: function (btn) {
                if (btn == 'yes') {
                    var temp = me.getRootNode();
                    temp.expandChildren(true);
                }
            }
        });

    }
});