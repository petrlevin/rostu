/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Number
* @extends Ext.form.field.Number
* @mixins App.components.mixins.FormField
* Числовое поле
*/

Ext.define('App.components.fields.Number', {
    extend: 'Ext.form.field.Number',
    alias: 'widget.app_numberfield',

    requires: [
        'App.components.mixins.FormField',
        'App.components.mixins.CommonField'
    ],
	
    /**
	* Используемые данным объектом mixins
	* @private
	*/
    mixins: {
        formfield: 'App.components.mixins.FormField',
        commonfield: 'App.components.mixins.CommonField'
    },

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function(config) {

        this.getTriggerMarkup = Ext.form.field.Trigger.prototype.getTriggerMarkup;

        // Это нужно! При заданных в валидаторе правилах minValue|maxValue 
        // Оно при изменении значения пытается активировать кнопочки вверх-вниз
        this.setSpinDownEnabled = Ext.emptyFn;
        this.setSpinUpEnabled = Ext.emptyFn;

        config = Ext.apply(config, {
            keyNavEnabled: false,
            mouseWheelEnabled: false
        });
        
        this.callParent([config]);
    },

    trigger1Cls: 'icon-trigger-calculator',

    trigger2Cls: undefined,

    onTrigger1Click: function () {

        var calc = Ext.create('App.components.FormulaCalculator', {
            numberField: this
        });
        App.WindowMgr.add({
            layout: 'fit',
            getForm: function () { return calc; }
        }, {
            height: 400,
            width: 300,
            modal: true,
            title: 'Калькулятор формул',
            maximizable: false
        }, false);
    }
});
