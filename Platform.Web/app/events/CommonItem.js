/**
* @class App.events.CommonItem
* Description
*/
Ext.define('App.events.CommonItem', {

   extend: 'App.events.Common',

	/**
	* Метод возвращает поле вышестоящей формы
	* @param  {String} field_name Имя поля формы
	* @return {Object} Запрошенное поле
	*/
	getField: function (field_name) {
		var form = this.getForm();
		//<debug>
		if (Ext.isEmpty(form)) {
			Ext.Error.raise('Компонент событий не может обнаружить вышестоящую форму!');
		};
		//</debug>
		return form.getField(field_name);
	},

	/**
	* Выставить видимость указанного поля формы
	* @param {String} field_name Имя поля формы
	* @param {Boolean} visibility Видимость поля
	*/
	setFieldVisibility: function (field_name, visibility) {
		var field = this.getField(field_name);
		//<debug>
		if (Ext.isEmpty(field)) {
			Ext.Error.raise(
				Ext.String.format('Невозможно найти поле с указанным именем! Имя поля :{0}', field_name)
			);
		};
		//</debug>
		if (Ext.isFunction(field.setVisible)) {

			// Выставить видимость компонента
			field.setVisible(visibility);
		}
	},

	/**
	* Спрятать поле формы
	* @param  {String} field_name Наименование поля формы
	*/
	hideField: function (field_name) {

		this.setFieldVisibility(field_name, false);
	},

	/**
	* Спрятать поля формы из переданного массива наименований
	* @param  {Array} fields_names Массив наименований полей формы
	*/
	hideFields: function (fields_names) {

		Ext.each(fields_names, function (item, index, all) {

			this.hideField(item);
		}, this);
	},

	/**
	* Показать поле формы
	* @param  {String} field_name Наименование поля формы
	*/
	showField: function (field_name) {

		this.setFieldVisibility(field_name, true);
	},

	/**
	* Показать поля формы из переданного массива наименований
	* @param  {Array} fields_names Массив наименований полей формы
	*/
	showFields: function (fields_names) {

		Ext.each(fields_names, function (item, index, all) {

			this.showField(item);
		}, this);
	},

	/**
	* Получает id выбранных строк для указаного грида
	* @param  {String} gridName Имя грида
	*/
	getSelectedRows: function (gridname) {
		var rows = this.getField(gridname).grid.getSelectionModel().getSelection();

		var result = [];
		Ext.each(rows, function (item) {
			result.push(item.get('id'));
		}, this);

		return result;
	},

	/**
	* Добавить новую кнопку в панель инструментов
	* @param  {Ext.toolbar.Toolbar} tbar  Меню
	* @param  {Object} Конфигурационный объект элемента панели инструментов (например, кнопки)
	* @param  {int} index Позиция куда вставляется
	*/
	addNewButton: function (tbar, config, index) {

		var cfg = Ext.apply({
			disabled: this.getForm().isNew, // начальное состояние
			scope: this
		}, config);

		if (cfg.id)
			cfg.id = this.form_id + "-button-" + cfg.id;
		
		if (Ext.isNumber(index)) {
			tbar.insert(index, cfg);
		} else {
			tbar.add(cfg);
		}
	},

	/**
	* Скрывет пэджинг у грида
	* @param  {String} gridName Имя грида
	*/
	hidePagging: function (gridName) {
		this.getField(gridName).grid.getDockedItems('toolbar[dock="bottom"]')[0].hide();
	},

	/**
	* Получает верхнее меню для грида
	* @param  {String} gridName Имя грида
	*/
	getTopToolBar: function (gridName) {
		return this.getField(gridName).grid.getDockedItems('toolbar[dock="top"]')[0];
	},
	
	/*
	* Скрыть колонки на гриде
	* Если переданы только скрываемые колонки -- все остальные будут отображенны.
	* Аналогично, если передали только видимые, остальные будет скрыты.
	* @param  {String}   gridName           Имя грида
	* @param  {Array}    hiddenFields       Скрытые колонки 
	* @param  {Array}    visibleFields      Видимые колонки
	*/
	hideColumnsOnGrid: function (gridName, hiddenFields, visibleFields) {
		var grid = this.getField(gridName).grid;

		// <debug>
		if (!grid && Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Указанной ТЧ не существует.');
		}
		// </debug>

		var hasVisibled = true;
		if (!visibleFields || !visibleFields.length)
			hasVisibled = false;

		var hasHidden = true;
		if (!hiddenFields || !hiddenFields.length)
			hasHidden = false;

		Ext.each(grid.headerCt.items.items, function (column) {
			var isHidden = hasHidden && Ext.Array.contains(hiddenFields, column.name.toLowerCase());
			var isVissibled = hasVisibled && Ext.Array.contains(visibleFields, column.name.toLowerCase());

			if ((!hasHidden && !isVissibled) || isHidden)
				if (column.rendered)
					column.hide();
				else
					column.hidden = true;
			else if (!hasVisibled || isVissibled)
				if (column.rendered)
					column.show();
				else
					column.hidden = false;
		});
	},
	
	/*
   * Показать/Скрыть колонки на гриде
   * @param  {String}   gridName           Имя грида
   * @param  {Array}    fields             Имена колонок
   * @param  {Boolean}    isHide             Скрывать?
   */
	setVisisibilityColumnsOnGrid: function (gridName, fields, isHide) {
		var grid = this.getField(gridName).grid;

		Ext.each(grid.headerCt.items.items, function (column) {
			if (Ext.Array.contains(fields, column.name.toLowerCase()) || Ext.Array.contains(fields, column.name)) {
				if (isHide)
					if (column.rendered)
						column.hide();
					else
						column.hidden = true;
				else
					if (column.rendered)
						column.show();
					else
						column.hidden = false;
			}
		});
	},

	/*
	* Получить id текущей сущности
	*/
	getDocId: function () {
		return this.getField('id').getValue();
	},

	/*
	* Удобное обращение к веб-сервису при нажатии на кнопку в ТЧ Документа.
	* Отправить запрос на сервис и обновить данные на клиенте.
	*
	* Сигнатуры метода на веб-сервиса:
	*   1) Если задан  только    параметр   service: A(int docId);
	*   2) Если задан  параметр         forGridRows: A(int docId, int[] rowIds);
	*   3) Если задан  параметр   withCommunication: A(CommunicationContext context, int docId, int[] rowIds);
	*   4) Если заданы параметры withCommunication, forGridRows, additionalParams: (например additionalParams = [1, 'test', [1,2,3] ] ) -- A(CommunicationContext context, int docId, int[] rowIds, int idSmth, string errorText, int[] anyIds);
	*   5) Если заданы параметры additionalParams, и не заданы withCommunication, forGridRows: -- A(int docId, int idSmth, string errorText, int[] anyIds);
	*
	* @param  {object}   service            Веб-сервис
	* @param  {Array}    gridRefresh        Гриды, которые обновятся после заверешения запроса
	* @param  {String}   forGridRows        Включить в запрос идентификаторы выделенных строк в ТЧ с именем forGridRows
	* @param  {Array}    additionalParams   Дополнительные параметры запроса
	* @param  {Function} customCallBack     Метод обработчик завершения запроса
	*/
	callServiceFromGrid: function (service, gridRefresh, forGridRows, additionalParams, customCallBack) {
		var docId = this.getDocId();
		if (!docId)
			return;

		//Засеряем гриды, которые обновятся после запроса
		Ext.each(gridRefresh, function (name) { this.getField(name).getEl().mask(); }, this);

		var args = [];

		//Идентификатор элемента
		args.push(docId);

		//Добавляем идентификаторы выбранных строк текущего грида
		if (forGridRows)
			args.push(this.getSelectedRows(forGridRows));

		//Добавляем дополнительные параметры
		if (Ext.isArray(additionalParams))
			args.concat(additionalParams);

		//Формируем callBack
		var callBack = function () {

			if (Ext.isFunction(customCallBack))
				customCallBack.call();

			//Обновляем гриды
			Ext.each(gridRefresh, function (name) {
				this.getField(name).refresh();
				this.getField(name).getEl().unmask();
			}, this);
		};
		args = args.concat([callBack, this]);

		service.apply(service, args);
	}
});