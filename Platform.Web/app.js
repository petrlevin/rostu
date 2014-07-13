/// <reference path="Overrides.js" />
/*
*
* @brief Основной класс приложения
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
* Andrew Shmelev (andrew.shmelev@gmail.com)
*/
/**
* Основные настройки приложения,
* путь приложения и путь библиотеки ExtJS
*/

Ext.Loader.setConfig({
    paths: {
        "App": "app",
        "Ext": "ext/src"
    },
    disableCaching: false // отключено добавление ?_dc= при загрузке скриптов для того, чтобы при отладке при f5 не отваливались брейкпоинты. Для клиента это не нужно, т.к. он должен использовать минифицированный js
});

Ext.require([
    "Ext.direct.*",

    "Ext.state.CookieProvider",

    "Ext.grid.feature.GroupingSummary",
    "Ext.grid.plugin.RowEditing",
    "Ext.grid.RowEditor",

    "Ext.layout.container.Border",
    "Ext.layout.container.Column",
    "Ext.layout.container.Fit",
    "Ext.layout.container.VBox",
    "Ext.layout.container.HBox",

    "Ext.view.TableLayout",

    "Ext.toolbar.Paging",
    "Ext.toolbar.Toolbar",

    "Ext.form.FieldContainer",
    "Ext.form.FieldSet",

    "Ext.form.field.VTypes",

    "Ext.tab.Panel",
    "Ext.panel.Table",

    "App.common.Locale",
    "App.common.Guid",
    "App.common.Ids",
    "App.common.Profile",

    'App.logic.HttpRequest',
    'App.logic.mixins.Columns',
    'App.logic.factory.Fields',
    'App.logic.EnumsManager',
    'App.logic.EntitiesList',
    'App.logic.EntityItem',
    'App.logic.LogsManager',
    'App.logic.ModelManager',
    'App.logic.FormManager',
    'App.logic.WindowManager',
    'App.logic.EntitiesManager',
    'App.logic.StatusManager',

    'App.patches.Summary',
    'App.patches.Connection',

    'App.direct.ControlingProvider',

    'App.clientactions.*',
    'App.components.*',
    'App.exceptions.*',
    'App.events.*'
]);

Ext.Loader.loadScript('app/Overrides.js');

// DO NOT DELETE - this directive is required for Sencha Cmd packages to work.
//@require @packageOverrides

Ext.application({

	/**
	* Отключаем создание вьюпорта, он должен создаться, только когда пользователь авторизуется
	* и введет все необходимые параметры
	*
	* @property
	* @type Boolean
	*/
	autoCreateViewport: false,

	/**
	* Наименование приложения
	*
	* @property
	* @type String
	*/
	name : 'App',

	/**
	* Контроллеры приложения, они прямо здесь будут инициализированы в момент инициализации приложения
	*/
	controllers: [
		'Application',
		'Login'
	],

	views: [
		'Login',
		'ChangePassword',
		'Logs',
		'Desktop',
		'SysDimensions'
	],

	stores: [
		'Navigation'
	],

	/**
	* Метод, который будет вызван при запуске приложения
	*/
	launch: function () {

		// Подготовка приложения
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Запуск приложения');
		}
		//</debug>

		/**
		* Проблема обрезания подсказки, решение взято тут :
		* http://stackoverflow.com/questions/15834689/extjs-4-2-tooltips-not-wide-enough-to-see-contents
		*/
		delete Ext.tip.Tip.prototype.minWidth;

		/**
		* Инициализация API удаленного вызова процедур сервера
		*/
		Ext.app.REMOTING_API.enableBuffer = 500;
		Ext.direct.Manager.addProvider(Ext.app.REMOTING_API);

		/**
		* Настройки глобальных менеджеров приложения
		* Менеджер логов (фактически выдает сообщение в окно "Сообщения"
		*
		* Пользоваться так :
		* Logs.log('message to show', Logs.INFO / Logs.WARN / Logs.ERROR);
		* <p>Линк на глобальный менеджер логов ({@link App.logic.LogsManager})</p>
		*/
		App.Logs = Ext.create('App.logic.LogsManager');

		/**
		* Менеджер моделей, регистрация кэша моделей
		* <p>Линк на глобальный менеджер моделей ({@link App.logic.ModelManager})</p>
		*/
		App.ModelMgr = Ext.create('App.logic.ModelManager');

		/**
		* Менеджер форм, регистрация кэша форм
		* <p>Линк на глобальный менеджер форм ({@link App.logic.FormManager})</p>
		*/
		App.FormMgr = Ext.create('App.logic.FormManager');

		/**
		* Профиль пользователя.
		* Здесь находятся основные методы управления сессией пользователя
		* <p>Линк на глобальный менеджер профиля ({@link App.common.Profile})</p>
		*/
		App.Profile = Ext.create('App.common.Profile');

		/**
		* @property
		* Менеджер окон
		* Здесь будут регистрироваться все формы, которые надо отобразить в окне
		* <p>Линк на глобальный менеджер окон ({@link App.logic.WindowManager})</p>
		*/
		App.WindowMgr = Ext.create('App.logic.WindowManager');

		/**
		* Менеджер списка основных справочников
		* <p>Линк на глобальный менеджер справочников ({@link App.logic.EntitiesManager})</p>
		*/
		App.EntitiesMgr = Ext.create('App.logic.EntitiesManager');

		/**
		* <p>Линк на глобальный менеджер статусов ({@link App.logic.StatusManager})</p>
		*/
		App.StatusMgr = Ext.create('App.logic.StatusManager');
	    
	    /**
		* <p>Линк на глобальный менеджер отчетов ({@link App.logic.ReportLauncher})</p>
		*/
		App.ReportLauncher = Ext.create('App.logic.ReportLauncher');
	    
		App.EnumsMgr.getEnums();
	}
});
