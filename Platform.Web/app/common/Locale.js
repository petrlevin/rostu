/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* @author Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*/
/**
* @class App.common.Locale
* @singleton
* Текстовые константы для приложения
*/

Ext.define("App.common.Locale", {

	/**
	* @private
	*/
    singleton: true,

    // Общие строковые константы
    APP_COMMON_LOADING: "Идет загрузка ...",

	// Кнопки
	APP_BUTTON_START: "Старт",
	APP_BUTTON_LOGIN: "Войти",
    APP_BUTTON_NEW: 'Новый',
    APP_BUTTON_OPEN: "Открыть",
    APP_BUTTON_CANCEL: "Отменить",
	APP_BUTTON_EDIT: "Редактировать",
	APP_BUTTON_DEPENDENCIES: "Зависимости",
	APP_BUTTON_DELETE: "Удалить",
	APP_BUTTON_CREATE: "Создать",
	APP_BUTTON_CREATECOPY: "Создать копированием",
	APP_BUTTON_SAVE: "Сохранить",
	APP_BUTTON_CLOSE: "Закрыть",
	APP_BUTTON_SAVE_CLOSE: "Сохранить и закрыть",
	APP_BUTTON_REFRESH: "Обновить",
	APP_BUTTON_REFRESH_ELEMENT: "Обновить элемент",
	APP_BUTTON_CANCEL_OPERATION: "Отменить",
	APP_BUTTON_COMMIT_OPERATION: "Применить",
    APP_BUTTON_SELECT: "Выбрать",
    APP_BUTTON_ACTIONS: "Действия",
    APP_BUTTON_IMPORT: "Импорт из Excel",
    APP_BUTTON_IMPORTXML: "Импорт из XML",
    APP_BUTTON_EXPORT: "Экспорт в Excel",
    APP_BUTTON_EXPORTXML: "Экспорт в XML",
    APP_BUTTON_REGISTRY_INFO: "Регистры",
    APP_BUTTON_REPORT: "Печать",
    APP_BUTTON_USERACTIVITY_REPORT: "История изменений",
    APP_BUTTON_MULTIEDIT: "Редактировать выделенное",
    APP_BUTTON_MULTIEDITSTATUS: "Изменить статус",
    APP_BUTTON_MULTIEDITOPERATION: "Выполнить операцию",
    APP_BUTTON_OPENREPORTPROFILE: 'Открыть профиль',

	// Константы для полей форм
	// Форма логина
	APP_LOGINFORM_TITLE : "Вход",
	APP_LOGINFORM_USERNAME : "Логин",
	APP_LOGINFORM_PASSWORD: "Пароль",
    APP_LOGINFORM_REMEMBER: "Запомнить меня",

    //Форма системных измерений
	APP_SYSDIMENSIONSFORM_TITLE: "Системные измерения",
	
    //Форма множественного редактирования
	APP_MULTIEDIT_FORM_MULTIFIELDS: "Невозможно задать для одного поля несколько значений!",
    APP_MULTIEDIT_FORM_NOFIELDS: "Отсутствует поля, доступные для редактирования.",
    APP_MULTIEDIT_SELECTEDNOTNEWREF: "Разрешено редактировать только элементы в статусе 'Новый'",
    APP_MULTIEDIT_NOSELECTED: "Не выделен ни один элемент",

    // Форма смены пароля
	APP_CHANGEFORM_BTN_CHANGE: "Сменить",
	APP_CHANGEFORM_BTN_CANCEL: "Отмена",
	APP_CHANGEFORM_TITLE: "Смена пароля",
	APP_CHANGEFORM_NEWPASSWORD : "Новый пароль",
	APP_CHANGEFORM_OLDPASSWORD : "Старый пароль",
	APP_CHANGEFORM_NEWPASSWORD_REPEAT: "Повторите новый пароль",

    // Сообщения системы контролей
    APP_CONTROL_EXCEPTION: "Сообщение контроля",

	// Константы для сообщений пользователю
	APP_MESSAGE_LOGINFAILED: "Ошибка входа в систему",
	APP_MESSAGE_EXCEPTION: "Произошла ошибка!",
	APP_MESSAGE_COFIRM_DELETE: "Вы действительно хотите удалить выделенные элементы?",
	APP_MESSAGE_FORM_INVALID: "На форме присутствуют некорректно заполненные поля.",
	APP_MESSAGE_FORM_WARNING: "Предупреждение",
    APP_MESSAGE_SHOULD_BE_SELECTED:"Должна быть выбрана хотя бы одна запись для добавления!",

    // Всплывающие подсказки
	APP_TOOLTIP_CREATE_VERSION: "Создать версию",
	APP_TOOLTIP_ACTUAL_DATE: "Дата актуальности",

	// Константы для десктопа
	APP_NAVIGATION_COLLAPSE : "Свернуть все",
	APP_NAVIGATION_EXPAND : "Развернуть все",
	APP_QUICKSTART_MINIMIZE : "Свернуть все окна",
	APP_QUICKSTART_CASCADE : "Расположить окна каскадом",
	APP_MENU_LOGOUT: "Выйти из системы",
	APP_MENU_CHANGE_PASWORD: "Смена пароля",
	APP_MENU_CHANGE_SYSDIMENSIONS: "Сменить параметры входа",
	APP_MENU_REFERENCES : "Справочники",
	APP_MESSAGES_TITLE : "Сообщения",
	APP_NAVIGATION_TITLE: "Навигация",
	APP_NAVIGATION_SEARCH: "Поиск",
	APP_WAIT_EXPANDING: "Разворачиваем все узлы...",

	APP_DELETEITEM_TITLE: "Удаление элементов",
	APP_DELETEITEM_SHOULD_SELECT: "Необходимо выделить элементы для удаления!",
	
    // Сообщения ошибок клиентской валидации
    VTYPE_FIELDS_DO_NOT_MATCH: "Пароли не совпадают!"
}, function() {

	Locale = this;
});
