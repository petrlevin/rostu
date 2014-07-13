Ext.define('App.components.ImportDialog', {

    /**
    * @cfg {Integer} idTemplate
    */

    /**
    * @cfg {Integer} idEntity
    */

    /**
    * @cfg {String} url
    * url веб-сервиса, например: Services/Import.aspx
    */
    url: 'Services/Import.aspx',
    
    /**
    * @cfg {String} format
    * Формат импортируемого файла: Указывается в заголовке окна.
    */
    format: 'Excel',

	requires: [
		'Ext.toolbar.Toolbar',
		'Ext.window.MessageBox',
		'Ext.form.field.Text',
		'Ext.form.field.File',
		'Ext.window.Window',
		'Ext.Button'
	],

	dialog: null,

	constructor: function (config) {
		Ext.apply(this, config);
	},

	getMessageFormConfig: function (title, html) {
		return {
            title: title,
		    bodyPadding: 5,
			html: html
		};
	},

	getSubmitConfig: function (window) {
		var me = this;
		return {
			url: me.url,
			params: this.params,
			waitMsg: 'Загрузка файла...',
			success: function (form, action) {
			    App.WindowMgr.add(me.getMessageFormConfig('Импорт завершен', action.result.msg));
				window.close();
			},
			failure: function(form, action) {
				switch (action.failureType) {
				case Ext.form.action.Action.CLIENT_INVALID:
					Ext.Msg.alert('Failure', 'Form fields may not be submitted with invalid values');
					break;
				case Ext.form.action.Action.CONNECT_FAILURE:
					Ext.Msg.alert('Failure', 'Ajax communication failed');
					break;
				case Ext.form.action.Action.SERVER_INVALID:
				    App.WindowMgr.add(me.getMessageFormConfig('Ошибка импорта', action.result.msg));
				}
				window.close();
			}
		};
	},
	
	getFormConfig: function () {
		var me = this;
		return {
			xtype: 'form',
			layout: 'anchor',
			bodyPadding: 5,
			defaults: {
				anchor: '100%'
			},
			// Поля
			defaultType: 'textfield',
			items: [{
					xtype: 'filefield',
					fieldLabel: 'Имя файла',
					name: 'fileName',
					buttonText: 'Выбрать ...',
					allowBlank: false
				}, {
					fieldLabel: 'Игнорировать',
					name: 'ignoreRows'
				}],

			// Панель инструментов
			dockedItems: [{
				xtype: 'toolbar',
				dock: 'bottom',
				items: [
					{ xtype: 'tbfill' },
					{
						xtype: 'button',
						text: 'Импортировать',
						handler: function() {
							var form = this.up('form').getForm();
							if (form.isValid()) {
								form.submit(me.getSubmitConfig(this.up('window')));
							}
						}
					},
					{
						xtype: 'button',
						text: 'Отмена',
						handler: function() {
							this.up('window').close();
						}
					}
				]
			}]
		};
	},

	getConfig: function () {
		var me = this;
		return {
			height: 130,
			width: 400,
			layout: 'fit',
			modal: true,
			title: Ext.String.format('Импорт из {0}: Выберите файл для импорта', this.format),
			items: me.getFormConfig()
		};
	},

	show: function () {
		if (!this.dialog)
			this.dialog = Ext.create('Ext.window.Window', this.getConfig());
		this.dialog.show();
	}
});