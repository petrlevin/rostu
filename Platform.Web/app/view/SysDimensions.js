/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.view.SysDimensions
* Description
*/
Ext.define('App.view.SysDimensions', {
	extend: 'Ext.Window',
	alias: 'widget.sysdimensions',

	requires: [
		'Ext.util.Cookies',
		'Ext.form.Panel',
		'Ext.form.field.ComboBox',
		'Ext.layout.container.Anchor'
	],

	title: Locale.APP_SYSDIMENSIONSFORM_TITLE,
	width: 400,
	closable: false,
	modal: true,

	constructor: function(config){

		Ext.apply(this, config);
		this.callParent([ config ]);
	},
	
	initComponent: function() {

		Ext.apply(this, {
			items: [{
			    xtype: 'form',
			    bodyStyle: 'padding: 10px;',
			    layout: 'anchor',
			    defaults: {
			        anchor: '100%'
			    },
			    items: [{
			        id: this.id + '-ppo',
			        xtype: 'combo',
			        fieldLabel: 'ППО',
			        name: 'PublicLegalFormation',
			        autoSelect: true,
			        allowBlank: false,
			        valueField: 'id',
			        displayField: 'caption',
			        store: this.ppo,
			        anchor: '100%',
			        queryMode: 'local',
			        typeAhead: true,
			        listeners: {
			            scope: this,
			            'change': this.onPpoSelect
			        }
			    }, {
			        id: this.id + '-budget',
			        xtype: 'combo',
			        fieldLabel: 'Бюджет',
			        name: 'Budget',
			        autoSelect: true,
			        allowBlank: false,
			        valueField: 'id',
			        displayField: 'caption',
			        store: this.budget,
			        queryMode: 'local',
			        typeAhead: true
			    }, {
			        id: this.id + '-version',
			        xtype: 'combo',
			        fieldLabel: 'Версия',
			        name: 'Version',
			        autoSelect: true,
			        allowBlank: false,
			        valueField: 'id',
			        displayField: 'caption',
			        store: this.version,
			        queryMode: 'local',
			        typeAhead: true
			    }],
				buttons: [{
				    text: Locale.APP_BUTTON_SELECT,
					handler: this.onClick,
					scope: this
				},
			        this.canCanceled ? {
			            text: Locale.APP_BUTTON_CANCEL,
			            handler: this.onCancel,
			            scope: this
			        } : null]
			}]
		});

		this.callParent();
		this.on('show', this.onShowDialog, this);
		// this.addEvents('enter');
	},

	checkValid: function () {

		// TODO: Should check form if it is valid
	    return Ext.getCmp(this.id + '-ppo').isValid() &&
	        Ext.getCmp(this.id + '-budget').isValid() &&
	        Ext.getCmp(this.id + '-version').isValid();
	},

	onPpoSelect: function (sender, records, eOpts) {
	    var temp = null;

	    // <debug>
	    if (Ext.isDefined(Ext.global.console)) {
	        Ext.global.console.log('PPO Select :'+ sender.getValue());
	    }
	    // </debug>
	    temp = [];
	    this.parent.budget.each(function (record) {
	        if (record.get('idpubliclegalformation') === sender.getValue()) {
	            temp.push({
	                'id': record.get('id'),
	                'caption': record.get('caption')
	            })
	            // <debug>
	        } else {
	            if (Ext.isDefined(Ext.global.console)) {
	                Ext.global.console.log('row :' + record.get('idpubliclegalformation'));
	            }
	            // </debug>
	        }
	    }, this);
	    this.budget.loadData(temp);

	    this.setDefaultValue('budget','Budget');
	    
	    temp = [];
	    this.parent.version.each(function (record) {
	        if (record.get('idpubliclegalformation') === sender.getValue()) {
	            temp.push({
	                'id': record.get('id'),
	                'caption': record.get('caption')
	            })
	            // <debug>
	        } else {
	            if (Ext.isDefined(Ext.global.console)) {
	                Ext.global.console.log('row :' + record.get('idpubliclegalformation'));
	            }
	            // </debug>
	        }
	    }, this);
	    this.version.loadData(temp);
	    this.setDefaultValue('version','Version');
	},

	onClick: function(sender) {
	    if(this.checkValid()) {
		    Ext.callback(this.handler, this.scope, [this, this.getValues()]);
		    this.hide();
		}
	},
	
	onCancel: function () {
	    this.hide();
	},

    onShowDialog: function(sender) {
	    sender.down('form').getForm().reset();

	    this.setDefaultValue('ppo','PublicLegalFormation');
	},

    setDefaultValue: function (storeKey, cookieKey) {
	    var defaultValue;
	    var cookieIdValue = Ext.util.Cookies.get(cookieKey);

	    var store = this[storeKey];

	    store.each(function () {
	        if (this.internalId == cookieIdValue)
	            defaultValue = this;
	    });

	    if (!defaultValue)
	        defaultValue = store.first();

	    this.down('form').getForm().getFields().get(this.id + '-' + storeKey).setValue(defaultValue);
    },

    getValues: function() {

		return this.down('form').getValues();
	},

	getValue: function(key) {
	    return this.down('form').getForm().getFields().get(this.id + '-' + key).getValue();
    },

    mask: function () {
	    if (!this.masked) {
	        this.masked = true;
	        this.down('form').mask();
	    }
	},

	unmask: function () {
	    if (this.masked) {
	        this.down('form').unmask();
	    }
	}
});
