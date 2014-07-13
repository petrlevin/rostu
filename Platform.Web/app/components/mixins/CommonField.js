/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.mixins.CommonField
*/
Ext.define("App.components.mixins.CommonField", {

    /**
    * Признак того, что это поле формы
    * @type {Boolean} [isFormField=true]
    */
    isFormField: true,

    constructor: function (config) {
	//<debug>
	if (Ext.isDefined(Ext.global.console)) {
		Ext.global.console.log('App.components.mixins.CommonField');
		Ext.global.console.dir(this);
	}
	//</debug>
    },

    getParent: function () {
        var me = this;

        return Ext.getCmp(me.parent_id) || me.entity;
    },

    /**
    * Функция возвращает форму-владельца 
    * @return {Ext.Base} Форма-владелец поля
    * ЭТО НЕПРАВДА
    */
    getOwner: function () {
        var me = this;
        // <debug>
        if (Ext.isEmpty(me.owner_id)) {
            Ext.Error.raise('Owner for field is not defined in call!');
        };
        // </debug>
        return Ext.getCmp(me.owner_id);
    },

    /**
    * Функция возвращает форму-владельца 
    * ЭТО ПРАВДА
    */
    getOwnerForm: function () {
        return this.up('window') || this.up('panel[isTop=true]');
    },

    getOwnerFormId: function() {
        var f = this.getOwnerForm();
        return f ? f.id : null;
    },

    getDependencyValue: function () {
        var temp = null, me = this;

        if (me.getValue) {
            temp = me.getValue();
        } else {
            temp = me.list.getValue();
        }
        if (temp && Ext.isDefined(temp.id)) {
            temp = temp.id;
        }
        return temp || null;
    },

    hasDependencies: function() {

        return !Ext.isEmpty(this.initialModel.dependencies);
    },

    getDependentFields: function() {
        var me= this,
            deps = me.initialModel.dependencies,
            parent = me.getParent(),
            result = [];

        if (!Ext.isEmpty(deps)) {
            Ext.each(deps, function(name) {
                var field = parent.getField(name);
                if (field) {
                    result.push(field);
                }
            });
        }

        return result;
    },

    getDependencies: function () {
        var me = this,
            deps = me.initialModel.dependencies,
            parent = me.getParent(),
            result = {};

        if (this.hasDependencies()) {
            Ext.each(deps, function(name) {
                var field = parent.getField(name);
                if (field) {
                    var temp = field.getDependencyValue();
                    result[field.initialModel.id] = temp;
                }
            });
        }

        return result;
    },
    
    /**
     * Определяет, является ли поле табличным.
     */
    isTableField: function () {
        return [
			App.enums.EntityFieldType.Multilink,
			App.enums.EntityFieldType.Tablepart,
			App.enums.EntityFieldType.VirtualTablePart
        ].indexOf(this.initialModel.identityfieldtype) >= 0;
    },
    
    /**
     * Разрешено ли полю быть доступным для редактирования?
     */
    canEnable: function () {
        var result = true;
        var form = this.getParent();
        if (form.isNew === false) {
            if (form.fieldFactory.statusField) {
                result = !form.checkRefStatus();
            } else if (form.hasOperations(form.elementData)) {
                result = !Ext.isEmpty(form.elementData.currentOperationId) 
                    && Ext.isArray(form.elementData.editableFields)
                    && form.elementData.editableFields.indexOf(this.name) > -1;
            }
        }
        // для табличных полей проверяем - выделен ли элемент родительской ТЧ
        if (form.independentGridsLoaded === true && this.isTableField()) {
            Ext.each(this.getDependentFields(), function (masterField) {
                if (masterField.isTableField()) {
                    result = result && masterField.grid.getSelectionModel().hasSelection();
                }
            }, this);
        }
        return result;
    }
})
