/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.controller.Application
* @extends Ext.app.Controller
* Main application controller
*/

Ext.define('App.controller.Application', {
	extend: 'Ext.app.Controller',

	requires: [
		'App.components.Navigation',
		'App.view.Viewport',
		'Ext.state.CookieProvider',
		'Ext.direct.Manager'
	],

	refs: [{
		ref: 'viewport', selector: 'viewport'
	}],

	init: function(application) {

		Ext.state.Manager.setProvider(
			Ext.create('Ext.state.CookieProvider', {
				prefix : '',
				expires : new Date(new Date().getTime() + (1000*60*60*24*3)) // 3 days
			})
		);

        Ext.direct.Manager.on("exception", function(event, eOpts) {
	        if (event.where &&
	            (event.where.indexOf('NotLogedException') >= 0 ||
	                event.where.indexOf('IsInteractive') >= 0 ||
	                event.where.indexOf('NotSysDimensionsException') >= 0 ||
	                event.where.indexOf('NotLicensedException') >= 0 ||
	                event.where.indexOf('NotBandWidthException') >= 0))
	            return;

	        App.exceptions.Handler.handle(event);
	    });
	},

	onLaunch: function(application) {

		App.Profile.on('logged_in', this.onLoginSuccess, this);
		App.Profile.on('logged_out', this.onLogoutSuccess, this);
		App.Profile.on('auth_failed', this.onFailedLogin, this);
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Application contoller launched');
		}
		// </debug>
	},

	onLoginSuccess: function(sender, params) {
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Successfull login to system');
			Ext.global.console.dir(params);
		}
	    //</debug>
		if (!this.configured || params.restart) {
		    var viewport = this.getViewport();

		    if (viewport) {
		        viewport.removeAll();
		        Ext.destroy(viewport);
		        window.location.reload();
		    }

		    viewport = Ext.create('App.view.Viewport');
            App.WindowMgr.configure(viewport);
		    
		    App.StatusMgr.init();
	        this.setTitle();
	        this.configured = true;
	    }
	},

	onLogoutSuccess: function() {
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Successfull logout from system');
		}
		//</debug>

		this.getViewport().removeAll();
		this.getViewport().destroy();
		window.location.reload();
	},

	onFailedLogin: function(sender) {
	},
	
	setTitle: function() {
	    SystemService.getTitle(function (result) {
	        document.title = result;
	    }, this);
	}
});