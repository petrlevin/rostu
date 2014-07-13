/**
 * Filters using an Ext.ux.grid.menu.RangeMenu.
 */
Ext.define('App.components.filters.NumericFilter', {
    extend: 'Ext.ux.grid.filter.NumericFilter',
    alias: 'gridfilter.extnumeric',

    /**
    * @cfg {Object} iconCls
    * The iconCls to be applied to each comparator field item.
    */
    iconCls: {
        gt: 'ux-rangemenu-gt',
        lt: 'ux-rangemenu-lt',
        ge: 'ux-rangemenu-ge',
        le: 'ux-rangemenu-le',
        ne: 'ux-rangemenu-ne',
        eq: 'ux-rangemenu-eq'
    },

    /**
    * @cfg {Array} menuItems
    * The items to be shown in this menu.  Items are added to the menu
    * according to their position within this array.
    */
    menuItems: ['lt', 'gt', 'le', 'ge', 'ne', 'eq'],

    /**
     * @cfg {Object} fieldLabels
     * Accessible label text for each comparator field item. Can be overridden by localization
     * files.
     */
    fieldLabels: {
        gt: 'Больше чем',
        lt: 'Меньше чем',
        ge: 'Больше или равно',
        le: 'Меньше или равно',
        ne: 'Не равно',
        eq: 'Равно'
    },

    /**
     * @private
     * Template method that is to initialize the filter and install required menu items.
     */
    init : function (config) {

	    this.callParent(arguments);
    },

    /**
     * @private @override
     * Creates the Menu for this filter.
     * @param {Object} config Filter configuration
     * @return {Ext.menu.Menu}
     */
    createMenu: function(config) {
        var me = this,
            menu;
        menu = Ext.create('App.components.menu.RangeMenu', Ext.apply(config, {
            menuItems: this.menuItems,
            itemIconCls: this.iconCls,
            fieldLabels: this.fieldLabels,
            dataKeys: {
                gt: App.enums.ComparisionOperator.Greater,
                lt: App.enums.ComparisionOperator.Less,
                eq: App.enums.ComparisionOperator.Equal,
                ge: App.enums.ComparisionOperator.GreaterOrEqual,
                le: App.enums.ComparisionOperator.LessOrEqual,
                ne: App.enums.ComparisionOperator.NotEqual
            }
        }));
        menu.on('update', me.fireUpdate, me);
        return menu;
    },

    /**
     * @private
     * Template method that is to get and return serialized filter data for
     * transmission to the server.
     * @return {Object/Array} An object or collection of objects containing
     * key value pairs representing the current configuration of the filter.
     */
    getSerialArgs : function () {

        return this.menu.getValue();
    }
});
