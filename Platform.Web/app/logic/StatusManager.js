/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.StatusManager
* @mixins Ext.util.Observable
*/

Ext.define("App.logic.StatusManager", {

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

    statusCache: null,

    init : function(){

        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Loading Status manager ...');
        }
        // </debug>

        DataService.getRefStatuses(function (result, response) {
            //
            if (response.type !== 'rpc') {
                return;
            }
            this.statusCache = result;
        }, this);
    },

    getStatusTextById:function(id){
        var result = '';

        Ext.each(this.statusCache, function (item) {
            if (item.id == id) {
                result = item.text;
            }
        });
        return result;
    },

    getMenu: function (handler, scope) {

        var temp = [];
        Ext.each(this.statusCache, function (item) {
            temp.push({
                text: item.text,
                origin: Ext.apply({}, item),
                handler: handler.bind(scope)
            });
        }, this);
        return temp;
    },

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Status manager constructor');
        }
        // </debug>

        this.mixins.observable.constructor.call(this, config);
        this.addEvents('update');
    }
});
