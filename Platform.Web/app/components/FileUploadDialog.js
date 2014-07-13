/**
*
* Диалог загрузки файла
*
* @class App.components.FileUploadDialog
* @extends Ext.form.Panel
*/

Ext.define('App.components.FileUploadDialog', {
	/**
	* @cfg {String} url
	* url веб-сервис
	*/
	url: 'Services/UploadFile.aspx',

	

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
			params: {},
			waitMsg: 'Загрузка файла...',
			success: function (form, action) {
				Ext.Msg.show({
					title: 'Загрузка завершена',
					msg: action.result.msg,
					width: 300,
					buttons: Ext.Msg.OK,
					icon: Ext.window.MessageBox.INFO
				});

				window.close();

				if (Ext.isFunction(me.successCallback))
				    me.successCallback.call(me.sender, action.result.FileLinkId, action.result.FileCaption, action.result.FileDescription);
			},
			failure: function (form, action) {
				var errTitle = 'Ошибка загрузки';
				switch (action.failureType) {
					case Ext.form.action.Action.CLIENT_INVALID:
						Ext.Msg.show({
							title: errTitle,
							msg: 'Form fields may not be submitted with invalid values',
							width: 300,
							buttons: Ext.Msg.OK,
							icon: Ext.window.MessageBox.ERROR
						});
						break;
					case Ext.form.action.Action.CONNECT_FAILURE:

						Ext.Msg.show({
							title: errTitle,
							msg: 'Ajax communication failed',
							width: 300,
							buttons: Ext.Msg.OK,
							icon: Ext.window.MessageBox.ERROR
						}); 
						break;
					case Ext.form.action.Action.SERVER_INVALID:
						var errMessage = action.result.msg || action.response.responseXML.title;

						Ext.Msg.show({
							title: errTitle,
							msg: errMessage,
							width: 300,
							buttons: Ext.Msg.OK,
							icon: Ext.window.MessageBox.ERROR
						}); 
				}
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
				fieldLabel: 'Файл',
				name: 'fileName',
				buttonText: 'Выбрать ...',
				allowBlank: false,
				onChange: function () {
					var fileName = this.value.substring(this.value.lastIndexOf('\\') + 1, this.value.lastIndexOf('.'));
					this.ownerCt.getForm().findField("caption").setValue(fileName);
					//alert(this.value);
				}
			}, {
				fieldLabel: 'Имя файла',
				name: 'caption',
				id: me.id + '-field-caption'
			}, {
				fieldLabel: 'Описание файла',
				name: 'description',
				xtype: 'textareafield',
				height: 60
			}],

			// Панель инструментов
			dockedItems: [{
				xtype: 'toolbar',
				dock: 'bottom',
				items: [
					{ xtype: 'tbfill' },
					{
						xtype: 'button',
						text: 'Загрузить',
						handler: function () {
							var form = this.up('form').getForm();
							if (form.isValid()) {
								form.submit(me.getSubmitConfig(this.up('window')));
							}
						}
					},
					{
						xtype: 'button',
						text: 'Отмена',
						handler: function () {
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
			height: 200,
			width: 400,
			layout: 'fit',
			modal: true,
			title: 'Загрузка файла',
			items: me.getFormConfig()
		};
	},

	show: function () {
		if (!this.dialog)
			this.dialog = Ext.create('Ext.window.Window', this.getConfig());
		this.dialog.show();
	}
})
