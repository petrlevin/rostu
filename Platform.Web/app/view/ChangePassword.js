/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.view.ChangePassword
* @extends Ext.Window
* Change password form for application
*/
Ext.define('App.view.ChangePassword', {
	extend: 'Ext.Window',
	alias: 'widget.changeview',

	requires: [
		'Ext.form.field.Text',
		'Ext.button.Button'
	],

	title: Locale.APP_CHANGEFORM_TITLE,
	width: 300,
	closable: false,
    resizable: false,
	modal: true,

	initComponent: function(config) {

		Ext.apply(this, {
			items: [{
				xtype: 'form',
				bodyStyle: 'padding: 10px;',
				items: [{
                    id: 'old-password',
					xtype: 'textfield',
					name: 'oldpass',
					inputType: 'password',
					fieldLabel: Locale.APP_CHANGEFORM_OLDPASSWORD
				}, {
                    id: 'new-password',
					xtype: 'textfield',
					name: 'newpass',
					inputType: 'password',
					fieldLabel: Locale.APP_CHANGEFORM_NEWPASSWORD
				}, {
					xtype: 'textfield',
					name: 'newpassrep',
					initialField: 'new-password',
                    vtype: 'confirm',
					inputType: 'password',
					fieldLabel: Locale.APP_CHANGEFORM_NEWPASSWORD_REPEAT
				}],
				buttons: [{
				    text: Locale.APP_CHANGEFORM_BTN_CHANGE,
					handler: this.onChangeClick,
					scope: this
				}, {
				    text: Locale.APP_CHANGEFORM_BTN_CANCEL,
				    handler: this.onCancelClick,
				    scope: this
				}]

			}]
		});

		this.callParent();
		this.on('show', this.onShowDialog, this);
		this.addEvents('login');
		this.addEvents('passwordchanged');
	},

	onChangeClick: function(sender) {

	    var form = this.down('form');
	    if (form && form.isValid() === true) {

	        ProfileService.changePassword(
                Ext.getCmp('old-password').getValue(),
                Ext.getCmp('new-password').getValue(), this.onPasswordChange, this);
		}
	},

	onPasswordChange: function (result, response) {
	    if (response.type !== 'rpc') {
	        return;
	    }
	    this.fireEvent('passwordchanged', this, this);
	    this.close();
	    
	},

	onCancelClick: function (sender) {

	    this.close();
	},

	onShowDialog: function(sender) {

		sender.down('form').getForm().reset();
	},

	getValues: function() {

		return this.down('form').getValues();
	}
});