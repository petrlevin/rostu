/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*/
/**
* @class App.common.Profile
* @singleton
* Профиль пользователя, содержащий все необходимые вызовы к серверной части
*/

Ext.define("App.common.Profile", {
    requires: [
        'Ext.window.MessageBox',
        'Ext.util.Observable',
        'Ext.state.Stateful',
        'App.view.ChangePassword',
        'App.logic.SysDimensions'
    ],

    /**
	* Используемые данным объектом mixins
	* @private
	*/
    mixins: {
        /**
		* Реализация событийной модели
		*/
        observable: 'Ext.util.Observable',

        /**
		* Сохранение состояния объекта
		*/
        state: 'Ext.state.Stateful'
    },

    /*
	* Указываем миксину, идентификатор для сохранения состояния компонента
	*/
    stateId: 'lastUser',

    /*
	* Указываем миксину, что мы будем сохранять состояние компонента
	*/
    stateful: true,

    /*
	* Имя пользователя, который был залогинен в систему последний раз
	*/
    lastUser: null,

    /*
	* Имя залогиненного пользователя
	*/
    userName: null,

    /*
    
    */
    rememberUser: false,
    
    
    /**
	* Метод вызывает логин на сервере
	*/
    login: function (values) {
        this.rememberUser = (values.remember && values.remember == 'on');

        ProfileService.login(values.user, values.pass, this.onLoginResponse, this);
    },

    /*
	* Функция запрашивает завершение сессии на сервере
	*/
    logout: function () {
        localStorage.clear();

        ProfileService.logout(function () {
            this.fireEvent('logged_out', this);
        }, this);
    },

    /*
	* Функция запрашивает у сервера состояние авторизации. Залогинен ли пользователь.
	*/
    checkLogged: function () {
        ProfileService.isLogged(this.onLoggedResponse, this);
    },

    /*
    * Выдать сообщение о недействительной лицензии
    */
    showNotLicensedMsg: function () {
        var me = this;
        Ext.Msg.show({
            title: 'Лицензия не действительна.',
            msg: 'Пользователь не лицензирован для выбранного ППО.',
            buttons: Ext.MessageBox.OK,
            icon: Ext.Msg.Error,
            fn: function (btn) {
                if (btn == 'ok')
                    me.logout();
            }
        }
        );
    },

        /*
	* Обработчик ответа сервера на запрос авторизации
	* @private
	*/
    onLoginResponse: function (response) {
        var me = this;
        if (response && response.authenticated) {
            this.userName = response.username;
            if (response.needChangePassword)
                this.changePassword(function() {
                    me.onLoginResponseSuccess(response);
                });
            else
                this.onLoginResponseSuccess(response);
        } else {
            this.fireEvent('auth_failed', this);
        }
    },
    
    hasSavedSysDimensions: function () {
        return (Ext.util.Cookies.get("PublicLegalFormation") && Ext.util.Cookies.get("Budget") && Ext.util.Cookies.get("Version"));
    },

    onLoginResponseSuccess: function (response) {

        if (this.rememberUser)
            this.saveUserLocal(response);

        
        if (this.hasSavedSysDimensions() )
            this.fireEvent('logged_in', this);
        else
            this.fireEvent('need_params', this, function (sender, values) {
                ProfileService.setSysDimensions(values, function () {
                    this.fireEvent('logged_in', this, response);
                }, this);
            }, this);

    },

    saveUserLocal: function (response) {
        var authCookie = Ext.util.Cookies.get(response.ticketName);
        localStorage.setItem(response.ticketName, authCookie);
    },


    tryLoginBySavedUser: function(cookieName) {
        var authCookie = localStorage.getItem(cookieName);
        if (!authCookie)
            return false;

        localStorage.removeItem(cookieName);

        ProfileService.autoLogin(authCookie, this.onLoginResponse, this);
        return true;
    },

        /**
	* Обработчик ответа сервера на запрос статуса авторизации
	* @private
	*/
    onLoggedResponse: function (response) {

        this.logged = response.logged;

        if (this.logged === true ) {
            this.userName = this.lastUser;
            this.fireEvent('logged_in', this, null);
        } else {
            if (!this.tryLoginBySavedUser(response.authCookieName))
                this.fireEvent('not_logged', this, null);
        }
    },

    /**
	* Данный метод возвращает логин текущего залогиненного в системе пользователя
	*/
    getUserName: function () {

        return this.userName;
    },

    /**
	* Реализация mixin Stateful
	* Данный метод вызывается когда объект собирается сохранить свое состояние
	*/
    getState: function () {

        return { name: this.userName };
    },

    /**
	* Реализация mixin Stateful
	* Данный метод вызывается когда объект восстанавливает состояние
	*/
    applyState: function (state) {

        if (state && state.name) {
            this.lastUser = state.name;
        }
    },

    changePassword: function (onchanged) {

        var view = Ext.create('App.view.ChangePassword');
        if ( Ext.isFunction(onchanged))
            view.on("passwordchanged", onchanged,this);

        view.show();
    },

    checkBandWidth : function() {

        Ext.Msg.show({
            title: 'Идет диагностика интернет-соединения',
            msg: 'Пожалуйста, подождите. Идет оценка вашего соединения',
            width: 300,
            //buttons: Ext.Msg.OK,
            icon: Ext.window.MessageBox.INFO
        });

        var size = 10;
        
        var start = Date.now();
        Ext.Ajax.request({
            url: 'Resources/testFiles/'+size+'MB.bin',
            method: 'GET',
            success: function () {
                var end = Date.now();
                var downloadTime = (end - start);
                //Скорость в мегабайтах в сек
                var downloadSpeed = (1000 * (size / downloadTime)).toFixed(2);

                start = Date.now();
                Ext.Ajax.request({
                    url: 'Resources/testFiles/ping.bin',
                    method: 'GET',
                    success: function () {
                        end = Date.now();
                        //Пинг в миллесекундах
                        var ping = Math.round( (end - start) / 2 );
                        
                        ProfileService.setUserBandWidth({ ping: ping, downloadSpeed: downloadSpeed }, function () {
                            Ext.Msg.hide();
                            App.Profile.changeSysDimensions();
                        });
                        
                    },
                    failure: function (response, opts) {
                        "Характеристики соединения не удовлетворяют минимальным требованиям.";
                    }
                });
            },
            failure: function (response, opts) {
                "Характеристики соединения не удовлетворяют минимальным требованиям.";
            }
        });
    },

    /**
    * Запрос на получение системных значений
    */
    changeSysDimensions: function () {
        var dimensions = Ext.create('App.logic.SysDimensions', {
            hasSaved: this.hasSavedSysDimensions(),
            handler: function (sender, values) {
                ProfileService.setSysDimensions(values, function () {
                    this.fireEvent('logged_in', this, {'restart':true});
                }, this);
            },
            scope: this
        });
    },

    /**
* Конструктор, который создает новый объект данного класса
* @param {Object} Объект с конфигурацией.
*/
    constructor: function (config) {

        /**
		* @private
		* Инициализация миксинов
		*/
        this.mixins.observable.constructor.call(this, config);
        this.mixins.state.constructor.call(this, config);

        this.addEvents(
			/**
			* @event logged_in
			* Событие уведомляющее о том, что пользователь залогинен в системе
			* Выбрасывается после обращения к методу сервера для проверки состояния сессии клиента
			* <b>ProfileService.isLogged</b> или запроса на логин <b>ProfileService.login</b>
			* @param {Ext.Component} this
			* @param {Object} response будет null если была проверка состояния
			*/
			'logged_in',
			/**
			* @event not_logged
			* Событие уведомляющее о том, что пользователь не залогинен в системе
			* Выбрасывается после обращения к методу сервера для проверки состояния сессии клиента:
			* <b>ProfileService.isLogged</b>
			* @param {Ext.Component} this
			*/
			'not_logged',
			/**
			* @event logged_out
			* Событие уведомляющее о завершении сессии пользователя
			* @param {Ext.Component} this
			*/
			'logged_out',
			/**
			* @event auth_failed
			* Событие уведомляющее о неудачной аутентификации
			* @param {Ext.Component} this
			*/
			'auth_failed',
			/**
			* @event need_params
			* Событие которое выбрасывается в случае, если от пользователя требуется ввести дополнительные данные
			* для того, чтобы войти в систему
			* @param {Ext.Component} this
			* @param {Object} parameters Конфигурация параметров, которые должны быть запрошены у клиента дополнительно
			*/
			'need_params'
		);
        this.addStateEvents('logged_in');
        
        /*Ext.define('Search', {
            fields: ['id', 'query'],
            extend: 'Ext.data.Model',
            proxy: {
                type: 'localstorage',
                id: 'twitter-Searches'
            }
        });*/

    }
});
