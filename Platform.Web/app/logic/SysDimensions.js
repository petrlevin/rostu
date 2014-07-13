/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.SysDimensions
* Description
*/
Ext.define('App.logic.SysDimensions', {

	requires:[
		'App.logic.factory.Store',
		'App.view.SysDimensions',
		'Ext.data.Store'
	],

	constructor: function(config) {
		Ext.apply(this, config);

		this.factory = Ext.create('App.logic.factory.Store', {});
		this.ppo = this.factory.dimension('-1946157026', ['id', 'caption'], this.onPpoLoad, this);
	},

	onPpoLoad: function(sender, records, successfull, eOpts) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('PPO store loaded');
		}
		// </debug>
		this.budget = this.factory.dimension('-1879048154', ['id', 'caption', 'year', 'idpubliclegalformation'], this.onBudgetLoad, this);
	},

	onBudgetLoad:function(sender, records, successfull, eOpts) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Budget store loaded');
		}
		// </debug>
		this.version = this.factory.dimension('-1677721573', ['id', 'caption', 'idpubliclegalformation'], this.onVersionLoad, this);
	},

	getPpoStore: function () {
	    var data = [];
	    this.ppo.each(function (record) {
	        data.push({
	            'id': record.get('id'),
	            'caption': record.get('caption')
	        });
	    });

	    return Ext.create('Ext.data.Store', {
	        fields: [ 'id', 'caption'],
	        data: data
	    });
	},

	getBudgetStore: function () {

	    return Ext.create('Ext.data.Store', {
	        fields: ['id', 'caption'],
	        data: []
	    });
	},

	getVersionStore: function () {

	    return Ext.create('Ext.data.Store', {
	        fields: ['id', 'caption'],
	        data: []
	    });
	},

	onVersionLoad: function (sender, records, successfull, eOpts) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Version store loaded');
		}
	    // </debug>

	    Ext.create('App.view.SysDimensions', {
            parent: this,
			ppo: this.getPpoStore(),
			budget: this.getBudgetStore(),
			version: this.getVersionStore(),
			handler: this.handler,
	        canCanceled: this.hasSaved,
			scope: this.scope
	    }).show();
	}
});