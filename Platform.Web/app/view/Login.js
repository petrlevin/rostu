/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.view.Login
* @extends Ext.Window
* Login form for application
*/

Ext.define('App.view.Login', {
	extend: 'Ext.Window',
	alias: 'widget.loginview',

	requires: [
		'Ext.form.field.Text',
		'Ext.button.Button',
        	'Ext.form.Panel'
	],

	title: Locale.APP_LOGINFORM_TITLE,
	width: 300,
	closable: false,
	modal: true,

	initComponent: function(config) {

		Ext.apply(this, {
			items: [{
				xtype: 'form',
				bodyStyle: 'padding: 10px;',
				items: [{
					xtype: 'textfield',
					name: 'user',
					fieldLabel: Locale.APP_LOGINFORM_USERNAME,
					listeners: {
					    scope: this,
                        'specialkey': this.onUserName
					}
				}, {
				    xtype: 'textfield',
				    id: 'login-pass-field',
				    name: 'pass',
				    inputType: 'password',
				    fieldLabel: Locale.APP_LOGINFORM_PASSWORD,
				    listeners: {
				        scope: this,
				        'specialkey': this.onSpecialKey
				    }
				}, {
				    xtype: 'checkboxfield',
				    id: 'remember-me-field',
				    name: 'remember',
				    fieldLabel: Locale.APP_LOGINFORM_REMEMBER,
				    listeners: {
				        scope: this
				    }
				}],
				buttons: [{
                    id: 'login-button',
					text: Locale.APP_BUTTON_LOGIN,
					handler: this.onLoginClick,
					scope: this
				}]
			}]
		});

		this.callParent();
		this.on('show', this.onShowDialog, this);
		this.addEvents('login');
	},

	onUserName: function (sender, event, eOpts) {
	    if (event.getKey() === event.ENTER) {
	        Ext.getCmp('login-pass-field').focus();
	    }
	},

	onSpecialKey: function (sender, event, eOpts) {

	    if (event.getKey() === event.ENTER) {
	        var btn = Ext.getCmp('login-button');
	        btn.focus();
	        this.onLoginClick(btn);
	    }
	},

	checkValid: function() {
		// TODO: Should check form if it is valid
		return true;
	},

	onLoginClick: function(sender) {

		if(this.checkValid()) {

			this.fireEvent('login', this, this.getValues());
		}
	},

	onShowDialog: function(sender) {

		sender.down('form').getForm().reset();
	},

	getValues: function() {

		return this.down('form').getValues();
	},

	mask: function () {
	    if (!this.masked) {
	        this.masked = true;
	        this.down('form').mask();
	    }
	},

	unmask: function () {
	    if (this.masked) {
	        this.masked = false;
	        this.down('form').unmask();
	    }
	    
	}
});