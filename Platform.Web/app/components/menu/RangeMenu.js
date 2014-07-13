/**
 * Custom implementation of {@link Ext.menu.Menu} that has preconfigured items for entering numeric
 * range comparison values: less-than, greater-than, and equal-to. This is used internally
 * by {@link Ext.ux.grid.filter.NumericFilter} to create its menu.
 */
Ext.define('App.components.menu.RangeMenu', {
    extend: 'Ext.ux.grid.menu.RangeMenu',

    /**
     * @cfg {Object} itemIconCls
     * The itemIconCls to be applied to each comparator field item.
     */
    itemIconCls : {
        gt : 'ux-rangemenu-gt',
        lt : 'ux-rangemenu-lt',
        eq : 'ux-rangemenu-eq'
    },

    /**
     * @cfg {Object} fieldLabels
     * Accessible label text for each comparator field item. Can be overridden by localization
     * files.
     */
    fieldLabels: {
        gt: 'Больше чем',
        lt: 'Меньше чем',
        eq: 'Равно'
    },

    /**
     * @cfg {Object} menuItemCfgs
     * Default configuration options for each menu item
     */
    menuItemCfgs : {
        emptyText: 'Введите число...',
        selectOnFocus: false,
        width: 155
    },

    constructor: function (config) {

        this.dataKeys = {
            gt: App.enums.ComparisionOperator.Greater,
            lt: App.enums.ComparisionOperator.Less,
            eq: App.enums.ComparisionOperator.Equal
        };
        this.callParent(arguments);
    },

    /**
     * Get and return the value of the filter.
     * @return {String} The value of this filter
     */
    getValue : function () {
        var result = [],
            fields = this.fields, 
            key, field;
            
        for (key in fields) {
            if (fields.hasOwnProperty(key)) {
                field = fields[key];
                if (field.isValid() && field.getValue() !== null) {
                    result.push({
                        comparison: this.dataKeys[key],
                        value: field.getValue()
                    });
                }
            }
        }
        return result;
    }
});
