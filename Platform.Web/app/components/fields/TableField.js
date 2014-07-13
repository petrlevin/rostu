/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.TableField
* @extends Ext.form.FieldContainer
* Class-parent for table part fields
*/
Ext.define('App.components.fields.TableField', {
    extend: 'Ext.form.FieldContainer',
    alias: 'widget.app_tablefield',
    
    initWatch: function() {
        var fields = this.getDependentFields();

        Ext.each(fields, function(field) {
        	if (field && Ext.isFunction(field.bindTo)) {
        	    //<debug>
        	    if (Ext.isDefined(Ext.global.console)) {
        	        Ext.global.console.log(Ext.String.format('Привязка к полю :{0}->{1}', this.name, field.name));
        	    }
        	    //</debug>
        	    field.bindTo(this.onParentSelectionChanged/*, this.onParentLoaded*/, this);
        	    //this.refresh();
        	}
        }, this);
    },

    canRefresh: function(){
        var fields = this.getDependentFields(), sm, result = true;

        Ext.each(fields, function (field) {
            var class_name = Ext.getClassName(field);
            if (class_name === 'App.components.fields.Multilink' ||
                class_name === 'App.components.fields.TablePart') {

                //<debug>
                if (Ext.isDefined(Ext.global.console)) {
                    Ext.global.console.log(Ext.String.format('Имеется связь с полем : {0}', field.name));
                }
                //</debug>
                sm = field.grid.getSelectionModel();
                result = sm.hasSelection();
            }
        }, this);

        // Можно обновлять только если нет связанных полей
        return result;
    },

    bindTo: function (onSelectionChangeCb/*, onLoadCb*/, childTableField) {

        var parentSm = this.grid.getSelectionModel();
        if (this.grid.getSelectionModel().getCount() > 0) {
            // если при привязке обнаруживается, что родительский грид уже имеет выделенную строку, то вызываем обработчик события выделения строки
            onSelectionChangeCb.call(childTableField, parentSm, parentSm.getSelection());
        }
        this.grid.on('selectionchange', onSelectionChangeCb, childTableField);
        //this.grid.getStore().on('load', onLoadCb, scope);
    },

    //onParentLoaded: function () {

        //this.refresh(); // Зачем это? Если обновление дочерних ТЧ инициируется при смене позиции в родительской.
    //},

    onParentSelectionChanged: function (sm, selected, eOpts) {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log(Ext.String.format('TableField::onBindEvent > Выбрано записей: {0}', selected.length));
        }
        // </debug>
        if (selected.length > 0) {
            this.refresh(true, 1);
            if (this.disabled === true) {
                this.enable();
            }
        } else {
            this.grid.getStore().removeAll();
            this.disable();
        }
    },

    getDependentGridValues: function() {
        var fields = this.getDependentFields(),
            result = new Ext.util.MixedCollection();

        Ext.each(fields, function (field) {
            var class_name = Ext.getClassName(field);
            if (class_name === 'App.components.fields.TablePart') {

                result.add(field.initialModel.identitylink,
                    field.getFullValue());
            }
            if (class_name === 'App.components.fields.Multilink') {

                result.add(field.list.model.entityId,
                    field.getFullValue());
            }
        }, this);

        // Можно обновлять только если нет связанных полей
        return result;
    },

    getFullValue: function () {
        var result = {}, sm, record;

        sm = this.grid.getSelectionModel();
        if (!Ext.isEmpty(sm) && sm.hasSelection()) {
            record = sm.getSelection()[0];

            var captionFieldModel = this.list.model.result.filter(function (field) {
                return field.name.toLowerCase() == this.list.model.captionField;
            }, this)[0];
            var captionFieldName = this.list.model.captionField + (
                captionFieldModel && captionFieldModel.identityfieldtype == App.enums.EntityFieldType.Link
                    ? '_caption'
                    : ''
            );

            result = {
                'id': record.get('id'),
                'caption': record.get(captionFieldName)
            };

            // <debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log(Ext.String.format('Значение поля ({0}) ТЧ, :"{1}"', this.name, result.id));
            }
            // </debug>
        } else {
            result = {
                'id': null,
                'caption': null
            };
        }

        return result;
    },

    getValue: function () {
        var sm, result = {};

        var sm = this.grid.getSelectionModel();
        if (!Ext.isEmpty(sm) && sm.hasSelection()) {
            result = sm.getSelection()[0].get('id');

            //<debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log(Ext.String.format('Значение поля ({0}) ТЧ, :"{1}"', this.name, result));
            }
            //</debug>
        }

        return result;
    },

    getTopToolbar: function () {

        return this.grid.getDockedItems('toolbar[dock="top"]')[0];
    },

    disable: function () {

        this.disabled = true;
        //this.callParent(arguments);
        this.setDisabledToolbar(true);
    },

    enable: function () {

        if (!this.canEnable())
            return;
        this.disabled = false;
        this.callParent(arguments);
        this.setDisabledToolbar(false);
    },

    setDisabledToolbar: function (disable) {
        var me = this;
        var tb = this.getTopToolbar();
        var entityItem = this.getParent();
        tb.items.each(function (item) {
            me.setDisabledButton(entityItem, item, disable);
        }, this);
    },

    setDisabledButton: function (entityItem, item, disable) {
        var me = this;
        if (Ext.isFunction(item.setAvailability)) {
            item.setAvailability(entityItem);
            
            if (item.menu) {
                item.menu.items.each(function (subItem) {
                    me.setDisabledButton(entityItem, subItem, disable);
                }, this);
            }
        } else {
            item.setDisabled(disable);
        }
    },

        /**
     * Зависит ли данное табличное поле от другого (других) табличных полей.
     * Используется при определении связей между табличными полями.
     * Табличное поле (мультиссылка) может зависеть от скалярного поля и тогда недостаточно проверить hasDependencies.
     */
    isDependsOnTablefield: function () {
        if (!this.hasDependencies())
            return false;
        var result = true;

        Ext.each(this.getDependentFields(), function (field) {
            if (!(field instanceof App.components.fields.TableField)) {
                result = false;
                return;
            }
        }, this);
        return result;
    },

    set: function(values) {
        var temp = values.result[0];
        this.docid = temp.id;
    },

    isValid: function() {

        return true;
    },

    isDirty: function() {

        return false;
    },

    validate: function () {
        return true;
    }
});