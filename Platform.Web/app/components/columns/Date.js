﻿/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.columns.DateTime
* @extends Ext.grid.column.Date
* Колонка с датой и временем
*/

Ext.define('App.components.columns.Date', {
    extend: 'Ext.grid.column.Date',

    /**
    * @cfg {String} String with format for output of datetime
    */
    format: 'd.m.Y',

    width: 70,

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('DateTime column > Standard constructor');
        }
        //</debug>

        this.data = Ext.apply({
		filterable: true,
		filter: {
			type: 'extdatetime',
			dateFormat: this.format
		}
	}, config);
        this.callParent([ this.data ]);
    }
});
