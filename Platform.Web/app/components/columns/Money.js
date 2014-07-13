/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
*
*/
/**
* @class App.components.columns.Money
* @extends App.components.columns.Number
* Денежная колонка
*/

Ext.define('App.components.columns.Money', {
    extend: 'App.components.columns.Number',

    

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

        config = Ext.apply(config, {
            align: 'right'
        });

        this.data = Ext.apply({}, config);
        this.callParent([config]);
    }
});
