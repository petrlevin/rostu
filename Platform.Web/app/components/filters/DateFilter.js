/**
 * Filter by a configurable Ext.picker.DatePicker menu
 *
 * Example Usage:
 *
 *     var filters = Ext.create('Ext.ux.grid.GridFilters', {
 *         ...
 *         filters: [{
 *             // required configs
 *             type: 'date',
 *             dataIndex: 'dateAdded',
 *      
 *             // optional configs
 *             dateFormat: 'm/d/Y',  // default
 *             beforeText: 'Before', // default
 *             afterText: 'After',   // default
 *             onText: 'On',         // default
 *             pickerOpts: {
 *                 // any DatePicker configs
 *             },
 *      
 *             active: true // default is false
 *         }]
 *     });
 */
Ext.define('App.components.filters.DateFilter', {
    extend: 'Ext.ux.grid.filter.DateFilter',
    alias: 'gridfilter.extdate',

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
    init : function (config) {

	    Ext.apply(this, {
		    compareMap: {
			    before: App.enums.ComparisionOperator.Less,
			    after: App.enums.ComparisionOperator.Greater,
			    on: App.enums.ComparisionOperator.InSameDate
		    }
	    });
	    this.callParent(arguments);
    },

    /**
     * @private
     * Template method that is to get and return serialized filter data for
     * transmission to the server.
     * @return {Object/Array} An object or collection of objects containing
     * key value pairs representing the current configuration of the filter.
     */
    getSerialArgs : function () {
        var args = [];
        for (var key in this.fields) {
            if(this.fields[key].checked) {
                args.push({
                    comparison: this.compareMap[key],
                    value: this.getFieldValue(key) // Ext.Date.format(this.getFieldValue(key), this.dateFormat)
                });
            }
        }
        return args;
    }
});
