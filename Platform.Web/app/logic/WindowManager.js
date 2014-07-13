/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.WindowManager
* @mixins Ext.util.Observable
* Основной менеджер окон
*
*/
Ext.define("App.logic.WindowManager", {

    requires: [
        'App.view.SysDimensions',
        'Ext.util.MixedCollection',
        'Ext.util.Observable'
    ],

    /**
	* Используемые данным объектом mixins
	* @private
	*/
    mixins: {
        /**
		* Реализация событийной модели
		*/
        observable: 'Ext.util.Observable'
    },

    /**
    * Массив в котором хранятся все открытые окна
    * Тип массива {@link Ext.util.MixedCollection}
    * @private
    */
    windows: null,

    /**
    * Ссылка на панель задач рабочего стола
    * @private
    */
    taskbar: null,

    /**
    * @method
    * Метод открывает новое окно. Для его вызова в приложении создан глобальный объект App.WindowMgr,
    * его нужно вызывать таким образом :
    *
    *   App.WindowMgr.add('Заголовок окна', window_panel);
    *
    * @param {String} title Заголовок создаваемого окна
    * @param {Ext.container.Container} item Форма которая будет открыта в окне, должна наследоваться от {@link Ext.container.Container} 
    * @param {Object} eOpts Дополнительные параметры окна, которые будут переданы конструктору
    * @param {Bool} asPanel Открывать как панель

    **/
    add: function (container, eOpts, asPanel) {

        var win;

        win = asPanel ? this.windows.getByKey(container.getWindowKey()) : container.winId ? this.windows.getByKey(container.winId) : null;
        if (win) {
            win.click();
            return false;
        }


        if (container.beforeCreateForm) {
            container.beforeCreateForm();
        }

        var title = container.title;
        var item = container.getForm ? container.getForm() : container;
        var main = Ext.getCmp('window-container');


        var config = {
            isTop:true,
            width: 700,
            height: 400,
            title: title || 'Window',
            minimizable: true,
            maximizable: true,
            maximized: false,
            layout: 'fit',
            closable: true,
            parent_id: container.parent_id,
            owner_form_id:container.owner_form_id,
            items: [
                item
            ],
            listeners: {
                scope: this,
                beforedestroy: this.onBeforeDestroy,
                minimize: this.onMinimize,
                beforeclose: this.onClose

            }
        };

        if (Ext.isDefined(eOpts)) {

            Ext.apply(config, eOpts);
        }

        win = asPanel ? Ext.create('widget.panel', config) : Ext.create('widget.window', config);
        if (win.xtype == 'panel')
            main.add(win);
        container.winId = win.id;
        var cmp = this.taskbar.addTaskButton(win);
        cmp.click();
        var winkey = asPanel && container.getWindowKey ? container.getWindowKey() : win.id;
        win.winkey = winkey;
        if (container.on)
            container.on("windowkeychanged", this.onWindowKeyChanged, this);
        this.windows.add(winkey, cmp);
        return true;
    },

    onWindowKeyChanged: function (sender, keys) {
        var index = this.windows.indexOfKey(keys.oldvalue);
        var win = this.windows.removeAtKey(keys.oldvalue);
        this.windows.insert(index, keys.value, win);
        win.win.winkey = keys.value;
    },

    hidePanels: function () {

    },

    remove: function (container) {

        var win = this.windows.getByKey(container.getWindowKey());
        win.close();

        //this.taskbar.removeTaskButton(
        //    this.windows.get(container.getWindowKey())
        //);
    },

    removeAll: function () {

        this.windows.each(
            function () {
                this.close();
            }
        );
    },

    /**
* Обработчик события уничтожения окна
* @private
*/
    onBeforeDestroy: function (sender, param) {

        if (this.windows.containsKey(sender.winkey)) {
            this.taskbar.removeTaskButton(this.windows.removeAtKey(sender.winkey));
        }
    },

    onClose: function (sender, param) {
        var me = this;
        if (!sender.isVisible())
            return;
	//<debug>
	if (Ext.isDefined(Ext.global.console)) {
		Ext.global.console.log('WindowManager > onClose');
		Ext.global.console.dir(sender.owner_form_id);
	}
	//</debug>

	if (sender.owner_form_id) {
		var win = Ext.getCmp(sender.owner_form_id);
		
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.dir(win);
		}
		//</debug>

		if (win) {
			this.windows.getByKey(win.winkey).click();
			//win.show();
			//if (win.xtype == 'window')
			//    win.toFront();
			return;
		}
	}
        var index = -1;
        for (var j = 0; j < me.windows.getCount() ; j++) {
            if (me.windows.getAt(j).win == sender) {
                index = j;
                break;
            }
        }
        if (index == -1)
            return;
        var show = function (start) {
            for (var j = start; j < index; j++) {
                var curWin = me.windows.getAt(j).win;
                curWin.show();
                if (curWin.xtype == 'window')
                    curWin.toFront();
            }
        };

        if (sender.xtype == 'panel') {
            for (var i = index - 1; i > 0; i--) {
                var win = this.windows.getAt(i).win;
                if (win.xtype == 'panel') {
                    show(i);
                    return;
                }
            }
            show(0);
        }

    },


    /**
    * Обработчик события минимизации окна
    * @private
    */
    onMinimize: function (sender, eOpts) {

        sender.hide();
    },

    /**
    * Метод для предварительного конфигурирования менеджера, фактически нужно это убрать в будущем
    * @private
    */
    configure: function (viewport) {

        this.taskbar = viewport.taskbar;
    },

    /**
    * Конструктор, который создает новый объект данного класса
    * @param {Object} Объект с конфигурацией.
    */
    constructor: function (config) {
        this.mixins.observable.constructor.call(this, config);

        this.windows = new Ext.util.MixedCollection();
        this.addEvents(

            /**
            * @event new_window
            * @param {App.logic.WindowManager} this
            */
            'new_window',

            /**
            * @event minimized
            * @param {App.logic.WindowManager} this
            */
            'minimized',

            /**
            * @event maximized
            * @param {App.logic.WindowManager} this
            */
            'maximized',

            /**
            * @event close
            * @param {App.logic.WindowManager} this
            */
            'close'
        );
    }
});