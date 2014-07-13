/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.controller.Login
* @extends Ext.app.Controller
* Login controller
*/

Ext.define('App.controller.Login', {
	extend: 'Ext.app.Controller',

	requires: [
		'App.exceptions.Handler',
		'App.logic.SysDimensions',
		'App.view.Login',
		'Ext.direct.Manager'
	],

	refs: [{
		ref : 'loginWindow',
		selector: 'loginview',
		autoCreate: true,
		xtype: 'loginview'
	}],

	init: function(application) {
	    
		this.control({
			'loginview': {
				login: this.onLoginClick
			}
		});
		
		Ext.direct.Manager.on("exception", function (event, eOpts) {
		    
		    if (event.where && (event.where.indexOf('IsInteractive') >= 0)) {
                return;
		    }

	        if (event.where && (event.where.indexOf('NotLogedException') >= 0)) {
	            App.Profile.checkLogged();
	            return;
	        }
	        
	        if (event.where && (event.where.indexOf('NotSysDimensionsException') >= 0)) {
	            App.Profile.changeSysDimensions();
	            return;
	        }
		    
	        if (event.where && (event.where.indexOf('NotBandWidthException') >= 0)) {
	            App.Profile.checkBandWidth();
	            return;
	        }

	        if (event.where && (event.where.indexOf('NotLicensedException') >= 0)) {
		        App.Profile.showNotLicensedMsg();
		        return;
		    }


		    App.exceptions.Handler.handle(event);
	    }, this);

	},

	onLaunch: function(application) {

		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Showing user login window');
		}
		// </debug>
		App.Profile.on('auth_failed', this.onAuthFailed, this);
		App.Profile.on('need_params', this.onNeedParams, this);
		App.Profile.on('logged_in', this.onLoginSuccess, this);
		App.Profile.on('not_logged', this.onNotLogged, this);

		App.Profile.checkLogged();
	},

	onAuthFailed: function() {

	    this.getLoginWindow().unmask();
	    //Ext.MessageBox.alert(Locale.APP_LOGINFORM_TITLE, Locale.APP_MESSAGE_LOGINFAILED);
	},

	onNotLogged: function() {

		this.getLoginWindow().show();
	},

	onLoginClick: function(sender, form) {
	    this.getLoginWindow().mask();
	    App.Profile.login(form);
	},

	onLoginSuccess: function(sender, response) {

	    this.getLoginWindow().unmask();
	    this.getLoginWindow().hide();
	},

	onNeedParams: function(sender, handler, scope) {
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Need additional parameters to enter the system');
		}
	    //</debug>
		var dimensions = Ext.create('App.logic.SysDimensions', {
		    handler: handler,
            scope: scope
		});
	}
});