

/*
Диалог множественного редактирования
*/
Ext.define('App.components.MultiEditDialog', {

    extend: 'Ext.form.Panel',

    requires: [
        'App.components.MultiEditFormElement',
		'Ext.toolbar.Toolbar',
		'Ext.window.MessageBox',
		'Ext.form.field.Text',
		'Ext.form.field.File',
        'Ext.form.Panel',
		'Ext.Button'
    ],

    /*
    * @property {Array} fields
    */
    fields: [],

    /*
    * @property {Object} entity
    */
    entity: null,

    /*
    * @property {Object} states
    */
    states: null,

    /*
    * @property {Object} entityModel
    */
    entityModel: null,

    /*
    * Группы редактирования, присутствующие на форме
    */
    editedFields: [],

    /**
    * @property {Ext.button.Button} createBtn
    * Кнопка добаления новой группы редактирования
    */
    createBtn: null,

    constructor: function (config) {
        Ext.apply(this, config);

        if (!this.componentId) {
            return;
        }

        this.fields = [];
        this.editedFields = [];
        this.entity = Ext.getCmp(this.componentId);
        this.entityModel = this.entity.listModel || [];

        this.initStore();

        if (!this.states.data.length) {
            Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MULTIEDIT_FORM_NOFIELDS);
            return;
        }

        this.init();

        this.callParent([this.panelMainConfig()]);
    },

    /*
   * Инициализируем список редактируемые поля
   */
    initStore: function () {
        var me = this;

        this.states = Ext.create('Ext.data.Store', {
            fields: ['id', 'name'],
            data: []
        });

        this.entityModel.result.filter(function (field) {
            return me.isEditableField(field);
        }, this).forEach(
                    function (field) {
                        me.states.add({ id: field.id, name: field.caption });
                    });
    },

    /*
    * Инициализируем начальное состояние формы
    */
    init: function () {
        var me = this;

        var selectionContainer = this.createContainer();

        this.commonContainer = Ext.create('Ext.form.FieldContainer',
                {
                    layout: 'vbox',
                    items: selectionContainer
                });

        this.createBtn = Ext.create('Ext.button.Button', {
            xtype: 'button',
            iconCls: 'icon-create',
            text: 'Добавить поле для редактирования',
            handler: function () {
                var newContainer = me.createContainer();
                me.commonContainer.add(newContainer);

                if (me.editedFields.length == me.states.data.length)
                    this.disable();
            }
        });

        if (this.states.data.length <= 1)
            this.createBtn.disable();
    },

    panelMainConfig: function () {
        var me = this;

        return {
            xtype: 'form',
            layout: 'anchor',
            bodyPadding: 5,
            defaults: { anchor: '100%' },
            defaultType: 'textfield',
            items: [me.commonContainer],

            // Панель инструментов
            dockedItems: [ me.getTopToolbarConfig(), me.getButtonToolbarConfig() ]
        };
    },

    /*
    * Верхняя панель. Содержит кнопку 'добавить поле'
    */
    getTopToolbarConfig: function () {
        var me = this;
        return {
            xtype: 'toolbar',
            dock: 'top',
            items: [me.createBtn]
        };
    },

    /*
    * Нижняя панель. 
    */
    getButtonToolbarConfig: function() {
        var me = this;
        return {
            xtype: 'toolbar',
            dock: 'bottom',
            items: [
                { xtype: 'tbfill' },
                {
                    xtype: 'button',
                    text: 'Сохранить',
                    handler: function() {
                        var form = this.up('form').getForm();
                        if (form.isValid()) {
                            me.submit();
                        } else {
                            Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MESSAGE_FORM_INVALID);
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: 'Отмена',
                    handler: function() {
                        this.up('window').close();
                    }
                }
            ]
        };
    },

    /*
    * Добавить на форму новую группу редактирования.
    */
    createContainer: function () {
        var me = this;
        var selectionContainer = Ext.create('App.components.MultiEditFormElement',
               {
                   entityModel: me.entityModel,
                   ownerDialog: me,
                   states: me.states
               });

        Ext.Array.push(this.editedFields, selectionContainer);

        return selectionContainer;
    },

    /*
    * Удалить с формы группу редактирования
    */
    removeContainer: function (removed) {
        var me = this;
        Ext.Array.remove(me.editedFields, removed);
        me.commonContainer.remove(removed);
    },

    isEditableField: function (field) {
        return !field.readonly &&
                        field.identityfieldtype != 8 &&
                        field.identityfieldtype != 9 &&
                        field.identityfieldtype != 13 &&
                        field.identityfieldtype != 18 &&
                        !field.idcalculatedfieldtype &&
                        Ext.isEmpty(field.dependencies) &&
                        !this.isStatusField(field) &&
                        !this.isEntitySelector(field);
    },

    isStatusField: function (field) {
        return (this.entity.entity.identitytype === App.enums.EntityType.Reference
            && field.identityfieldtype === App.enums.EntityFieldType.Link
            && field.identitylink === App.common.Ids.RefStatus);
    },

    /**
     * Системное поле выбора сущности, имя которого заканчивается на 'Entity'
     * @param {Object/App.logic.factory.Field} fieldCfg конфигурация поля сущности. 
     */
    isEntitySelector: function (field) {
        return field.identityfieldtype == App.enums.EntityFieldType.Link
            && field.issystem === true
            && field.name.endsWith('Entity');
    },

    /*
    * Проверка валидности и отправка запроса на сохранение
    */
    submit: function () {
        var me = this;
        var idEntity = this.entity.model.entityId;

        var result = {};

        var existedFieldIds = [];

        var isValid = true;

        Ext.each(this.editedFields, function (container) {
            var fieldId = container.selectorElement.value;
            var valueFieldName = container.valueElement.name;

            if (Ext.Array.contains(existedFieldIds, fieldId)) {
                Ext.MessageBox.alert(Locale.APP_MESSAGE_FORM_WARNING, Locale.APP_MULTIEDIT_FORM_MULTIFIELDS);
                isValid = false;
                return;
            }

            Ext.Array.push(existedFieldIds, fieldId);

            var value = container.valueElement.get();
            if (!value || value[valueFieldName] === undefined ) {
                value = {};
                value[valueFieldName] = null;
            }
            
            Ext.apply(result, value);
        });

        if (!isValid)
            return;

        CommunicationDataService.updateElements(idEntity, this.selectedElements, result, function (data, response) {
            me.entity.refresh();

            if (response.type !== 'rpc') {
                // <debug>
                if (Ext.isDefined(Ext.global.console)) {
                    Ext.global.console.log('Произошла ошибка при изменении элементов');
                }
                // </debug>
                return;
            }

            me.up('window').close();
        }, this);
    }
})