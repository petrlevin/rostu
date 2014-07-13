/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
/**
* @class App.components.fields.Money
* @extends App.components.fields.Number
* @mixins App.components.mixins.FormField
* Числовое поле
*/

Ext.define('App.components.fields.Money', {
    extend: 'App.ux.NumericField',
    alias: 'widget.app_moneyfield',

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {

        config = Ext.apply(config, {
            'alwaysDisplayDecimals': true,
            'currencySymbol': '',
            'style': '',
            'decimalPrecision': 2,
            'decimalSeparator': ',',
            'alternativeThouthandSeparator': ' '
        });
        
        this.callParent([config]);
    }
});
