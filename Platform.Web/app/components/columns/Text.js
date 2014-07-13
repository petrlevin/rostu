/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.columns.Text
* @extends Ext.grid.column.Column
* Текстовая колонка
*/

Ext.define('App.components.columns.Text', {
    extend: 'Ext.grid.column.Column',

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Standard column constructor');
        }
        //</debug>

        this.data = Ext.apply({
		filterable: true,
		filter: {
			type: 'extstring'
		}
	}, config);
        this.callParent([ this.data ]);
    }
});
