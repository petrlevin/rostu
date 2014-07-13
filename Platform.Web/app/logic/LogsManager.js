/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.LogsManager
* @mixins Ext.util.Observable
* Основной менеджер окна отчета
*/

Ext.define("App.logic.LogsManager", {

	requires: [
		'Ext.data.ArrayStore',
		'Ext.util.Observable'
	],

	INFO : 'info',
	WARNING : 'warn',
	ERROR: 'error',

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
		/**
		* Реализация событийной модели
		*/
		observable : 'Ext.util.Observable'
	},

	logMaximum: 100,

	log: function(line, severity) {

		this.logs.add({
			'text': line,
			'severity': severity
		});

		if(this.logs.getCount() > this.logMaximum) {
			this.logs.removeAll();
			this.logs.add({
				'text': 'Log was cleared due to overload',
				'severity': this.INFO
			});
		}
		this.fireEvent('update', this, this.logs);
	},

	getStore: function() {

		return this.store;
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function(config) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Logs manager constructor');
		}
		// </debug>

		this.logs = Ext.create('Ext.data.ArrayStore', {
			storeId: 'logsstore',
			fields: [
				{ name: 'severity', type: 'string' },
				{ name: 'text', type: 'string'}
			]
		});

		this.mixins.observable.constructor.call(this, config);
		this.addEvents('update');
	}
});
