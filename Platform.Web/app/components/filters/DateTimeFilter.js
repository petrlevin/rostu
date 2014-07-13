Ext.define('App.components.filters.DateTimeFilter', {
    extend: 'Ext.ux.grid.filter.DateTimeFilter',
    alias: 'gridfilter.extdatetime',

    uses: [
        'Ext.picker.Time'
    ],

    /**
     * @cfg {String} afterText
     * Defaults to 'After'.
     */
    afterText: 'После даты',

    /**
     * @cfg {String} beforeText
     * Defaults to 'Before'.
     */
    beforeText: 'До даты',

    /**
     * @cfg {String} onText
     * Defaults to 'On'.
     */
    onText: 'На дату',

    /**
     * @cfg {Date} maxDate
     * Allowable date as passed to the Ext.DatePicker
     * Defaults to undefined.
     */
    maxDate: new Date(3000, 0, 1, 0, 0, 0),

    /**
     * @cfg {Date} minDate
     * Allowable date as passed to the Ext.DatePicker
     * Defaults to undefined.
     */
    minDate: new Date(1970, 0, 1, 0, 0, 0),

    dateFormat: 'd.m.Y',

    /**
     * @private
     * Template method that is to initialize the filter and install required menu items.
     */
    init: function (config) {
        var me = this,
            dateCfg = Ext.applyIf(me.date || {}, me.dateDefaults),
            timeCfg = Ext.applyIf(me.time || {}, me.timeDefaults),
            dockCfg = me.dock, // should not default to empty object
            defaultListeners = {
                click: {
                    scope: me,
                    click: me.onMenuSelect
                },
                select: {
                    scope: me,
                    select: me.onMenuSelect
                }
            },
            pickerCtnCfg, i, len, item, cfg,
            items = [dateCfg, timeCfg],

            // we need to know the datepicker's position in the items array
            // for when the itemId name is bound to it before adding to the menu
            timepickerPosition = 1,
            datepickerPosition = 0;

        Ext.apply(me, {
            compareMap: {
                before: App.enums.ComparisionOperator.Less,
                after: App.enums.ComparisionOperator.Greater,
                on: App.enums.ComparisionOperator.InSameDate
            }
        });

        if (!me.positionDatepickerFirst) {
            items = items.reverse();
            timepickerPosition = 0;
            datepickerPosition = 1;
        }

        pickerCtnCfg = Ext.apply(me.pickerOpts, {
            xtype: !dockCfg ? 'container' : 'panel',
            layout: 'hbox',
            items: items
        });

        // If there's no dock config then bind the default listener to the desired picker.
        if (!dockCfg) {
            if (me.selectDateToFilter) {
                dateCfg.listeners = defaultListeners.select;
            } else {
                timeCfg.listeners = defaultListeners.select;
            }
        } else if (dockCfg) {
            me.selectDateToFilter = null;

            if (dockCfg.dockedItems) {
                pickerCtnCfg.dockedItems = dockCfg.dockedItems;
                // TODO: allow config that will tell which item to bind the listener to
                // right now, it's using the first item
                pickerCtnCfg.dockedItems.items[dockCfg.bindToItem || 0].listeners = defaultListeners.click;
            } else {
                // dockCfg can be `true` if button text and dock position defaults are wanted
                if (Ext.isBoolean(dockCfg)) {
                    dockCfg = {};
                }
                dockCfg = Ext.applyIf(dockCfg, me.dockDefaults);
                pickerCtnCfg.dockedItems = {
                    xtype: 'toolbar',
                    dock: dockCfg.dock,
                    items: [
                        {
                            xtype: 'button',
                            text: dockCfg.buttonText,
                            flex: 1,
                            listeners: defaultListeners.click
                        }
                    ]
                };
            }
        }

        me.fields = {};
        for (i = 0, len = me.menuItems.length; i < len; i++) {
            item = me.menuItems[i];
            if (item !== '-') {
                pickerCtnCfg.items[datepickerPosition].itemId = item;
                pickerCtnCfg.items[timepickerPosition].hidden = item === 'on';
                cfg = {
                    itemId: 'range-' + item,
                    text: me[item + 'Text'],
                    menu: Ext.create('Ext.menu.Menu', {
                        items: pickerCtnCfg
                    }),
                    listeners: {
                        scope: me,
                        checkchange: me.onCheckChange
                    }
                };
                item = me.fields[item] = Ext.create('Ext.menu.CheckItem', cfg);
            }
            me.menu.add(item);
        }
        me.values = {};
    },

    /**
     * @private
     * Template method that is to get and return serialized filter data for
     * transmission to the server.
     * @return {Object/Array} An object or collection of objects containing
     * key value pairs representing the current configuration of the filter.
     */
    getSerialArgs: function () {
        var me = this,
            key,
            fields = me.fields,
            args = [];

        for (key in fields) {
            if (fields[key].checked) {
                args.push({
                    comparison: me.compareMap[key],
                    value: me.getFieldValue(key)
                });
            }
        }
        return args;
    }
});
