/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.factory.Store
* Фабрика хранилищ для грида.
*/
Ext.define("App.logic.factory.Store", {

    requires: [
        'App.components.data.TreeStore',
        'Ext.data.proxy.Direct',
        'Ext.data.DirectStore',
        'Ext.data.TreeStore'
    ],

    hierarchy: function() {

        return Ext.create('App.components.data.TreeStore', {
            model: App.ModelMgr.get(this.initialConfig.model),
            autoLoad: false,
            remoteSort: true,
            nodeParam: 'HierarchyFieldValue',
            defaultRootId: null,
            root: {
                expanded: false,
                text: "root"
            },
            proxy: {
                type: 'direct',
                directFn: DataService.gridSourceHierarchy,
                extraParams: this.initialConfig.extraParams,
                reader: {
                    root: 'rows',
                    totalProperty: 'count'
                }
            },
            listeners: {
                'beforeinsert': {
                    fn: this.onBeforeInsert,
                    scope: this
                },
                'beforeappend': {
                    fn: this.onBeforeAppend,
                    scope: this
                }
            }
        });
    },

    checkNodeLeaf: function (node) {
        if (node.raw && Ext.isDefined(node.raw.isgroup)) {
            node.data.leaf = !node.raw.isgroup;
        }
    },

    onBeforeAppend: function(sender, node, eOpts) {

        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Before append node (event) into TreeStore');
        }
        // </debug>
        this.checkNodeLeaf(node);
    },

    onBeforeInsert: function(sender, node, refNode, eOpts) {

        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Before insert node (event) into TreeStore');
        }
        // </debug>
        this.checkNodeLeaf(node);
    },

    plain: function () {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('App.logic.factory.Store -> plain()');
        }
        // </debug>
        return Ext.create('Ext.data.DirectStore', {
            model: App.ModelMgr.get(this.initialConfig.model),
            autoLoad: false,
            remoteSort: true,
            proxy: {
                type: 'direct',
                directFn: this.initialConfig.directFn || DataService.gridSource,
                extraParams: this.initialConfig.extraParams,
                reader: {
                    root: 'rows',
                    totalProperty: 'count'
                }
            }
        });
    },

    dimension:function(id, fields, handler, scope) {

        var store = Ext.create('Ext.data.DirectStore', {
            fields: fields,
            autoLoad: true,
            proxy: {
                type: 'direct',
                directFn: SysDimensionsService.gridSource,
                extraParams: {
                    "EntityId": id
                },
                reader: {
                    root: 'rows',
                    totalProperty: 'count'
                }
            }
        });
        store.on('load', handler, scope);

        return store;
    },

    userSettings: function () {
        return Ext.create('Ext.data.Store',
            {
                proxy: {
                    type: 'localstorage',
                    id: 'userSettings'
                }
            }
        );
    },

        /**
	* Конструктор, который создает новый объект данного класса
	*/
    constructor: function (config) {

        Ext.apply(this, {

            initialConfig: config
        });
    }
});