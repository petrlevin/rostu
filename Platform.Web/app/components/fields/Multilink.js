/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Multilink
* @extends App.components.fields.TableField
* Multilink field
*/

Ext.define('App.components.fields.Multilink', {
    extend: 'App.components.fields.TableField',
    alias: 'widget.app_multilink',

    uses: [
        'App.logic.EntitiesList'
    ],

    requires: [
        'App.components.mixins.CommonField',
        'App.components.Search',
        'Ext.window.MessageBox',
        'Ext.Action'
    ],
    
    mixins: {
        commonfield: 'App.components.mixins.CommonField'
    },

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function(config) {
        var proxy, form;

        config.id = config.id || Ext.id();
        this.parentEntity = config.entity.entity;
        Ext.apply(this, config);

        this.list = Ext.create('App.logic.EntitiesList', {
            preventLoad: true,
            usePlainGrid: true,
            field_id: config.id,
            parent_id: this.owner_id,
            owner_form_id: this.getOwnerFormId(),
            autoShow: false,
            direct_function: DataService.multilinkSource,
            columnNames: config.columnNames,
            model_id: Ext.String.format('tp-{0}-{1}', config.initialModel.identity, config.initialModel.id),
            entity_id: config.initialModel.identity,
            override_entity_id: config.initialModel.identitylink,
            getDefaults: this.getDefaults.bind(this),
            getActions: this.getActions.bind(this),
            onEntityUpdate: this.onEntityUpdate.bind(this),
            getTools: function () {
                return [
                    Ext.create('App.components.Maximize', {
                        parentForm: this
                    })
                ]
            },
            getVersioning: function () {
                return false;
            }
        });
        this.grid = this.list.getConfig();
        this.grid.on('afterrender', this.initWatch, this);
        this.grid.on('afterrender', function () { this.list.owner_form_id = this.getOwnerFormId(); }, this);

        config = Ext.apply(config, {
            border: false,
            height: 320,
            layout: 'fit',
            labelAlign: 'top',
            items: [
                this.grid
            ]
        });
        this.callParent([config]);

        Ext.apply(this.grid, {
            getParent: App.components.mixins.CommonField.prototype.getParent,
            getDependencies: App.components.mixins.CommonField.prototype.getDependencies,
            getDependencyValue: App.components.mixins.CommonField.prototype.getDependencyValue
        });
    },

    refresh: function (forced) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log(Ext.String.format('Обновление мультилинка, поле {0}', this.name));
        }
        //</debug>

        this.list.refreshList([
            { name: 'fieldid', value: this.initialModel.id },
            { name: 'docid', value: this.docid },
            { name: 'srcentityid', value: this.parentEntity.id },
            { name: 'fieldvalues', value: this.getDependencies() }
        ], forced);
    },
    
    /**
    *
    * @private
    */
    onEntityUpdate: function (sender, entityId, docId) {

        if (entityId && entityId === this.initialModel.identitylink) {

            this.refresh();
        }
    },

    getActions: function () {
        var result = null;

        this.searchField = Ext.create('App.components.Search', {
            width: 200
        });
        this.searchField.on('search', this.onSearch, this);
        result = [
            this.list.Buttons.create({
                handler: this.createMultilink,
                scope: this
            }),
            this.list.Buttons.open({
                handler: this.openItem,
                scope: this
            }),
            this.list.Buttons.del({
                handler: this.deleteMultilink,
                scope: this
            }),
            this.list.Buttons.refreshgrid({
                handler: this.refresh,
                scope: this
            }),
            /*
            заменил кнопки на фабричные
            Ext.create('Ext.Action', {
                iconCls: 'icon-page',
                text: Locale.APP_BUTTON_CREATE,
                handler: this.createMultilink,
                scope: this
            }),
            Ext.create('Ext.Action', {
                iconCls: 'icon-delete',
                text: Locale.APP_BUTTON_DELETE,
                handler: this.deleteMultilink,
                scope: this
            }),
            Ext.create('Ext.Action', {
                iconCls: 'icon-refresh',
                handler: this.refresh,
                scope: this
            }), 
            */
            '->',
            this.searchField
        ];
        // Добавление "утилитных" кнопок
        result.push(Ext.create('App.components.Maximize', {
            parentForm: this
        }));
        return result;
    },

    /**
    * Обработчик события поиска
    * @private
    */
    onSearch: function (sender) {

        this.refresh();
    },

    openItem: function (sender) {

        var record = this.list.grid.getSelectionModel().getLastSelected();
        this.list.onItemDblClick(sender, record);
    },

    /**
    *
    * @private
    */
    onSelectItem: function (sender) {

        var window = sender.up('window');
        if (window) {
            var sm = this.dialog.grid.getSelectionModel();
            if (sm && sm.hasSelection()) {
                var value = this.dialog.getValue();
                CommunicationDataService.createMultilink(this.initialModel.identitylink, this.parentEntity.id, this.docid, [value.id], function () {
                    App.EntitiesMgr.registerUpdate(this.initialModel.identitylink, null);
                }, this);
                window.destroy();
            } else {
                Ext.MessageBox.show({
                    title: Locale.APP_MESSAGE_FORM_WARNING,
                    msg: Locale.APP_MESSAGE_SHOULD_BE_SELECTED,
                    width: 300,
                    buttons: Ext.MessageBox.OK,
                    icon: Ext.MessageBox.WARNING
                })
            }
        } else {
            //<debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Dictionary link field > select item');
                Ext.Exception.raise('Could not find window!');
            }
            //</debug>
        }
    },

    createMultilink: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 1, select');
        }
        //</debug>
        if (Ext.isDefined(this.dialog)) {
            this.dialog.show();
        } else {
            var listId = Ext.id();
            this.dialog = Ext.create('App.logic.EntitiesList', {
                id: listId,
                field_id: this.id,
                entity_id: this.initialModel.identitylink,
                idOwner: this.parentEntity.id,
                owner_form_id: this.getOwnerFormId(),
                iconCls: 'icon_selection',
                modal: true,
                formType: 3, // ToDo{CORE-280} тип формы: форма выбора
                openAsWindow: true,
                customGrid: {
                    extendedActions: [
                        Ext.create('Ext.Action', {
                            id: listId + '-button-select',
                            text: Locale.APP_BUTTON_SELECT,
                            handler: this.onSelectItem,
                            scope: this
                        })
                    ],
                    listeners: {
                        scope: this,
                        itemdblclick: this.onSelectItem,
                        selectionchange: this.onSelectionChange
                    }
                },
                customWindow: {
                    modal: true
                }
            });

            this.dialog.on('beforerequest', function (sender, proxy) {

                proxy.setExtraParam('fieldid', this.initialModel.id);
            }, this);
        }
    },

    deleteMultilink: function() {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log(
                Ext.String.format('Deleting items from current entity "{0}"', this.entity.name)
            );
        }
        // </debug>

        var sm = this.grid.getSelectionModel();
        if (sm) {
            var rows = sm.getSelection();
            if (rows.length > 0) {
                Ext.MessageBox.confirm(Locale.APP_DELETEITEM_TITLE, "Вы действительно хотите удалить выделенные элементы?",
                    function (button) {
                        if (button === "yes") {
                            var items = [];
                            Ext.each(rows, function (item) {
                                items.push(item.get("id"));
                            }, this);
                            CommunicationDataService.deleteMultilink(this.initialModel.identitylink, this.parentEntity.id, this.docid, items, function () {
                                App.EntitiesMgr.registerUpdate(this.initialModel.identitylink, null);
                            }, this);
                        }
                    }, this);
            } else {
                Ext.MessageBox.alert(Locale.APP_DELETEITEM_TITLE, Locale.APP_DELETEITEM_SHOULD_SELECT);
            }
        }
    },

    getDefaults: function () {
        var result = {};

        result[this.ownerfield.name.toLowerCase()] = this.docid;
        result[this.ownerfield.name.toLowerCase() + '_caption'] =
            this.entity.elementData.result[0][this.model.captionField];

        return result;
    },
    
    /**
    * @private
    *
    */
    onSelectionChange: function (sender, selected, eOpts) {

        this.dialog.checkIsSelectable();
    }
});
