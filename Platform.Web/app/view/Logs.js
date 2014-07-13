/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.view.Login
* @extends Ext.DataView
* Окно лога/сообщений
*/

Ext.define("App.view.Logs", {
	extend: 'Ext.DataView',

	requires: [
		'Ext.data.StoreMgr',
		'Ext.XTemplate'
	],

	itemSelector: 'div.logs',
	tpl: new Ext.XTemplate(
		'<div class="logs">',
		'	<tpl for=".">',
		'		<div class="log-{severity}">{text}</div>',
		'	</tpl>',
		'</div>'
	),

	initComponent: function() {
		this.on('afterrender', this.onAfterRender, this);
		this.callParent(arguments);
	},

	onAfterRender: function() {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('After render of logs view');
		}
		// </debug>
		var store = Ext.StoreMgr.lookup('logsstore');
		if(store) {
			this.bindStore(store);
		}
	}
});