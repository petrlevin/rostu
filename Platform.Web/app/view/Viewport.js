/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*
*/
/**
* @class App.view.Viewport
* @extends Ext.Viewport
* Основной Viewport приложения
*/

Ext.define('App.view.Viewport', {
	extend: 'Ext.Viewport',

	requires: [
        'App.components.menu.RangeMenu',
		'App.components.Navigation',
		'App.components.TrayClock',
		'App.components.TaskBar',
		'App.view.Desktop',
		'Ext.layout.container.VBox'
	],

	layout: {
		type: 'vbox',
		align: 'stretch',
		pack: 'start'
	},

	renderTo: Ext.getBody(),

	getStartMenuCfg: function () {

		return {
			title: App.Profile.getUserName(),
			iconCls: 'ux-user',
			height: 300,

			toolbar: [{
			    text: Locale.APP_MENU_CHANGE_SYSDIMENSIONS,
			    iconCls: 'ux-logout',
			    handler: App.Profile.changeSysDimensions,
			    scope: App.Profile
			}, {
				text: Locale.APP_MENU_LOGOUT,
				iconCls: 'ux-logout',
				handler: App.Profile.logout,
				scope: App.Profile
			}, {
			    text: Locale.APP_MENU_CHANGE_PASWORD,
			    iconCls: 'ux-logout',
			    handler: App.Profile.changePassword,
			    scope: App.Profile
			}],

			menu: [{
				id: 'entities-menu',
				text: Locale.APP_MENU_REFERENCES,
				iconCls: 'ux-reference',
				menu: App.EntitiesMgr.getEntities()
			}]
		};
	},

	initComponent: function () {

		var me = this;
		Ext.apply(me, {
			items : [
				Ext.create('App.view.Desktop', {}),
				this.taskbar = Ext.create('App.components.TaskBar', {
					startBtnText: Locale.APP_BUTTON_START,
					startMenuItems: me.getStartMenuCfg(),
					trayItems: [
						Ext.create('App.components.TrayClock', {
							flex: 1
						})
					],
					quickLaunchItems: [{
						text: Locale.APP_QUICKSTART_MINIMIZE,
						iconCls: 'ux-desktop',
						handler: me.minimizeWindows,
						scope: me
					}, {
						text: Locale.APP_QUICKSTART_CASCADE,
						iconCls: 'ux-cascade',
						handler: me.cascadeWindows,
						scope: me
					}]
				})
			]
		});
		this.callParent(arguments);
	}
});