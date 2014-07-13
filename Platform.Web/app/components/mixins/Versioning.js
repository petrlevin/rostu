/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.mixins.Versioning
* Примесь для работы с версионными справочниками
*/
Ext.define("App.components.mixins.Versioning", {

    uses: [
        'App.logic.EntityItem'
    ],

    requires: [
        'Ext.tip.ToolTip',
        'Ext.form.field.Date'
    ],

    /**
    * Метод возвращает признак того, что данная сущность версионная
    * @return {Boolean}
    */
    getVersioning: function () {
        var me = this;

        return me.entity.isversioning;
    },

    /**
    * Данный метод должен вернуть массив кнопок, которые будут отражены в гриде
    * @return {Array}
    */
    getActions:function() {
        var me = this,
            result = [];

        if (me.getVersioning() === true) {
            result = result.concat([
                me.Buttons.createversion({
                    handler: me.createVersion,
                    disabled: true,
                    scope: me
                }),
                me.versionDate = Ext.create('App.components.fields.DateTime', {
                    id:me.id+'-versioning-date',
                    width: 80,
                    value: new Date(),
                    format: 'd.m.y',
                    plugins: [Ext.create('Ext.ux.InputTextMask', '99.99.99')],
                    listeners: {
                        afterrender: {
                            element: me.el,
                            fn: me.onAfterRender,
                            scope: me
                        }
                    }
                })
            ]);
        }
        return result;
    },

    /**
    * Метод дополняет передаваемое хранилище дополнительными параметрами необходимыми для функционирования примеси
    */
    getStoreParams: function (proxy) {
        var me =this;

        if (me.getVersioning()=== true) {
            var field = me.versionDate || Ext.getCmp(me.id + '-versioning-date');
            if(Ext.isEmpty(field)){
                //<debug>
                Ext.Error.raise('Произошла ошибка! В версионной сущности не можем найти поле для считывания версии!');
                //</debug>
            } else {
                proxy.setExtraParam('ActualDate', field.getValue());
            }
        }
    },

    /**
    * Метод вызывает серверный метод для создания версии выбранного элемента
    */
    createVersion: function () {
        var me = this;

        var sm = me.grid.getSelectionModel();
        if (sm) {
            var selected = sm.getLastSelected();
            if (selected) {
                CommunicationDataService.createVersion(me.entity.id, selected.get('id'), me.docid, me.onCreateVersion, me);
            }
        }
    },

    /**
     * Это обработчик события должен поставить тултип на поле ввода даты актуальности
     * @param  {Ext.form.field.Date} sender Поле на котором произошло событие
     * @param  {Object} eOpts  Параметры события
     * @private
     */
    onAfterRender: function(sender, eOpts) {
        Ext.create('Ext.ux.InputTextMask', {
            target: sender.getEl(),
            inputMask: "99.99.99"
        });

        Ext.create('Ext.tip.ToolTip', {
            target: sender.getEl(),
            html: Locale.APP_TOOLTIP_ACTUAL_DATE
        });
    },

    /**
    * @private
    */
    onCreateVersion: function (result, response) {
        var me = this;

        if (response.type !== 'rpc') return;

        var temp = Ext.create('App.logic.EntityItem', {
            entity: me.entity,
            docid: id,
            preventLoad: true
        });
        temp.setData(result, null);
    },

    /**
    * Данный метод вызывается, чтобы проверить статус кнопки и сделать ее видимой
    * @private
    */
    onSelection: function (sender, selected, eOpts) {
        var me = this;

        if (me.getVersioning()) {

            me.Buttons.createversion().setDisabled(Ext.isEmpty(selected) || selected.length !== 1);
        }
    },

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        var me = this;

        Ext.apply(me, config);
    }
});