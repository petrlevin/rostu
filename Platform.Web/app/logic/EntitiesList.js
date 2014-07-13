/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.EntitiesList
* Список элементов справочника
*/
Ext.define("App.logic.EntitiesList", {

	requires: [
		'App.components.mixins.ListExportImport',
		'App.components.mixins.ListStatusedReference',
		'App.components.mixins.ListDocument',
		'App.components.mixins.ListMultiEdit',
		'App.components.mixins.Versioning',
		'App.logic.factory.Buttons',
		'App.logic.mixins.Columns',
		'App.logic.mixins.Events',
		'App.components.EntityTreeGrid',
		'App.components.EntityGrid',
		'App.components.Search',
		'App.logic.EntityItem',
		'Ext.util.Observable'
	],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
		listexportimport: 'App.components.mixins.ListExportImport',
		listmultiedit: 'App.components.mixins.ListMultiEdit',
		liststatusedreference: 'App.components.mixins.ListStatusedReference',
		listdocument: 'App.components.mixins.ListDocument',
		versioning: 'App.components.mixins.Versioning',
		events: 'App.logic.mixins.Events',

		/**
		* Реализация событийной модели
		*/
		observable: 'Ext.util.Observable',
		colums:'App.logic.mixins.Columns'
	},

	/**
	* Автоматически показывать список в окне, как только он будет создан
	* @cfg {Boolean} [autoShow=true]
	*/
	autoShow: true,

	/**
	* Конструктор, который создает новый объект данного класса
	* 
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {

		config.id = config.id || Ext.id();
		Ext.apply(this, config);

		Ext.ComponentManager.register(this);

		// Инициализируем событийную модель объекта
		this.mixins.observable.constructor.call(this, config);
		this.addEvents(
			'beforerequest',
			'storeload'
		);

		App.ModelMgr.getModel(this.model_id || this.entity_id, config.idOwner, this.onModel, this);
	},

	onBeforeDestroy: function () {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('EntitiesList::beforeDestroy');
		}
		// </debug>
		App.EntitiesMgr.un('updates', this.onEntityUpdate, this);
	},

	/**
	* Метод, который конфигурирует кнопки, которые будут показаны на гриде
	* @return {Array} Массив кнопок которые должны быть созданы на гриде
	*/
	getActions: function () {
		var result = [];

		if (Ext.isDefined(this.customGrid) &&
			Ext.isDefined(this.customGrid.extendedActions)) {

			result = result.concat(this.customGrid.extendedActions);

			Ext.apply(this, this.customGrid.flags);
		}

		if (!Ext.isDefined(this.preventCreateBtn)) {
			/*if (this.entity.identitytype == App.enums.EntityType.Reference) {
				result = result.concat([this.Buttons.createsplit({
					handler: this.createItem,
					scope: this
				}, {
					handler: this.createItemAsCopy,
					scope: this
				})]);
			} else*/
			{
				result = result.concat([
				this.Buttons.create({
					handler: this.createItem,
					scope: this
				})
				]);
			}
		}
		if (!Ext.isDefined(this.preventOpenBtn)) {
			result = result.concat([
				this.Buttons.open({
					handler: this.openItem,
					scope: this
				})
			]);
		}
		if (!Ext.isDefined(this.preventDeleteBtn)) {
			result = result.concat([
				this.Buttons.del({
					handler: this.deleteItem,
					scope: this
				})
			]);
		}

		result.push(this.Buttons.actions()); 

		// Проход всех примесей, чтобы получить меню в соответствии с функциями
		Ext.iterate(this.mixins, function (name, mixin) {
			if (Ext.isFunction(mixin.getActions)) {
				var mixinResult = mixin.getActions.call(this, result);
				// если mixinResult неопределен, то считаем, что getActions вставил нужные пункты в существующее меню
				if (Ext.isDefined(mixinResult))
					result = result.concat(mixinResult);
			}
		}, this);

		result = result.concat([
			this.Buttons.dependencies({
				handler: this.viewDependencies,
				scope: this
			})
		]);

		result.push(this.Buttons.refreshgrid({
			handler: this.refresh,
			scope: this
		}));
		result = result.concat(['->',
			this.searchField = Ext.create('App.components.Search', {
				width: 200
			})
		]);
		this.searchField.on('search', this.onSearch, this);

		// Добавление "утилитных" кнопок
		if(Ext.isFunction(this.getTools)) {
			result = result.concat(this.getTools());
		}
		return result;
	},

	/**
	* Метод возвращает выбранное в гриде значение
	* @return {String} Значение идентификатора выбранной строки
	*/
	getValue: function () {
		var fieldValue = {
			id: null,
			caption: null
		};

		var sm = this.grid.getSelectionModel();
		if (sm) {
			var selected = sm.getLastSelected();
			if (selected) {
				fieldValue = {
					id: selected.get('id'),
					caption: selected.get(Ext.String.format('{0}_caption', this.model.captionField)) ||
						selected.get(this.model.captionField)
				};
			}
		}
		return fieldValue;
	},

	viewDependencies: function () {
		var sm = this.grid.getSelectionModel();
		if (sm) {
			var selected = sm.getLastSelected();
			if (selected) {
				var itemId = selected.get('id');
				DbDependencyService.getDependent(itemId, this.entity.id, function (result, responce) {
					App.WindowMgr.add({
						title: 'Просмотр зависимостей',
						key: Ext.String.format('deps-{0}-{1}', this.entity.id, itemId),
						getWindowKey: function () {
							return this.key;
						},
						getForm: function () { return { html: result }; }
					}, {}, true);
				}, this);
				
			}
		}
	},
	/**
	* Возвращает ссылку на родительский компонент грида
	* @return {Ext.Base} Компонент, которы открыл данный грид
	*/
	getParent: function () {

		return Ext.getCmp(this.parent_id);
	},

	/**
	* Обновление отображаемого списка
	*/
	refresh: function (forced, pageNumber) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('EntitiesList::refresh');
		}
		// </debug>

		this.refreshList({}, forced, pageNumber);
	},
	
	/**
	* Обновление данных в списке
	*/
	refreshList: function (params, forced, pageNumber) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('EntitiesList::refreshList');
		}
		// </debug>

		var field = Ext.getCmp(this.field_id);
		if (field && Ext.isFunction(field.canRefresh) && forced !== true) {
			if (!field.canRefresh()) {
				// <debug>
				if (Ext.isDefined(Ext.global.console)) {
					Ext.global.console.log('Поле не может быть обновлено, так как связанные поля не имеют значений!');
				}
				// </debug>
				return;
			}
		}

		var proxy = this.grid.getStore().getProxy(),
			proxy_params = params || [];

		if ( this.searchField && this.searchField.__Value && proxy.extraParams.Search != this.searchField.__Value())
			pageNumber = 1;

		Ext.each(proxy_params, function (item) {
			proxy.setExtraParam(item.name, item.value);
		});
		this.fireEvent('beforerequest', this, proxy);

		if (Ext.getClassName(this.grid) === 'App.components.EntityTreeGrid') {
			this.grid.setRootNode({ leaf: false });
			this.grid.getRootNode().expand();
		} else {
			// Данная ф-ция вызывается из ф-ции refresh, которая в свою очередь вызывается либо напрямую, либо как обработчик события
			// В случае обработчика параметр pageNumber содержит в себе не число, а объект события. Для этого здесь вставлена проверка на число. 
			var page = Ext.isNumber(pageNumber) ? pageNumber : this.grid.getStore().currentPage;
			this.grid.getStore().loadPage(page);

		}
	},

	onBeforeLoad: function (store, operation, eOpts) {
		var proxy = store.getProxy(),
			proxy_params = [], field;

		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Добавление параметров к прокси перед запросом данных');
		}
		// </debug>

		field = this.field_id ? Ext.getCmp(this.field_id) : null;

		// Проход всех примесей, чтобы получить меню в соответствии с функциями
		Ext.iterate(this.mixins, function (name, mixin) {
			if (Ext.isFunction(mixin.getStoreParams)) {
				mixin.getStoreParams.apply(this, [proxy]);
			}
		}, this);

		if (field && Ext.isFunction(field.getDependencies)) {

			proxy_params.push({
				name: 'fieldvalues',
				value: field.getDependencies()
			});
		}

		var parent = this.getParent();
		if (parent) {
			if (parent.entity && parent.entity.identitytype == 4 /*ТЧ*/ && this.ownerdocid) {
				proxy_params.push({
					name: 'docid',
					value: this.ownerdocid
				});
			} else if (parent.docid) {
				proxy_params.push({
					name: 'docid',
					value: parent.docid
				});
			}
			proxy_params.push({
				name: 'itemid',
				value: parent.docid
			});
		}

		if (field) {
			proxy_params.push({
				name: 'fieldid',
				value: field.initialModel.id
			});
		}

		if(Ext.isDefined(this.formType) && this.formType === 3) {
			proxy.setExtraParam('IsSelectionFormRequest', true);
		}

		Ext.each(proxy_params, function (item) {
			proxy.setExtraParam(item.name, item.value);
		});
		if (Ext.isDefined(this.searchField)) {
			proxy.setExtraParam('Search', this.searchField.__Value());
		}
		if (field && Ext.isDefined(field.searchField) && Ext.isDefined(field.searchField.__Value())) {
			proxy.setExtraParam('Search', field.searchField.__Value());
		}
		proxy.setExtraParam('VisibleColumns', this.getVisibleColumnsName());
		
	},

	/**
	* Обработчик события поиска
	* @private
	*/
	onSearch: function (sender) {
		this.refreshList({},false, 1);
	},

	/**
	*
	*/
	createEntityItem: function (id, values, ownerid, protoId) {
		var field, temp;
		if (this.field_id) {
			field = Ext.getCmp(this.field_id);
			if (field && Ext.isFunction(field.getDependentGridValues)) {
				temp = field.getDependentGridValues();
			} else {
				// Создание элемента из окна, поднятого из поля выбора
			}
		}
		Ext.create('App.logic.EntityItem', {
			parent_id: this.id,
			entity: this.entity,
			docid: id,
			ownerid : ownerid,
			defaults: values,
			gridsValues: temp,
			protoId: protoId,
			selection_form: Ext.isDefined(this.selection_form),
			owner_form_id: this.owner_form_id||this.winId
		});
	},

	copyEntityItem: function(protoId) {
		this.createEntityItem(null, null, null, protoId);
	},

	/**
	* Создание элемента текущего справочника
	*/
	createItem: function () {

		var defaults = null;
		if (Ext.isFunction(this.getDefaults)) {
			defaults = this.getDefaults();
		}
		this.createEntityItem(null, defaults);
	},

	/**
	* Создание элемента копированием
	*/
	createItemAsCopy: function() {
		var sm = this.grid.getSelectionModel();
		if (sm) {
			var rows = sm.getSelection();
			Ext.each(rows, function (item) {
				this.copyEntityItem(item.get("id"));
			}, this);
		}
	},

	/**
	* Открытие выделенных элементов справочника
	*/
	openItem: function () {

		var sm = this.grid.getSelectionModel();
		if (sm) {
			var rows = sm.getSelection();
			Ext.each(rows, function (item) {
				if (this.entity.identitytype == 4 /*ТЧ*/) {
					var parent = this.getParent();
					if (parent && parent.getParent) {
						var pparent = parent.getParent();
						if (pparent && pparent.getValue && pparent.getValue('id'))
							this.createEntityItem(item.get("id"), null, parent.getParent().getValue('id').id);
						else
							this.createEntityItem(item.get("id"));
					} else
						this.createEntityItem(item.get("id"));
			} else
					this.createEntityItem(item.get("id"));
			}, this);
		}
	},

	getParentEntityId: function () {

		var owner = this.getParent(),
			entity = Ext.isDefined(owner) ? owner.entity : null;

		if (!Ext.isEmpty(entity) && Ext.isDefined(entity.id)) {
			return entity.id;
		} else {
			return null;
		}
	},

	/**
	* Функция удаления элементов из справочника
	*/
	deleteItem: function (sender, eventArgs, confirmText) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log(
				Ext.String.format('Deleting items from current entity "{0}"', this.entity.name)
			);
		}
		// </debug>

		var sm = this.grid.getSelectionModel();
		if (sm) {
			var rows = sm.getSelection();
			if (rows.length > 0) {
				Ext.MessageBox.confirm(Locale.APP_DELETEITEM_TITLE, confirmText || Locale.APP_MESSAGE_COFIRM_DELETE,
					function (button) {
						if (button === "yes") {
							var items = [];
							Ext.each(rows, function (item) {
								items.push(item.get("id"));
							}, this);
							var entity_id = this.getParentEntityId();
							CommunicationDataService.deleteItem(this.entity.id, items, entity_id, this.onItemsDeleted, this);
						}
					},
					this);
			} else {
				Ext.MessageBox.alert(Locale.APP_DELETEITEM_TITLE, Locale.APP_DELETEITEM_SHOULD_SELECT);
			}
		}
	},

	/**
	* Функция коллбэк после удаления элементов справочника
	* @private
	*/
	onItemsDeleted: function (result, response) {

		App.EntitiesMgr.registerUpdate(this.entity.id, null);
	},

	/**
	* Метод создающий обычный грид
	* @return {Object} Конфигурация готовая к созданию
	*/
	createPlainGrid: function () {

		this.grid = Ext.create('App.components.EntityGrid', {
			directFn: this.direct_function,
			actions: this.getActions(),
			searchField: this.searchField,
			customGrid: this.hasAggregates ? Ext.merge({
				features: [{
					ftype: 'summary',
					dock: 'bottom'
				}]
			}, this.customGrid || {}) : this.customGrid,
			model: this.listModel || this.model, //ToDo{CORE-280} Tablepart.ctor: this.list.getConfig();
			gridListeners: {
				scope: this,
				itemdblclick: this.onItemDblClick,
				selectionchange: this.onSelectionChange
			},
			extraParams: Ext.merge((this.extraParams ? this.extraParams : {}),
			{
				EntityId: this.override_entity_id || this.model.entityId
			})
		});

		this.grid.getStore().on('load', function () {

			this.fireEvent('storeload', this);

			if (this.grid.rendered) {
				this.grid.getSelectionModel().select(0);
			} else {
				this.grid.on('afterrender', function () {
					this.grid.getSelectionModel().select(0);
				}, this);
			}
		}, this);
	},

	/**
	* Метод создающий иерархический грид
	* @return {Object} Конфигурация готовая к созданию
	*/
	createHierarchyGrid: function () {

		// Поле по которому будет строиться иерархия
		var parent = this.model.hierarchyFields[0];

		this.grid = Ext.create('App.components.EntityTreeGrid', {
			actions: this.getActions(),
			customGrid: this.hasAggregates ? Ext.merge({
				features: [{
					ftype: 'summary',
					dock: 'bottom'
				}]
			}, this.customGrid || {}) : this.customGrid,
			hierarchyViewField: Ext.isDefined(this.formCfg) ? this.formCfg.hierarchyViewField : undefined,
			model: this.listModel || this.model, //ToDo{CORE-280},
			gridListeners: {
				scope: this,
				itemdblclick: this.onItemDblClick,
				selectionchange: this.onSelectionChange
			},
			extraParams: {
				HierarchyFieldName: parent,
				EntityId: this.override_entity_id || this.model.entityId
			}
		});

		this.grid.getStore().on('expand', function (sender, node) {

			var selectionModel = this.grid.getSelectionModel();

			if (!(selectionModel.selected && selectionModel.selected.length))
				selectionModel.select(0);
			
		}, this);
	},

	onAfterRender: function() {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Initializing UserEvents ...');
		}
		// </debug>
		this.mixins.events.constructor.call(this, {
			form_id: this.id,
			entity_name: this.entity.name,
			model_name: "List"
		});
		
		this.fireEvent('afterrender', this.getForm(), this);
	},

	/**
	* Функция формирует конфигурацию для грида
	*/
	getConfig: function () {

		var config;

		this.Buttons = Ext.create('App.logic.factory.Buttons', this.id);
		this.entity = App.EntitiesMgr.getEntityById(this.model.entityId);
		this.mixins.versioning.constructor.call(this);

		if (Ext.isEmpty(this.model.hierarchyFields || []) || this.usePlainGrid === true) {
			this.createPlainGrid();
		} else {
			this.createHierarchyGrid();
		}
		this.grid.getStore().on('beforeload', this.onBeforeLoad, this);
		this.grid.on('beforedestroy', this.onBeforeDestroy, this);
		this.grid.on('afterrender', this.onAfterRender, this);

		this.title = this.entity.caption;

		// Регистрируем подписку на событие обновление справочника
		App.EntitiesMgr.on('updates', this.onEntityUpdate, this);

		return this.grid;
	},
	
	beforeCreateForm: function () {
		
		this.getConfig();
	},

	/**
	* @private
	*/
	onModel: function (sender, model) {

		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Got into callback with model');

			if (Ext.isEmpty(model)) {
				Ext.Error.raise('EntityList -> Model should be defined!!!');
			}
		}
		// </debug>
		this.model = model;
		
		if (Ext.isArray(this.columnNames) && this.columnNames.length > 0) {
			this.prepareListModel(); // если перечень колонок передан. Для ТЧ и мультиссылок.
		} else {
			if (this.formName)
				App.FormMgr.getFormByName(this.formName, this.formType || 2, this.onForm, this); // Для StateProgram.js(198)
			else
				App.FormMgr.getForm(model.entityId, this.formType || 2, this.onForm, this);

			//CORE-802,803,808,809: отключение кеша форм решает данные проблемы.
			//if (!Ext.isDefined(this.field_id)) // загружаем форму элемента еще на этапе открытия списка. Для того, чтобы не тратить время при открытии элемента
			//    App.FormMgr.getForm(model.entityId, 1, Ext.emptyFn, this);
		}
	},

	/**
	* 
	* @private
	*/
	onForm: function (sender, formCfg) {

		if (formCfg) {
			this.formCfg = formCfg;
			this.hasAggregates = formCfg.hasAggregates;
			this.columnNames = [];
			Ext.each(this.formCfg.formItems, function (item) {
				this.columnNames.push(item.entityFieldName.toLowerCase());
			}, this);
		}
		this.prepareListModel();
	},

	/**
	 * @private
	 * Возвращает модель колонки по имени
	 */
	getColumnModelByName: function (colName, columns, colNameName) {

		var model = columns || this.model.result;
		var realName = colNameName || "name";
		
		var col = model.filter(function (colModel) {
			return colModel[realName].toLowerCase() == colName.toLowerCase();
		}, this);
		
		if (col.length == 1)
			return col[0];
		return null;
	},

	prepareListModel: function() {

		this.listModel = Ext.clone(this.model);
		if (Ext.isArray(this.columnNames) && this.columnNames.length > 0) {
			this.listModel.result = [];
			Ext.each(this.columnNames, function (colName) {
				var columnModel = this.getColumnModelByName(colName);
				//Если мы сказали в форме, что хотим вывести колонку, то мы реально хотим её вывести...
				columnModel.ishidden = false;

				if (this.formCfg) {
					var formColumnModel = this.getColumnModelByName(colName, this.formCfg.formItems, "entityFieldName");

					if (formColumnModel) {
						var properties = Ext.JSON.decode(formColumnModel.properties, true);
						Ext.apply(columnModel, properties);
					}
				}
				
				this.listModel.result.push(columnModel);
			}, this);

			// добавляем колонки модели, не входящие в состав формы списка CORE-602, в конец списка и делаем их скрытыми
			/* однако это плохо влияет на производительность.
			   После реализации CORE-969 данное поведение становится некорректным, поэтому комментирую.
			   Если по бизнес логике требуется добавление раскрытие доп. колонок, то следует расширить состав формы.
			Ext.each(this.model.result, function (column) {
				if (this.columnNames.indexOf(column.name.toLowerCase()) == -1) {
					var c = Ext.clone(column);
					c.ishidden = true;
					this.listModel.result.push(c);
				}
			}, this);*/

			if (!this.getColumnModelByName('id', this.listModel.result))
				this.listModel.result.push(this.getColumnModelByName('id'));
		}

		if (this.autoShow === true) {
			this.show();
		}
	},


	getForm: function () {

		return this.grid;
	},

	getWindowKey: function () {

		return '' + this.model.entityId;
	},

	/**
	* Данная фнукция показывает грид, зарегистрировав его в менеджере окон
	*/
	show: function () {
		// <debug>
		if (Ext.isEmpty(this.model)) {
			Ext.Error.raise('Model should be defined!!!');
		}
		// </debug>

		

		if((App.WindowMgr.add(this, Ext.merge({ iconCls: this.iconCls, width: 720 }, this.customWindow), this.openAsWindow ? false : true)) &&
		(!this.preventLoad)) {
			// Обновлять только если это не поле
			this.refresh();
		}
	},

	/**
	* @private
	* Событие будет вызываться каждый раз, когда происходит изменение данных справочника
	*/
	onEntityUpdate: function (sender, entityId, docId) {

		if (entityId && entityId === this.entity.id) {
			// <debug>
			if (Ext.isDefined(Ext.global.console)) {
				Ext.global.console.log('Пришло оповещение об изменении справочника, обновляемся.');
			}
			// </debug>
			this.refresh();
		}
	},

	/**
	 * @private
	 * Если isselectable выделенной строки грида = 0, то блокируются кнопки "Открыть","Удалить","Выбрать".
	 * @return Возвращает isselectable выделенной строки. undefined - если нет признака isselectable.
	 */
	checkIsSelectable: function() {
		var selected = this.grid.getSelectionModel().getSelection();
		var isselectable;
		if (selected && selected.length > 0 && Ext.isDefined(isselectable = selected[selected.length - 1].getData().isselectable)) {
			isselectable = isselectable !== 0; // Если явно не указано значение 0, то считаем isselectable=true. Это сделано для мультиссылок, для них isselectable не приходит с сервера, но модель данное поле содержит.
			if (Ext.isDefined(this.field_id)) {
				// => данный список является полем. Учитываем доступность поля.
				isselectable = isselectable && (Ext.getCmp(this.field_id).disabled === false);
			} 
			var action = isselectable ? 'enable' : 'disable';
			Ext.each([/*'open',ToDo{SBORIII-690} */'delete', 'select'], function (btnName) { // ToDo{SBORIII-690} кнопка Открыть теперь всегда доступна, в т.ч. и в формах выбора
				var btn = Ext.getCmp(this.id + '-button-' + btnName);
				if (btn)
					btn[action]();
			}, this);
			return isselectable;
		}
		return undefined;
	},

	/**
	* @private
	*
	*/
	onSelectionChange: function (sender, selected, eOpts) {

		this.checkIsSelectable();
		this.mixins.versioning.onSelection.apply(this, arguments);
	},

	/**
	* Обработчик события двойного клика в списке с элементами справочника.
	* Открывает элемент на котором произошло событие на редактирование.
	* @private
	*/
	onItemDblClick: function (sender, record, item, index, e, eOpts) {

		if (Ext.isDefined(this.field_id)) {
			var temp = Ext.getCmp(this.field_id);
			// ToDo{SBORIII-690}
			/*if (temp.disabled === true) {
				// <debug>
				if (Ext.isDefined(Ext.global.console)) {
					Ext.global.console.log('Поле заблокировано, открытие элементов по двойному клику невозможно.');
				}
				// </debug>
				return;
			}*/
		}

		this.createEntityItem(record.get("id"));
	},

	isXType: function (xtype) {

		return false;
	}
});