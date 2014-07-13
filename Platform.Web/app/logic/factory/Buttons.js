/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.logic.factory.Buttons
* Фабрика колонок грида. Создает необходимые колонки в соответствии с типом {@link App.enums.EntityFieldType}
*/
Ext.define("App.logic.factory.Buttons", {
    requires: [
        'Ext.button.Button',
        'Ext.button.Split',
        'Ext.menu.Menu',
        'Ext.Action',
        'Ext.Array',
        'Ext.JSON'
    ],

    /**
    * @method
    * Метод для создания кнопки "Сохранить". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Сохранить"
    */
    save: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-save');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-save',
                text: Locale.APP_BUTTON_SAVE,
                iconCls: 'icon-save'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Сохранить и закрыть". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Сохранить и закрыть"
    */
    saveandclose: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-save-close');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-save-close',
                text: Locale.APP_BUTTON_SAVE_CLOSE,
                iconCls: 'icon-save'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * Закрыть
    */
    close: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-close');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-close',
                text: Locale.APP_BUTTON_CLOSE
                //iconCls: 'icon-close'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Редактировать". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Редактировать"
    */
    edit: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-edit');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-edit',
                hidden: true,
                text: Locale.APP_BUTTON_EDIT,
                iconCls: 'icon-edit'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Обновить". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Обновить"
    */
    refreshgrid: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-refresh');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-refresh',
                tooltip: Locale.APP_BUTTON_REFRESH,
                iconCls: 'icon-refresh',
                setAvailability: function(entityItem) {
                    this.enable(); // всегда доступна
                }
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Обновить". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Обновить"
    */
    refresh: function(config) {

        var result = Ext.getCmp(this.sender_id + '-button-refresh');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-refresh',
                tooltip: Locale.APP_BUTTON_REFRESH_ELEMENT,
                iconCls: 'icon-refresh'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки-списка "Создать"/"Создать с копированием". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Создать"
    */
    createsplit: function(createConfig, copyConfig){
        var btnId = this.sender_id + '-button-splitcreate';
        var result = Ext.getCmp( btnId );
        if (Ext.isEmpty(result)) {
            var temp = this.create(createConfig);

            Ext.apply(temp.initialConfig, {
                menu: [ this.createcopy(copyConfig) ]
            });
            
            result = Ext.create('Ext.button.Split', temp);
        }

        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Создать копированием". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Создать копированием"
    */
    createcopy: function (config) {

        var btnId = this.sender_id + '-button-createcopy';
        var result = Ext.getCmp( btnId );
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: btnId,
                text: Locale.APP_BUTTON_CREATECOPY,
                iconCls: 'icon-createcopy'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Создать". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Создать"
    */
    create: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-create');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-create',
                text: Locale.APP_BUTTON_CREATE,
                iconCls: 'icon-create'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

	/**
	* @method
	* Метод для создания кнопки "Действия". Если кнопка для данного объекта уже создавалась -
	* метод вернет ранее созданную кнопку.
	* @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Действия"
	*/
    actions: function (config) {

    	var result = Ext.getCmp(this.sender_id + '-button-actions');
    	if (Ext.isEmpty(result)) {
    	    var entitiesList = Ext.getCmp(this.sender_id);
    	    var menuItems = [];
    	    var actions = entitiesList.model.actions || [];
    	    Ext.Array.map(actions, function (action) {
    	        menuItems.push({
    	            serviceName: action.service,
    	            methodName: action.method,
    	            text: action.caption,
    	            handler: function () {
    	                var record = this.up('grid').getSelectionModel().getSelection()[0];
    	                if (record) {
    	                    var args = {};
    	                    Ext.each(record.fields.items, function (field) {
    	                        args[field.name] = record.get(field.name);
    	                    });
    	                    var service = Ext.JSON.decode(this.serviceName);
    	                    var cb = function (actions) {
    	                        Ext.each(actions || [], function(action) {
    	                            Ext.create("App.clientactions." + action.clientHandler, action).execute();
    	                        });
    	                    };
    	                    service[this.methodName](args, cb, this);
    	                }
    	            }
    	        });
    	    }, this);

    	    var temp = Ext.apply({
    			id: this.sender_id + '-button-actions',
    			text: Locale.APP_BUTTON_ACTIONS,
    			menu: menuItems,
    		    setAvailability: Ext.emptyFn
    		}, config);
    		result = Ext.create('Ext.button.Split', temp);
    	}
    	return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Создать версию". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Создать версию"
    */
    createversion: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-create-version');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-create-version',
                iconCls: 'icon-create-version',
                tooltip: Locale.APP_TOOLTIP_CREATE_VERSION,
                disabled: true
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Открыть". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Открыть"
    */
    open: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-open');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-open',
                text: Locale.APP_BUTTON_OPEN,
                iconCls: 'icon-page',
                setAvailability: function () {
                    this.enable(); // всегда доступна
                }
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Удалить". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Удалить"
    */
    del: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-delete');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-delete',
                text: Locale.APP_BUTTON_DELETE,
                iconCls: 'icon-delete'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки со списком операций. Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.button.Split} Метод возвращает новую, либо ранее созданную кнопку операций
    */
    operations: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-operations');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-operations',
                hidden: true,
                menu: Ext.create('Ext.menu.Menu')
            }, config);
            result = Ext.create('Ext.button.Split', temp);
        }
        return result;
    },
    
    registryinfo: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-registryinfo');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-registryinfo',
                hidden: true,
                text:Locale.APP_BUTTON_REGISTRY_INFO,
                menu: Ext.create('Ext.menu.Menu')
            }, config);
            result = Ext.create('Ext.button.Split', temp);
        }
        return result;
    },


    /**
    * @method
    * Метод для создания кнопки отмены выполнения операции. Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку отмены выполнения операции
    */
    operation_cancel: function(config){
        var result = Ext.getCmp(this.sender_id + '-button-cancel-operation');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-cancel-operation',
                text: Locale.APP_BUTTON_CANCEL_OPERATION,
                hidden: true,
                iconCls: 'cancel-operation'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * Метод для создания кнопки подтверждения операции. Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку подтверждения операции
    */
    operation_commit: function (config) {
        var result = Ext.getCmp(this.sender_id + '-button-commit-operation');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-commit-operation',
                text: Locale.APP_BUTTON_COMMIT_OPERATION,
                hidden: true,
                iconCls: 'commit-operation'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * @method
    * @param {Array} list Список имен классов печатных форм.
    * Кнопка со списком печатных форм.
    * @return {Ext.button.Split} Метод возвращает новую, либо ранее созданную кнопку
    */
    printForms: function (list) {
        var me = this;
        var id = me.sender_id + '-button-printforms';
        var result = Ext.getCmp(id);
        if (Ext.isEmpty(result)) {
            if (!Ext.isDefined(list))
                return undefined; // если кнопка еще не создана и не передана конфигурация для ее создания, то выходим

            var handler = function () {
                var entityItem = Ext.getCmp(me.sender_id);

                var params = {
                    entityName: entityItem.entity.name,
                    printFormClassName: this.name,
                    docId: entityItem.docid
                };
                
                App.ReportLauncher.launch(params, 'PrintForm');
            };

            if (list.length > 1) {
                var menuItems = [];
                Ext.each(list, function (item) {
                    menuItems.push(Ext.apply({ handler: handler }, item));
                }, this);

                result = Ext.create('Ext.button.Split', {
                    id: id,
                    text: 'Печать',
                    menu: menuItems
                });
            } else {
                result = Ext.create('Ext.button.Button', {
                    id: id,
                    name: list[0].name,
                    text: 'Печать',
                    handler: handler
                });
            }
        }
        return result;
    },
    
    /**
     * Кнопка "Печать" для отчетов
     */
    printReport: function (config) {
        var me = this;
        var id = me.sender_id + '-button-report';
        var result = Ext.getCmp(id);
        if (Ext.isEmpty(result)) {
            var handler = function () {
                var entityItem = Ext.getCmp(me.sender_id);

                var params = {
                    _entityName: entityItem.entity.name
                };

                entityItem.form.getFields().each(function (field) {
                    if (field.initialModel) {
                        var value;
                        if (field.getXTypes().indexOf('app_datetimefield') > -1) {
                            value = field.getValue();
                            if (value && value.toUTCString)
                                value = value.toUTCString();
                        } else if (field.getXTypes().indexOf('app_datefield') > -1) {
                            value = field.getValue();
                            if (value && value.toLocaleDateString )
                                value = value.toLocaleDateString() ;
                        } else {
                            value = field.getValue();
                            if (value && value.toString)
                                value = value.toString();
                        }
                        params[field.name] = value;
                    }
                }, this);

                if (entityItem.saveElement() === false)
                //if (entityItem.isValid())
                    App.ReportLauncher.launch(params, 'ordinalreport');
                else
                    // ToDo: более правильным было бы использовать событие полного завершения загрузки элемента, т.е. вместе со всеми ТЧ
                    entityItem.on('itemloaded', function() {
                        App.ReportLauncher.launch(params, 'ordinalreport');
                    }, this, { single: true });
            };

            result = Ext.create('Ext.Action', Ext.apply({
                id: id,
                text: Locale.APP_BUTTON_REPORT,
                iconCls: 'button-report',
                handler: handler
            }, config));
        }
        return result;
    },

    /*
    * @method
    * В справочниках 
    *
    */
    getUserActivityReport : function(config) {
        var me = this;
        var id = me.sender_id + '-button-user-activity-report';
        var result = Ext.getCmp(id);
        
        if (Ext.isEmpty(result)) {
            var handler = function () {

                var reportName = 'UserActivityReport';
                
                var entityItem = Ext.getCmp(me.sender_id);

                var refIdEntity = entityItem.model.entityId;
                var refId = entityItem.getField('id').getValue();

                var params = {
                    _entityName: reportName,
                    noSavedItem: true,
                    idAuditEntityEntity: refIdEntity,
                    idAuditEntity: refId,
                    id: refId
                };

                App.ReportLauncher.launch(params, 'ordinalreport');
            };

            result = Ext.create('Ext.Action', Ext.apply({
                id: id,
                text: Locale.APP_BUTTON_USERACTIVITY_REPORT,
                handler: handler
            }, config));
        }

        return result;
    },

    /**
    * @method
    * Метод для создания кнопки "Зависимости". Если кнопка для данного объекта уже создавалась -
    * метод вернет ранее созданную кнопку.
    * @return {Ext.Action} Метод возвращает новую, либо ранее созданную кнопку "Сохранить"
    */
    dependencies: function (config) {

    	var result = Ext.getCmp(this.sender_id + '-button-dependencies');
    	if (Ext.isEmpty(result)) {
    		var temp = Ext.apply({
    			id: this.sender_id + '-button-dependencies',
    			text: Locale.APP_BUTTON_DEPENDENCIES,
    			iconCls: 'icon-dependencies',
    			setAvailability: Ext.emptyFn
    		}, config);
    		result = Ext.create('Ext.Action', temp);
    	}
    	return result;
    },

    /**
    * Открыть профиль
    * @return {Ext.Action}
    */
    openReportProfile: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-openreportprofile');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-openreportprofile',
                text: Locale.APP_BUTTON_OPENREPORTPROFILE,
                iconCls: 'icon-page'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
    * Кнопка "Новый".
    * @return {Ext.Action}
    */
    newButton: function (config) {

        var result = Ext.getCmp(this.sender_id + '-button-new');
        if (Ext.isEmpty(result)) {
            var temp = Ext.apply({
                id: this.sender_id + '-button-new',
                text: Locale.APP_BUTTON_NEW,
                iconCls: 'icon-create'
            }, config);
            result = Ext.create('Ext.Action', temp);
        }
        return result;
    },

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {String} Идентификатор родителя.
	*/
    constructor: function (id) {

        Ext.apply(this, {

            sender_id: id
        });
    }
});