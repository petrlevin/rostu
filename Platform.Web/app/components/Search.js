/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.Search
* @extends Ext.form.field.Trigger
* Компонент поиска для гридов
*/

Ext.define('App.components.Search', {
    extend: 'Ext.form.field.Trigger',

    requires: [
        'Ext.util.Observable'
    ],

    /**
	* Используемые данным объектом mixins
	* @private
	*/
    mixins: {
        /**
		* Реализация событийной модели
		*/
        observable: 'Ext.util.Observable'
    },

    /**
    *
    */
    trigger1Cls: 'x-form-search-trigger',

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field constructor');
        }
        //</debug> 
        Ext.apply(this, config);
        this.callParent([config]);

        this.on('change', function (sender, newValue, oldValue) {
            this.fieldValue = newValue;
        }, this);
        this.on('specialkey', function (field, event) {
            if (event.getKey() === event.ENTER) {
                this.fireEvent('search', this, this.fieldValue);
            }
        }, this);

        this.mixins.observable.constructor.call(this, config);
        this.addEvents('search');
    },

    /**
    *
    */
    onTrigger1Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 1, select');
        }
        //</debug>
        this.fireEvent('search', this, this.fieldValue);
    },

    __Value: function () {

        return this.fieldValue;
    },
    
    setAvailability: function (entityItem) {
        this.enable(); // всегда доступна
    }
});
