/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.view.ChangePassword
* @extends Ext.panel.Panel
* Рабочий стол приложения
*/

Ext.define('App.view.Desktop', {
    extend: 'Ext.panel.Panel',

    uses: [
        'App.components.Navigation',
		'Ext.container.Container',
        'App.view.Logs',
        'Ext.StoreMgr'
    ],

	flex: 1,
	layout: 'border',
	bodyBorder: false,

	defaults: {
		collapsible: true,
		split: true
	},

	initComponent: function() {

        Ext.apply(this, {
			items: [
				Ext.create('App.components.Navigation', {
					store: Ext.StoreMgr.lookup('navigation'),
					region: 'west',
					layout: 'fit',
					floatable: true,
					width: 175,
					minWidth: 100,
					maxWidth: 250
				}), {
			        id: 'window-container',
				    xtype: 'container',
				    collapsible: false,
				    region: 'center',
				    layout: 'fit',
					flex : 1
				}, {
					xtype: 'panel',
					layout: 'fit',
					region: 'south',
					collapsible: true,
					collapsed: true,
					floatable: false,
					title: Locale.APP_MESSAGES_TITLE,
					height: 150,
					minHeight: 75,
					maxHeight: 250,
					items: [
						Ext.create('App.view.Logs')
					]
			}]
        });
        this.callParent();
	}
});