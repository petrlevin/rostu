/**
* @class App.components.MultiEditFormElement
* @extends Ext.form.FieldContainer
* 
* Контейнер, состояший из выпадающего списка -- выбор свойства для редактирования и поля(-ей) определющих значение.
*/
Ext.define('App.components.MultiEditFormElement', {
    extend: 'Ext.form.FieldContainer',

    /**
    * @property {Ext.from.ComboBox} selectorElement
    * Элемент выбора поля для редактирования
    */
    selectorElement: null,

    /**
    * @property {Object} valueElement
    * Элемент выбора поля для редактирования
    */
    valueElement: null,

    /**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {
        var me = this;

        Ext.apply(this, config);
        this.callParent( [me.containerConfig()] );

        this.selectorElement = this.createSelectorElement();

        this.removeButton = this.createRemoveBtnElement();

        this.add(this.removeButton);
        this.add(this.selectorElement);
    },

    /*
    * Параметры для FieldContainer
    */
    containerConfig: function() {
        var me = this;
        return {
            width: '100%',
            layout: 'column',
            entity: me.entityModel,
            listeners: {
                mouseover: {
                    element: 'el',
                    fn: function () {
                        if (me.ownerDialog.editedFields.length > 1) {
                            me.removeButton.show();
                        }
                    }
                },
                mouseout:{
                    element: 'el',
                    fn: function() { me.removeButton.hide(); }
                }
            }};
    },

    createSelectorElement : function() {
        var me = this;

        return Ext.create('Ext.form.ComboBox', {
            fieldLabel: 'Поле',
            store: this.states,
            labelAlign: 'top',
            queryMode: 'local',
            displayField: 'name',
            valueField: 'id',
            columnWidth: 0.25,
            allowBlank: false,
            renderTo: Ext.getBody(),
            onChange: function () {
                var fieldId = this.getValue();

                if (me.valueElement)
                    me.remove(me.valueElement);

                me.valueElement = me.createValueElement(fieldId);
                me.add(me.valueElement);
            }
        });
    },
    
    createValueElement: function (fieldId) {
        var me = this;
        
        var fieldModel = this.entityModel.result.filter(function (f) {
            return (f.id == fieldId);
        }, this);
        if (fieldModel && fieldModel.length > 0)
            fieldModel = fieldModel[0];
        else {
            //<debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('В модели объекта отсутствует поле с Id = ' + fieldId);
            }
            //</debug>
            return;
        }

        var field = Ext.create('App.logic.factory.Field', Ext.apply({
            form_id: me.ownerCt.id,
            owner_id: me.ownerCt.id,
            entity: me.ownerDialog.entity,  // объект класса EntityItem - сущность, которой принадлежит поле
            model: me.entityModel,
            fields: me.ownerDialog.fields
        }, fieldModel));

        Ext.Array.push(me.ownerDialog.fields, field);
        
        return Ext.apply(field.getField(), { labelAlign: 'top', margin: '0 0 0 10', columnWidth: 0.7 });
    },

    createRemoveBtnElement: function () {
        var me = this;

        return Ext.create('Ext.Img',
            {
                hidden: true,
                margin: '15 0 0 0',
                hideMode: 'visibility',
                width: 33,
                columnWidth: 0.05,
                cls: 'icon-delete-multiedit',
                listeners: {
                    click: {
                        element: 'el',
                        fn: function () {
                            me.ownerDialog.removeContainer(me);
                            me.ownerDialog.createBtn.enable();
                        }
                    }
                }
            });
    }
});
