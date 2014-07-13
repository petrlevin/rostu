/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.mixins.Operations
* Примесь для работы с операциями
*/
Ext.define("App.components.mixins.Operations", {

    requires: [
        'Ext.util.Observable',
        'Ext.menu.Item',
        'Ext.Error'
    ],

    mixins: {
        observable: 'Ext.util.Observable'
    },
    
    /**
     * Таймаут при выполнении операции. 
     * Время прошедшее с момента вызова операции (выполнение атомарной операции, завершение неатомарной, отмена неатомарной), 
     * по прошествии которого в случае отсутствия ответа от сервера запрос считается невыполненным.
     * Мотивом для введения отдельного таймаута для операций служит то, 
     * что логика операций может быть очень сложной и операции могут долго обрабатываться на сервере.
     * Указание таймаута при обращение к методам веб-сервисам отменяет пакетную отправку запросов.
     */
    timeout: 15 * 60 * 1000,

    /**
    * Проверяет, есть ли в данных описание операций
    * @method
    * @return
    */
    hasOperations: function (data) {

        return !Ext.isEmpty(data.operations) || !Ext.isEmpty(data.currentOperationId);
    },

    /**
    * Обновление списка операций в кнопке вызова
    * @method
    */
    update: function (data) {
        if (Ext.isEmpty(data) || !this.hasOperations(data)) {
            return;
        }

        // Здесь необходимо заблокировать все поля родителя
        this.Buttons.saveandclose().setVisible(false);
        this.Buttons.save().setVisible(false);
        this.Buttons.refresh().setVisible(false);
        this.Buttons.edit().setVisible(false);

        if (Ext.isEmpty(data.currentOperationId)) {
            this.blockFields();
            var split = this.Buttons.operations();
            if (split) {
                split.menu.removeAll();
                Ext.each(data.operations.list, function (item) {
                    if (item.name.toLowerCase() === 'edit') {
                        this.Buttons.edit().setVisible(true);
                        this.edit_operation = {
                            entity: this.entity.id,
                            doc: this.docid,
                            operation: Ext.apply({}, item)
                        };
                    } else {
                        split.menu.add(Ext.create('Ext.menu.Item', {
                            text: item.text,
                            handler: function () {
                                this.executeOperation(this.entity.id, this.docid, Ext.apply({}, item));
                            }.bind(this)
                        }));
                    }
                }, this);
                split.setVisible(this.hasOperations(data));
                split.setText(data.operations.caption);
            }
        } else {
            // Сохраним информацию о текущей операции, чтобы в дальнейшем она была всегда легко доступна
            var operation = data.operations.list.filter(function (op) { return op.id == data.currentOperationId; })[0] || data.currentOperationId;
            
            this.current_operation = {
                entityId: this.entity.id,
                docId: this.docid,
                operation: operation
            };
            this.blockFields(data.editableFields);
            this.beginOperation();
        }
    },

    /**
    * Метод возвращающий набор кнопок для формы
    * @method
    */
    getActions: function(){

        return [
            this.Buttons.edit({
                handler: this.launchEditOperation,
                scope: this
            }),
            this.Buttons.operations(),
            this.Buttons.operation_commit({
                handler: this.commitOperation,
                scope: this
            }),
            this.Buttons.operation_cancel({
                handler: this.cancelOperation,
                scope: this
            })
        ];
    },

    /**
    * Метод, который будет вызван для выполнения операции
    * @method
    */
    executeOperation: function (entityId, docId, operation) {

        var opts = { timeout: this.timeout };
        if (operation.isAtomic) {
            OperationsService.exec(entityId, docId, operation.id, this.onExecutedOperation, this, opts);
        } else {
            // Сохраним информацию о текущей операции, чтобы в дальнейшем она была всегда легко доступна
            this.current_operation = {
                entityId: entityId,
                docId: docId,
                operation: operation
            };
            OperationsService.beginOperation(entityId, docId, operation.id, this.onBeginOperation, this, opts);
        }
    },

    /**
    * Запуск операции редактирования, шорткат
    */
    launchEditOperation: function () {

        this.Buttons.edit().setVisible(false);
        this.executeOperation(this.edit_operation.entity, this.edit_operation.doc, this.edit_operation.operation);
    },

    onBeginOperation: function(result, event, success, eOpts) {

        if (success === true) {

            this.elementData = result;
            this.blockFields(result.editableFields);
            this.beginOperation();
        } else {
            
            this.blockFields();
            this.Buttons.edit().setVisible(true);
        }
    },

    /**
    * Метод начинающий выполнение неатомарной операции
    * @method
    */
    beginOperation: function () {

        if (!Ext.isDefined(this.Buttons)) {
            // <debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Buttons factory is not defined in entity item!');
            }
            Ext.Error.raise('Buttons factory is not defined in entity item!');
            // </debug>
            return;
        }

        // Hide operations button
        this.Buttons.operations().setVisible(false);
        this.Buttons.operation_cancel().setVisible(true);
        this.Buttons.operation_commit().setVisible(true);

        this.fireEvent('operationbegin', this);
    },


    /**
    * Метод подтверждающий выполнение неатомарной операции
    * @method
    */
    commitOperation: function () {
        if (Ext.isEmpty(this.current_operation)) {
            // <debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Calling Operations -> commitOperation without information about current operation');
            }
            // </debug>
            return;
        }
        OperationsService.completeOperation(
            this.current_operation.entityId,
            this.current_operation.docId,
            this.formValue(),
            this.operationDone,
            this,
            { timeout: this.timeout }
        );
        this.blockFields();
    },


    /**
    * Метод отменяющий выполнение неатомарной операции
    * @method
    */
    cancelOperation: function () {
        if (Ext.isEmpty(this.current_operation)) {
            // <debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Calling Operations -> cancelOperation without information about current operation');
            }
            // </debug>
            return;
        }

        var me = this;

        var operationCaption = this.current_operation.operation.text;

        Ext.Msg.show({
            title: 'Отменить операцию ' + operationCaption + '?',
            msg: 'Вы уверены, что хотите отменить операцию "' + operationCaption + '"? Все изменения будут утрачены.',
            buttons: Ext.MessageBox.YESNO,
            icon: Ext.Msg.QUESTION,
            fn: function (btn) {
                if (btn == 'yes') {
                    OperationsService.cancelOperation(
                        me.current_operation.entityId,
                        me.current_operation.docId,
                        me.operationCancelled,
                        me,
                        { timeout: me.timeout }
                    );
                    
                    me.blockFields();
                }
            }
        });
    },

    /**
    * Обработчик события по окончании выполнения операции
    * @method
    * @private
    */
    operationDone: function () {
        this.current_operation = null;

        this.Buttons.operations().setVisible(true);
        this.Buttons.operation_cancel().setVisible(false);
        this.Buttons.operation_commit().setVisible(false);
        this.fireEvent('operationdone', this);
        this.refreshElement();
    },

    /**
    * Обработчик события по отмене операции
    * @method
    * @private
    */
    operationCancelled: function () {
        this.current_operation = null;

        this.Buttons.operations().setVisible(true);
        this.Buttons.operation_cancel().setVisible(false);
        this.Buttons.operation_commit().setVisible(false);

        this.fireEvent('operationcancelled', this);
        this.refreshElement();
    },

    /**
    * Обработчик вызываемый после того, как сервер вернет ответ после выполнения операции
    * @private
    */
    onExecutedOperation: function () {

        this.fireEvent('operationexecuted', this);
        this.refreshElement();
    },

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {

        Ext.apply(this, config);
        
        this.mixins.observable.constructor.call(this);

        this.addEvents(
			/**
			* @event operationdone
			* Событие возникает после завершения неатомарной операции
			*/
			'operationdone',
            
            /**
            * @event operationcancelled
            * Событие возникает после отмены неатомарной операции
            */
            'operationcancelled',
            
            /**
            * @event operationexecuted
            * Событие возникает после выполнения атомарной операции
            */
            'operationexecuted',
            
            /**
            * @event operationbegin
            * Событие возникает после начала неатомарной операции
            */
            'operationbegin'
		);
    }
});