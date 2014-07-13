/**
* @class App.events.Common
* Description
*/
Ext.define('App.events.Common', {

   /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Общий конструктор объектов обработчиков клиентских событий.');
		}
		// </debug>

		Ext.apply(this, config);

		if (this.events) {
			// <debug>
			if (Ext.isDefined(Ext.global.console)) {
				Ext.global.console.log('Создание предварительно задекларированных событий.');
			}
			// </debug>
			Ext.each(this.events, function (event) {

				this.setEvent(event);
			}, this);
		}
	},

	/**
	* Метод установления события по конфигурации
	* @param {Object} event Конфигурация события
	*/
	setEvent: function (event, handler, field_name) {
		var temp, field, listener;

		if (Ext.isString(event)) {
			temp = {
				name: event,
				handler: handler,
				item: field_name
			}
		} else {
			temp = event;
		}

		listener = temp.item === null ? this.getForm() : this.getField(temp.item);
		listener = listener || this.getForm()[temp.item];

		if (Ext.isEmpty(listener)) {
			Ext.Msg.show({
				title: 'Ошибка событий',
				msg: Ext.String.format('Ошибка в обработчике события "{0}", невозможно найти поле "{1}"!',
					temp.name,
					temp.item
				),
				buttons: Ext.Msg.OK,
				icon: Ext.MessageBox.ERROR
			});
		} else {
			// Установка обработчика события
			try {
				listener.on(temp.name, this[temp.handler], this);
			} catch (ex) {
				Ext.Msg.show({
					title: 'Ошибка событий',
					msg: Ext.String.format('Ошибка в обработчике события "{0}", ошибка выставления обработчика!</br>Текст ошибки : "{1}"!',
						temp.name,
						ex.message
					),
					buttons: Ext.Msg.OK,
					icon: Ext.MessageBox.ERROR
				});
			}
		}

	},

	/**
	* Метод возвращает вышестоящую форму данного объекта событий
	* @return {App.logic.EntityItem} Родительская форма
	*/
	getForm: function () {

		return Ext.getCmp(this.form_id);
	},
	
	/**
	* Получает кнопку
	* @param  {toolbar}  tbar    Меню
	* @param  {String}   buttons Ключ для поиска кнопки, например "create" или "delete" или "open" или аналогичные
	*/
	getButton: function (tbar, button) {
		var result = null;
		tbar.items.items.forEach(function (item) {
			if (new RegExp("-button-(" + button + ")$").test(item.id)) {
				result = item;
			}
		});
		return result;
	},

	/**
	* Скрывает кнопки
	* @param  {toolbar}  tbar    Меню
	* @param  {String}   buttons Ключ для поиска кнопки, например "create|delete" или "create" или "open|create" или аналогичные
	*/
	hideButtons: function (tbar, buttons) {
		tbar.items.items.forEach(function (item) {
			if (new RegExp("-button-(" + buttons + ")$").test(item.id)) {
				item.hide();
			}
		});
	},

	/**
	* Отключает/включает кнопки
	* @param  {toolbar}  tbar    Меню
	* @param  {String}   buttons Ключ для поиска кнопки, например "create|delete" или "create" или "open|create" или аналогичные
	*/
	controlButtons: function (tbar, buttons, enable) {
		tbar.items.items.forEach(function (item) {
			if (new RegExp("-button-(" + buttons + ")$").test(item.id)) {
				if (enable) {
					item.enable();
				} else {
					item.disable();
				}
			}
		});
	}
});