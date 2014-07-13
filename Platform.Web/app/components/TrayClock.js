/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.components.TrayClock
* @extends Ext.toolbar.TextItem
*/
Ext.define('App.components.TrayClock', {
	extend: 'Ext.toolbar.TextItem',

	alias: 'widget.trayclock',

	cls: 'ux-desktop-trayclock',

	html: '&#160;',

	timeFormat: 'g:i A',

	initComponent: function () {

		this.callParent();

		Ext.TaskManager.start({
			run: function() {

				this.setText(
					Ext.Date.format(new Date(), this.timeFormat)
				);
			},
			interval: 1000,
			scope: this
		});
	},
	
	setText: function (text) {
	    var me = this;
	    me.text = text;
	    if (me.rendered) {
	        me.el.update(text);
	 //       me.updateLayout();
	    }
	}
});
