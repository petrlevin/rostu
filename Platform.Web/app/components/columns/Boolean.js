/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.columns.Boolean
* @extends Ext.ux.CheckColumn
* Булевая колонка
*/

Ext.define('App.components.columns.Boolean', {
    extend: 'Ext.ux.CheckColumn',

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
			type: 'extboolean'
		}
	}, config);
        this.callParent([ this.data ]);
        this.on('beforecheckchange', this.onBeforeCheckChange, this);
    },

    /**
    * Event handler for 'beforecheckchange' event. We returning false to
    * prohibit editing this type of columns.
    * TODO: When grid will be editable - we will add ability to edit it
    */
    onBeforeCheckChange: function () {

        // На данный момент не разрешаем менять в гриде чекбокс
        return false;
    }
});