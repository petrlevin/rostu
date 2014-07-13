Ext.define('App.components.filters.LinkFilter', {
    extend: 'Ext.ux.grid.filter.StringFilter',
    alias: 'gridfilter.extlink',

    /**
     * @cfg {String} iconCls
     * The iconCls to be applied to the menu item.
     * Defaults to <tt>'ux-gridfilter-text-icon'</tt>.
     */
    iconCls: 'ux-gridfilter-text-icon',

    emptyText: 'Enter Filter Text...',

    listValue: null,

    /**
     * @private
     * Template method that is to initialize the filter and install required menu items.
     */
    init: function (config) {
        Ext.applyIf(config, {
            enableKeyEvents: true,
            labelCls: 'ux-rangemenu-icon ' + this.iconCls,
            hideEmptyLabel: false,
            labelSeparator: '',
            labelWidth: 29,
            listeners: {
                scope: this,
                keyup: this.onInputKeyUp,
                el: {
                    click: function (e) {
                        e.stopPropagation();
                    }
                }
            }
        });

        this.inputItem = Ext.create('Ext.form.field.Text', config);
        this.menu.add(this.inputItem);

        // Фильтры Null/Not null
        var gId = Ext.id();
        this.nullable = new Ext.menu.CheckItem({
            text: 'Пусто',
            group: gId,
            checked: false,
            listeners: {
                scope: this,
                'checkchange': this.onCheckChange
            }
        });
        this.notNull = new Ext.menu.CheckItem(new Ext.menu.CheckItem({
            text: 'Не пусто',
            group: gId,
            checked: true,
            listeners: {
                scope: this,
                'checkchange': this.onCheckChange
            }
        }));

        this.menu.add('-');
        this.menu.add(this.nullable);
        this.menu.add(this.notNull);
        this.menu.add('-');
        this.menu.add(Ext.create('Ext.menu.Item', {
            text: 'Выбрать',
            handler: this.onSelection,
            scope: this
        }));
        this.updateTask = Ext.create('Ext.util.DelayedTask', this.fireUpdate, this);
    },

    onSelection: function(item, event) {

        this.dialog = Ext.create("App.logic.EntitiesList", {
            entity_id: this.entityId,
            iconCls: 'icon_selection',
            openAsWindow: true,
            autoShow: true,
            modal: true,
            formType: 3, // ToDo{CORE-280} тип формы: форма выбора
            customGrid: {
                selModel: Ext.create("Ext.selection.CheckboxModel", {
                    checkOnly: true
                }),
                flags: {
                    preventDeleteBtn: true
                },
                extendedActions: [
                    Ext.create('Ext.Action', {
                        id: Ext.id() + '-button-select',
                        text: Locale.APP_BUTTON_SELECT,
                        handler: this.onSelectItem,
                        scope: this
                    })
                ],
                listeners: {
                    scope: this,
                    itemdblclick: this.onSelectItem
                }
            }
        });
    },

    onCheckChange: function () {

        this.updateTask.delay(this.updateBuffer);
    },

    /**
     * Template method that is to return <tt>true</tt> if the filter
     * has enough configuration information to be activated.
     * @return {Boolean}
     */
    isActivatable: function () {

        return this.inputItem.getValue().length > 0 ||
            this.nullable.checked ||
            this.notNull.checked ||
            !Ext.isEmpty(this.listValue);
    },

    /**
     * @private
     * Template method that is to get and return serialized filter data for
     * transmission to the server.
     * @return {Object/Array} An object or collection of objects containing
     * key value pairs representing the current configuration of the filter.
     */
    getSerialArgs: function () {
        var result = [];

        if (this.getValue().length > 1) {
            result.push({
                comparison: App.enums.ComparisionOperator.Like,
                value: '%' + this.inputItem.getValue() + '%'
            });
        }
        if (this.nullable.checked) {
            result.push({
                comparison: App.enums.ComparisionOperator.IsNull
            });
        }
        if (this.notNull.checked) {
            result.push({
                comparison: App.enums.ComparisionOperator.IsNotNull
            });
        }
        if (!Ext.isEmpty(this.listValue)) {
            var ids = this.listValue, values = [];
            Ext.each(ids, function (item) {
                values.push(item.get('id'));
            });
            result.push({
                comparison: App.enums.ComparisionOperator.InList,
                value: values
            });
        }
        return result;
    },

    onSelectItem: function (sender) {

        if (this.dialog.checkIsSelectable() == 0)
            return;

        var sm = this.dialog.grid.getSelectionModel();
        if (sm && sm.hasSelection()) {
            this.listValue = sm.getSelection();
            this.updateTask.delay(this.updateBuffer);
        }
        // Прячем окно с гридом, чтобы не создавать каждый раз заново
        sender.up('window').destroy();
    }
});
