/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.EntityItem
* Элемент сущности
*/
Ext.define("App.logic.EntityItem", {

	requires: [
		'App.logic.mixins.TablepartsDenormalizer',
		'App.components.mixins.RegistryInfo',
		'App.logic.mixins.ItemDenormalizer',
		'App.components.mixins.Operations',
		'App.logic.factory.Buttons',
		'App.logic.factory.Fields',
		'App.logic.mixins.Events',
		'Ext.window.MessageBox',
		'Ext.util.Observable',
		'Ext.form.Panel',
		'Ext.form.field.Text'
	],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {

		/**
		* Реализация работы документа с операциями
		*/
		operations: 'App.components.mixins.Operations',
		registryInfo: 'App.components.mixins.RegistryInfo',
		events: 'App.logic.mixins.Events',
		observable: 'Ext.util.Observable',
		tablepartsDenormalizer: 'App.logic.mixins.TablepartsDenormalizer',
		itemDenormalizer: 'App.logic.mixins.ItemDenormalizer'
	},

	/**
	* Дефолтные значения,которые будут передаваться при создании
	* новых элементов справочника
	* @type {Object}
	*/
	defaults: null,

	/**
	* Указывает, должна ли форма сразу быть показана клиенту
	* @cfg {Boolean} [autoShow=true]
	*/
	autoShow: true,

	/**
	* Метод возвращающий набор кнопок для формы
	* @method
	*/
	getActions: function () {

		var result = this.mixins.registryInfo.getActions.apply(this);

		result.push(this.Buttons.refresh({
			handler: this.refreshElement,
			scope: this,
			disabled: true
		}));
		
		if (this.entity.identitytype == App.enums.EntityType.Reference) {
			result.push(this.Buttons.getUserActivityReport());
		}

		if (!Ext.isEmpty(this.model.printForms)) {
			result.push(this.Buttons.printForms(this.model.printForms));
		}
		
		result.push(this.Buttons.save({
			handler: this.saveElement,
			scope: this
		}));

		result.push(this.Buttons.saveandclose({
			handler: this.saveAndCloseElement,
			scope: this
		}));

		this.fieldFactory.setMenuHandler(function (item, field_name) {
			var field, entity_id, temp;

			field = this.getField(field_name);
			if (field) {
				field.setValue(item.origin.id);

				this.saveElement();
			}
		}, this);

		var status = this.fieldFactory.getStatusBtn();
		if (status !== null) {
			result.push(status);
		}

		var operations = this.mixins.operations.getActions.apply(this);
		return result.concat(operations);
	},

	formValue: function () {
		var result = {};

		this.form.getFields().each(function (field) {
			if (Ext.isFunction(field.get)) {
				Ext.apply(result, field.get());
			}
		}, this);

		return Object.keys(result).length !== 0 ? result : false;
	},

	getParentEntityId: function () {

		var owner = this.getParent(), entity = owner.entity;
		if (Ext.isDefined(entity) && Ext.isDefined(entity.id)) {
			return entity.id;
		} else {
			return null;
		}
	},

	/**
	* Метод должен проверить валидна ли форма и выдать сообщение,
	* если данные не введены корректно
	*/
	isValid: function () {

		var valid = this.form.isValid();
		if (!valid) {
			Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MESSAGE_FORM_INVALID);
		}
		return valid;
	},

	/**
	* Сохранение элемента с последующим закрытием формы
	*/
	saveAndCloseElement: function () {
		if (!this.isValid()) {
			return;
		}

		var result = this.formValue();
		this.shouldBeClosed = true;

		if (result !== false) {
			// Если есть что сохранять - сохраняем
			this.send(result);
		} else {
			this.close();
		}
	},

	/**
	* Сохранение элемента
	* @method
	* @return false - если нечего сохранять и метод отправки данных на сервер не был вызван.
	*/
	saveElement: function () {
		if (!this.isValid()) {
			return;
		}

		this.shouldBeClosed = false;

		var result = this.formValue();
		if (result !== false) {
			// Если есть что сохранять - сохраняем
			this.send(result);
		}
		return result;
	},

	send: function (result) {
		var entity_id = null, tp, ei;

		// Получаем вышестоящую ТЧ
		if (!Ext.isDefined(this.selection_form)) {
			tp = this.getParent();
			if (tp && tp.getParent) {
				// Берем родителя вышестоящей ТЧ (форма)
				ei = tp.getParent();
				if (ei) {
					entity_id = ei.entity.id;
				}
			}
		}

		CommunicationDataService.saveElement(this.docid, this.entity.id, result, entity_id, this.onSavedElement, this);
	},

	/**
	* Метод вызывает серверный метод, чтобы получить значения по умолчанию для сущности
	* отображаемой в списке
	*/
	getDefaultValues: function () {
		var me = this;
		
		var clientDefaultValues = Ext.apply({}, this.defaults || {});
		
		//Переводим значения из gridsValues -- в набор { 'имя поля' : значение }
		//todo: вынести в обший метод
		if (this.gridsValues)
			this.gridsValues.keys.forEach(function (key) {
				me.model.result.forEach(function (modelField) {
					if (modelField.identitylink == key) 
						clientDefaultValues[ modelField.name.toLowerCase() ] = me.gridsValues.get(key).id;
				});
			});

		DataService.getDefaults(this.entity.id, clientDefaultValues, function (result) {
			if (result && Ext.isDefined(result.defaults)) {
				this.defaults = Ext.apply(this.defaults || {}, result.defaults);
			}
			if (!Ext.isEmpty(this.defaults)) {
				this.setData({ result: [this.defaults] }, true);
			}
			this.fireEvent('dataget', this.getForm(), this.defaults || {});

			// Установка значений в соответствии с вышестоящим гридом
			if (!Ext.isEmpty(this.gridsValues)) {
				this.form.getFields().each(function (field) {
					if (field.initialModel && this.gridsValues.indexOfKey(field.initialModel.identitylink) >= 0) {
						var value = this.gridsValues.get(field.initialModel.identitylink);
						if (Ext.getClassName(field) === 'App.components.fields.Hidden') {
							field.setValue(value.id);
						}
						if (Ext.getClassName(field) === 'App.components.fields.Link') {
							field.setValue(value.id, value.caption);
						}
					}
				}, this);
			}

			this.unmaskForm();
		}, this);
	},

	/**
	* Обновление формы, запрос данных с сервера
	* @method
	*/
	refreshElement: function () {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('EntityItem::refreshElement');
		}
		// </debug>

		this.maskForm(Locale.APP_COMMON_LOADING);
		if (this.docid !== null) {
			if (this.entity.identitytype == 4 /*ТЧ*/) {
				var ownerEntityId, ownerItemId;
				var owner = this.getParent().getParent();
				ownerEntityId = owner.entity.id;
				ownerItemId = owner.getField('id').getValue();
				DataService.getTpItem(this.entity.id, this.docid, ownerEntityId, ownerItemId, this.onLoad, this);
			} else {
				DataService.getItem(this.entity.id, this.docid, this.onLoad, this);
			}
		} else {
			this.getDefaultValues();
		}
	},

	setData: function (data, defaults) {
		if (Ext.isEmpty(data.result)) {
			// <debug>
			if (Ext.isDefined(Ext.global.console)) {
				Ext.global.console.log('Пришел пустой ответ от сервера!');
			}
			// </debug>
			return;
		}

		this.form.getFields().each(function (field) {
			if (Ext.isFunction(field.set)) {
				field.set(data, defaults);
			}
		}, this);
		this.mixins.operations.update.apply(this, [data]);
		this.mixins.registryInfo.update.apply(this, [data]);
	},

	getField: function (name) {

		var result = null, fields = this.form.getFields();

		if (fields) {
			fields.each(function (field) {
				if (field.name.toLowerCase() === name.toLowerCase()) {
					result = field;
				}
			}, this);
		}

		return result;
	},

	blockFields: function (exceptions) {

		this.setDisabled(true, exceptions);
	},

	/**
	 * is formShouldBeDisabled ?
	 */
	checkRefStatus: function () {

		var formShouldBeDisabled = !this.isNew
			&& this.fieldFactory.statusField
			&& this.getField(this.fieldFactory.statusField).getValue() != App.enums.RefStatus.New;

		return formShouldBeDisabled;
	},

	/**
	* Устанавливает доступность формы
	*/
	setDisabled: function (disableForm, exceptions) {

		this.setDisabledFields(this.form.getFields(), disableForm, exceptions);
	},

	/**
	* @param {Ext.util.MixedCollection} fields Поля, для которых следует установить доступность
	* @param {Bool} disable true - для блокировки полей, false - для разблокировки
	* @param {Array} exceptions Если поле находится среди исключений, то оно не может быть заблокировано. 
	* Но при операции разблокирования, оно будет разблокировано.
	*/
	setDisabledFields: function (fields, disable, exceptions) {
		var temp = exceptions || [];
		fields.each(function (field) {
			if (field.initialModel) {
				var dis = disable && !Ext.Array.contains(temp, field.name); // учитываем исключения
				dis = dis || field.initialModel.isReadOnly(); // учитываем первоначальную настройку редактируемости поля

				if (Ext.isFunction(field.setReadOnly))
					field.setReadOnly(dis);
				else
					field.setDisabled(dis);
			}
		}, this);
	},


	getFieldsConfig: function(model, form_items) {

		return Ext.create('App.logic.factory.Fields', {
			form_id: this.id,
			docid: this.docid,
			model: model,
			formCfg: form_items,
			entity: this
		});
	},

	getConfig: function () {

		this.fieldFactory = this.getFieldsConfig(this.model, this.formCfg.formItems);
		
		this.panel = Ext.create('Ext.form.Panel', {
			defaultType: 'textfield',
			autoScroll: true,
			bodyPadding: '5 5 0',
			defaults: {
				anchor: '100%'
			},
			fieldDefaults: {
				labelWidth: 120
			},
			items: this.fieldFactory.get(),
			buttons: this.getActions()
		});
		this.mixins.itemDenormalizer.addFieldsInDenormalizedForm.apply(this);
		this.panel.on('afterrender', this.onAfterRender, this);
		this.form = this.panel.getForm();
		this.title = this.entity.caption;
	},

	/**
	* Обработчик событий, вызываемый после сохранения элемента справочника
	* @private
	*/
	onSavedElement: function (result, response) {
		if (response.type !== 'rpc') {
			// <debug>
			if (Ext.isDefined(Ext.global.console)) {
				Ext.global.console.log('Произошла ошибка при сохранении элемента');
			}
			// </debug>
			this.fieldFactory.resetStatus();
			return;
		}

		App.EntitiesMgr.registerUpdate(this.entity.id, this.docid);

		this.docid = this.docid || result;

		this.fireEvent('aftersave', this);
		if (this.isNew) {
			var oldWindowKey = this._windowKey;
			this._windowKey = null;
			this.fireEvent('windowkeychanged', this, { oldvalue: oldWindowKey, value: this.getWindowKey() });
		}

		if (this.shouldBeClosed) {
			this.close();
		} else {
			this.refreshElement();
		}
	},

	/**
	* Обработчик событий, вызываемый после загрузки элемента справочника
	* @private
	*/
	onLoad: function (result, response) {

		if (response.type !== 'rpc') {
			Ext.Error.raise('Exception occured while getting element data!');
		}
		this.elementData = result;
		this.setData(result);
		this.isNew = false;

		this.Buttons.refresh().enable();

		this.updateButtonsState();
		
		if (result.readOnly)
			this.setDisabled(true);
		else if (this.entity.identitytype == App.enums.EntityType.Reference)
			this.setDisabled(this.checkRefStatus());
		else {
			var tableFields = this.form.getFields().filterBy(function (field) {
				return field.isTableField && field.isTableField();
			}, this);

			// т.к. пришли данные => делаем доступными табличные поля.
			// если поле нельзя делать доступным, то CommonField.canEnable
			this.setDisabledFields(tableFields, false);
		}

		this.refreshGrids();

		this.fireEvent('dataget', this.getForm(), this.elementData.result[0]);
		this.fireEvent('itemloaded', this);
	},

	refreshGrids: function () {
		// TODO Это здесь не должно быть ни в коем случае
		if (!this.isDenormalizerFinished || !this.elementData)
			return;
		
		// Блокируем зависимые ТЧ. Они будут разблокированы при выделении строки родительской ТЧ.
		this.form.getFields().each(function (field) {
			if (field instanceof App.components.fields.TableField && field.isDependsOnTablefield()) {
				field.disable();
			}
		}, this);

		// Запускаем обновление всех независимых табличных полей.
		var independentGrids = this.form.getFields().filterBy(function (field) {
			return Ext.isDefined(field.initialModel) && field.isTableField() && !field.isDependsOnTablefield();
		}, this);
		this.pendingIndependentGridsCnt = independentGrids.getCount();
		independentGrids.each(function (field) {
			field.grid.getStore().on('load', function () {
				this.pendingIndependentGridsCnt--;
				if (this.pendingIndependentGridsCnt == 0) {
					// => последний независимый грид, завершивший загрузку данный
					this.independentGridsLoaded = true;
					// <debug>
					if (Ext.isDefined(Ext.global.console)) {
						Ext.global.console.log('fireEvent independentgridsloaded');
					}
					// </debug>
					this.fireEvent('independentgridsloaded', this);
				}
			}, this);
			field.refresh();
		}, this);

		this.unmaskForm();
	},

	/** 
	 * Обновляет состояние кнопок панели инструментов формы элемента.
	 * Устанавливается видимость и доступность кнопок.
	 */
	updateButtonsState: function () {
		var printForms = this.Buttons.printForms();
		Ext.isDefined(printForms) && printForms.setVisible(!this.isNew);
	},

	/**
	* Обработчик событий, вызываемый после отрисовки формы
	* @private
	*/
	onAfterRender: function () {

		if (!this.preventLoad) {

			this.preventLoad = false;
			this.refreshElement();
		}
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Initializing UserEvents ...');
		}
		// </debug>
		this.mixins.events.constructor.call(this, {
			form_id: this.id,
			entity_name: this.entity.name
		});
		this.checkParentGrid();
		this.fireEvent('afterrender', this.getForm());
	},

	/**
	* Обработчик событий, вызываемый после того, как сервер прислал модель справочника
	* @private
	*/
	onModel: function (sender, model) {

		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Got into callback with model');

			if (Ext.isEmpty(model)) {
				Ext.Error.raise('Model should be defined!!!');
			}
		}
		// </debug>

		this.model = model;
		this.fireEvent('model', this);
		App.FormMgr.getForm(model.entityId, 1, this.onForm, this);
	},

	/**
	* 
	* @private
	*/
	onForm: function (sender, formCfg) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Got into callback with form');
		}
		// </debug>
		this.formCfg = formCfg;
		if (this.autoShow === true) {
			this.show();
		}
	},

	getForm: function () {
		return this.panel;
	},

	// Что это?!
	_windowKey: null,

	getWindowKey: function () {
		if (this._windowKey)
			return this._windowKey;
		if (this.docid)
			return this._windowKey = '' + this.model.entityId + '_' + this.docid;
		else {
			return this._windowKey = App.common.Guid.NewGuid();
		}
	},

	/**
	* @method
	* Метод регистрирует окно в глобальном менеджере окон
	*/
	show: function () {
		// <debug>
		if (Ext.isEmpty(this.model)) {
			Ext.Error.raise('Model should be defined!!!');
		}
		// </debug>

		this.getConfig();

		this.updateButtonsState();

		// Блокируем табличные поля в форме создания нового элемента
		if (this.isNew) {
			var tableFields = this.form.getFields().filterBy(function(field) {
				return field.isTableField && field.isTableField();
			}, this);
			
			this.setDisabledFields(tableFields, true);
		}

		this.fireEvent('beforeshow', this);
		App.WindowMgr.add(this, { iconCls: 'icon_element', width: 300 }, true);
	},

	close: function () {

		App.WindowMgr.remove(this);
	},


	/**
	* Возвращает ссылку на родительский компонент грида
	* @return {Ext.Base} Компонент, которы открыл данный грид
	*/
	getParent: function () {

		return Ext.getCmp(this.parent_id);
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		config.id = config.id || Ext.id();
		Ext.apply(this, config);
		this.isNew = this.docid === null;
		this.independentGridsLoaded = false;

		Ext.ComponentManager.register(this);

		// <debug>
		if (Ext.isDefined(config)) {
			if (Ext.isDefined(Ext.global.console)) {
				if (config.docid === null) {
					Ext.global.console.log(
						Ext.String.format('Creating element of entity {0}. Form id = {1}', this.entity.name, this.id)
					);
				} else {
					Ext.global.console.log(
						Ext.String.format('Opening element : {0} of entity {1}. Form id = {2}', config.docid, this.entity.name, this.id)
					);
				}
			}
		} else {
			Ext.Error.raise('Configuration is not defined in EntityItem creation!');
		}
		// </debug>

		this.mixins.observable.constructor.call(this, config);
		this.addEvents(
			/**
			* @event itemloaded
			* Событие возникает после загрузки элемента в форму сразу после начала (но еще до завершения) загрузки занных в табличные поля.
			* Событие не возникает при открытии нового элемента, т.е. в этом случае запроса за данными не происходит.
			* @param {Ext.Component} this
			*/
			'itemloaded',
			/**
			 * @event defaultsloaded
			 * Событие возникает после получения значений системных измерений с сервера.
			 * @param {Ext.Component} this
			 */
			'defaultsloaded',
			/**
			 * @event beforeshow
			 * Событие возникает непосредственно перед открытием формы элемента
			 * @param {Ext.Component} this
			 */
			'beforeshow',
			/**
			 * @event aftersave
			 * Событие возникает после сохранения элемента. Когда пришел ответ от сервера с новым id.
			 * @param {Ext.Component} this
			 */
			'aftersave',
			'windowkeychanged',
			'afterrender',
			/**
			* @event dataget
			* Событие возникает после загрузки элемента в форму сразу после начала (но еще до завершения) загрузки данных в табличные поля.
			* Событие возникает при открытии нового элемента, в этом случае передается значения по умолчанию
			* @param {Ext.Component} this
			*/
			'dataget',
			
			/**
			* @event independentgridsloaded
			* Событие возникает один раз при загрузке элемента - после загрузки данных во все независимые гриды.
			* @param {Ext.Component} this
			*/
			'independentgridsloaded',
			
			/**
			* @event model
			* Событие возникает после получения модели.
			* @param {Ext.Component} this
			*/
			'model'
		);

		this.mixins.operations.constructor.call(this);
		this.mixins.tablepartsDenormalizer.constructor.call(this);
		this.mixins.itemDenormalizer.constructor.call(this);

		// После загрузки данных в независимые гриды попытаемся сделать доступными все зависимые гриды.
		// При попытке проверяется idRefStatus, editableFields и наличие выделенной строки в родительской ТЧ.
		// Данная мера необходима, т.к. TableField.onParentSelectionChanged не сработает в случае 
		// если при обновлении табличных полей выделенный элемент останется прежним.
		this.on('independentgridsloaded', function () {
			this.form.getFields().each(function (field) {
				if (field instanceof App.components.fields.TableField &&
					field.isDependsOnTablefield()) {
					field.enable();
				}
			}, this);
		}, this);

		this.Buttons = Ext.create('App.logic.factory.Buttons', this.id);
		App.ModelMgr.getModel(config.entity.id, null, this.onModel, this);
	},

	checkParentGrid: function() {

		var tmp;
		tmp = this.getParent();
		if (!tmp) return;
		tmp = Ext.getCmp(tmp.field_id);
		if (!tmp) return;
		
		if (tmp.disabled) {
			this.setDisabled(true);
			// ToDo{SBORIII-690}: Это надо перенести в setDisabled
			this.panel.getDockedItems('toolbar')[0].items.each(function (item) {
				item.setDisabled(true);
			});
		} 
	},

	isXType: function (xtype) {

		return false;
	},

	unmaskForm: function () {

		this.panel.getEl().unmask();
	},

	maskForm: function (text) {

		this.panel.getEl().mask(text);
	}
})