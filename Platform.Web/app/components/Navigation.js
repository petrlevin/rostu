/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.components.Navigation
* @extends Ext.tree.Panel
* Дерево навигации
*/

Ext.define('App.components.Navigation', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.navigation',

    /**
	* Используемые данным объектом классы
	*/
    requires: [
        'App.logic.ReportEntityItem',
        'App.logic.EntitiesList',
        'App.store.Navigation',
        'Ext.toolbar.Toolbar',
        'Ext.form.field.Text',
        'Ext.tip.ToolTip',
        'App.components.Chart'
    ],

    /**
     * Типы сущностей с указанием порядка. Ключ равен идентификатору типа.
     */
    entityTypes: {
        1: { caption: 'Перечисления', order: 4 },
        2: { caption: 'Точки данных', order: 5 },
        3: { caption: 'Справочники', order: 1 },
        4: { caption: 'Табличные части', order: 6 },
        5: { caption: 'Мультиссылки', order: 7 },
        6: { caption: 'Документы', order: 2 },
        7: { caption: 'Инструменты', order: 8 },
        8: { caption: 'Регистры', order: 3 },
        9: { caption: 'Отчеты', order: 9 }
    },

    /**
    *
    */
    rootVisible: false,

    /**
    *
    */
    useArrows: true,

    /**
    *
    */
    animate: false,

    /**
    *
    */
    stateful: true,

    /**
    *
    */
    stateId: 'nav-tree',

    /**
    *
    */
    initComponent: function () {

        Ext.apply(this, {
            title: Locale.APP_NAVIGATION_TITLE,
            store: Ext.create('App.store.Navigation'),
            listeners: {
                scope: this,
                itemclick: this.onClick,
                afterrender: this.onAfterRender
            },
            dockedItems: [{
                xtype: 'toolbar',
                items: [{
                    text: Locale.APP_NAVIGATION_EXPAND,
                    handler: this.expandNodes,
                    scope: this
                }, {
                    text: Locale.APP_NAVIGATION_COLLAPSE,
                    handler: this.collapseNodes,
                    scope: this
                }]
            }, {
                xtype: 'toolbar',
                items: [{
                    name: 'filter',
                    xtype: 'textfield',
                    width: '100%',

                    enableKeyEvents: true,

                    scope: this,
                    handler: this.filter,
                    listeners: {
                        scope: this,
                        keyup: function (field, e) {
                            if (this.oldValue == field.getValue())
                                return;

                            this.oldValue = field.getValue();
                            if (this.filterTask)
                                this.filterTask.cancel();
                            if (e.getKey() == e.ENTER) {
                                this.filterData(this.oldValue);
                            } else {
                                this.filterTask = new Ext.util.DelayedTask(this.filterData, this, [this.oldValue]);
                                this.filterTask.delay(300);
                            }
                        }
                    }
                }]

            }]
        });
        //    html: Locale.APP_TOOLTIP_CREATE_VERSION
        //});
        this.callParent(arguments);
    },

    /**
    *
    */
    onAfterRender: function () {

        App.EntitiesMgr.on('entities', this.onUpdate, this);
        App.EntitiesMgr.on('groups', this.onGroups, this);

        Ext.create('Ext.tip.ToolTip', {
            target: this.down('[name=filter]').el,
            html: Locale.APP_NAVIGATION_SEARCH
        });

        //this.onUpdate(null, App.EntitiesMgr.getEntities());
    },


    filterData: function (filterValue) {
        this.filter = filterValue;
        this.setTree(this.entities);
        if (!Ext.isEmpty(filterValue))
            this.store.getRootNode().expandChildren(true);
    },

    getExpanded: function () {
        var expanded = {};
        var fn = function (node) {
            if (node.isLeaf())
                return true;
            if (node.isExpanded())
                expanded[node.data.text] = true;
            node.eachChild(fn);
            return true;
        };
        this.store.getRootNode().eachChild(fn);
        return expanded;
    },


    setExpanded: function (expanded) {

        var fn = function (node) {
            if (node.isLeaf())
                return true;
            if (expanded[node.data.text])
                node.expand();
            node.eachChild(fn);
            return true;
        };
        this.store.getRootNode().eachChild(fn);

    },


    makeGroups: function (groupArray) {
        var result = {};
        Ext.each(groupArray, function (item) {
            result[item.id] = item;
        });
        return result;

    },

    getEntityTypes: function () {
        var result = {};
        Ext.iterate(this.entityTypes, function (itemId) {
            var item = this.entityTypes[itemId];
            result[itemId] = { children: [], order: item.order };
        }, this);
        return result;
    },

    makeTree: function (data) {
        var me = this;
        var temp = me.getEntityTypes();

        var filterFn = Ext.isEmpty(this.filter) ? function () {
            return true;
        } : function (arg) {
            return arg ? arg.toLowerCase().indexOf(me.filter.trim().toLowerCase()) != -1 : false;
        };

        var pushEnt = function (item) {
            var parent = temp[item.identitytype];
            if (item.identitygroup) {
                var groups = [];
                var group = me.groups['' + item.identitygroup];
                groups.push(group);
                while (group.idparent != null) {
                    group = me.groups[group.idparent];
                    groups.push(group);
                }
                group = groups.pop();
                while (group) {
                    if (!parent[group.caption])
                        parent[group.caption] = { children: [], order: group.order };
                    parent = parent[group.caption];
                    group = groups.pop();
                }
            }
            parent.children.push(Ext.apply({
                text: Ext.isEmpty(me.filter) ? item[data.captionField] : item[data.captionField].replace(new RegExp(me.filter, 'gi'), function (was) { return "<span style ='background:yellow'>" + was + "</span>" }),
                leaf: true
            }, item));

        };

        Ext.each(data.result, function (item) {

            if ((!item.hidden) && filterFn(item[data.captionField])) {
                pushEnt(item);
            }
        });

        var orderer = function(parent) {
            var clone = {},
                properitesInOrder = [],
                orders = [];
            
            Ext.iterate(parent, function (caption, item) {
                var ord = item && item.order ? item.order : 0;
                clone[caption] = ord;
                orders.push(ord);
            });

            orders = orders.sort(function (a, b) {
                return a - b;
            });

            var i = 0;
            while (!Ext.Object.isEmpty(clone)) {
                Ext.iterate(clone, function (caption, ord) {
                    if (ord == orders[i]) {
                        properitesInOrder.push(caption);
                        delete clone[caption];
                        i++;
                        return;
                    }
                });
            }
            
            return properitesInOrder;
        };

        var toTree = function (parent) {
            var children = [];
            var properitesInOrder = orderer(parent);
            Ext.each(properitesInOrder, function (caption) {
                if (caption == 'children' || caption == 'order')
                    return true;
                var item = parent[caption];
                children.push({
                    text: caption,
                    leaf: false,
                    children: toTree(item).concat(item.children)
                });
            }, this);
            return children;
        };

        var test = {};
        Ext.iterate(temp, function(i) {
            test[this.entityTypes[i].caption] = temp[i];
        }, this);

        return {
            text: Locale.APP_MENU_REFERENCES,
            expanded: true,
            leaf: false,
            children: toTree(test)
        };
    },

    setTree: function (data) {
        if (this.groups) {
            var expanded = this.entities ? this.getExpanded() : [];
            var tree = this.makeTree(data);
            this.store.setRootNode(tree);
            this.setExpanded(expanded);
        }
        this.entities = data;
    },

    /**
    *
    */
    onUpdate: function (sender, result) {
        this.setTree(result);
    },

    onGroups: function (sender, groups) {
        this.groups = this.makeGroups(groups);
        this.setTree(this.entities);
    },

    /**
    *
    */
    collapseNodes: function () {

        var toolbar = this.down('toolbar');
        toolbar.disable();

        this.collapseAll(function () {
            toolbar.enable();
        });
    },

    /**
    *
    */
    expandNodes: function () {

        this.getEl().mask(Locale.APP_WAIT_EXPANDING);
        var toolbar = this.down('toolbar');
        toolbar.disable();
        this.expandAll(function () {
            this.getEl().unmask();
            toolbar.enable();
        });
    },

    /**
    *
    */
    onClick: function (tree, record, item, index, e, opts) {

        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Кликнули на элементе дерева');
        }
        //</debug>

        if (Ext.isString(record.raw.action)) {
            // специальные узлы
            if (record.raw.action == 'link') {
                var request = Ext.create('App.logic.HttpRequest', {
                    method: 'GET',
                    url: record.raw.link
                });
                request.submit();
            } else {
                var actionNode = Ext.create(record.raw.action, record.raw);
                actionNode.open();
            }
            return;
        }
            
        if (Ext.isEmpty(record.get('id')))
            return;

        Ext.create('App.logic.factory.NavigationPanel', {
            record: record
        }).createPanel();
    }
   
});
