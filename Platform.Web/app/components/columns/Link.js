/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.columns.Link
* @extends Ext.grid.column.Column
* Колонка со ссылкой
*/

Ext.define('App.components.columns.Link', {
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

        if (config.dataIndex.indexOf('_caption') === -1) {
            config.dataIndex = Ext.String.format('{0}_caption', config.dataIndex);
        }
        this.callParent([Ext.apply(config, {
            filterable: true,
            filter: {
                type: 'extlink',
                entityId: config.identitylink
            }
        }) ]);
    }
});
