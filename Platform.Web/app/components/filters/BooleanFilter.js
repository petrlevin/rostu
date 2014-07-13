/**
 * Boolean filters use unique radio group IDs (so you can have more than one!)
 */
Ext.define('App.components.filters.BooleanFilter', {
    extend: 'Ext.ux.grid.filter.BooleanFilter',
    alias: 'gridfilter.extboolean',

	/**
	 * @cfg {String} yesText
	 * Defaults to 'Yes'.
	 */
	yesText : 'Истина',

	/**
	 * @cfg {String} noText
	 * Defaults to 'No'.
	 */
	noText : 'Ложь',

    /**
     * @private
     * Template method that is to get and return serialized filter data for
     * transmission to the server.
     * @return {Object/Array} An object or collection of objects containing
     * key value pairs representing the current configuration of the filter.
     */
    getSerialArgs : function () {

        return {
            comparison: App.enums.ComparisionOperator.Equal,
            value: this.getValue()
        };
    }
});
