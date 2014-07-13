/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.Link
* @extends Ext.form.field.Trigger
* @mixins App.components.mixins.FormField
* Ссылка на справочник
*/

Ext.define('App.components.fields.Link', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.app_linkfield',

    uses: [
        'App.logic.EntitiesList',
        'App.logic.EntityItem'
    ],

    requires: [
        'App.components.mixins.LinkField',
        'App.components.mixins.FormField',
        'App.components.mixins.CommonField',
        'Ext.window.MessageBox',
        'Ext.Action'
    ],

	/**
	* Используемые данным объектом mixins
	* @private
	*/
	mixins: {
	    linkfield: 'App.components.mixins.LinkField',
	    formfield: 'App.components.mixins.FormField',
	    commonfield: 'App.components.mixins.CommonField'
	},

    /**
    * 
    */
    trigger1Cls: 'icon-trigger-dots',

    /**
    *
    */
    trigger2Cls: 'x-form-search-trigger',

    /**
    *
    */
    trigger3Cls: 'x-form-clear-trigger',

    /**
    * @cfg {Boolean} editable We don't want user to change the value inside this field by hands
    */
    editable: false,

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		//<debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Dictionary link field constructor');
		}
		//</debug> 
		Ext.apply(this, config);
		this.callParent([ config ]);
	},

	setReadOnly: function (state) {

	    var action = function () {
	        this.triggerEl.each(function(trigger, composite, index) {
	            if (index !== 1) {
	                if (state) {
	                    trigger.hide();
	                    trigger.el.parent().setWidth(0);
	                } else {
	                    var width = this.triggerEl.item(1).getWidth();
	                    trigger.show();
	                    trigger.el.parent().setWidth(width);
	                }
	            }
	        }, this);	        
	    };

	    if (this.rendered)
	        action.call(this);
	    else
	        this.on('render', action, this, { single: true });

        this[state ? 'addCls' : 'removeCls'](this.readOnlyCls);
	    //this.callParent(arguments);
	},
    
    onSelectItem: function (sender) {

        if (this.list.checkIsSelectable() == 0)
            return;
        
        var window = sender.up('window');
        
        if (window) {
            var sm = this.list.grid.getSelectionModel();
            if (sm && sm.hasSelection()) {

                this.setValue(this.list.getValue());
                window.destroy();
            } else {
                Ext.MessageBox.show({
                    title: Locale.APP_MESSAGE_FORM_WARNING,
                    msg: Locale.APP_MESSAGE_SHOULD_BE_SELECTED,
                    width: 300,
                    buttons: Ext.MessageBox.OK,
                    icon: Ext.MessageBox.WARNING
                });
            }
        } else {
            //<debug>
            if (Ext.isDefined(Ext.global.console)) {
                Ext.global.console.log('Dictionary link field > select item');
                Ext.Exception.raise('Could not find window!');
            }
            //</debug>
        }
    },

    setValue: function (value, caption, description) {

        if (Ext.isObject(value)) {
            this.fieldValue = value;
            this.callParent([value.caption]);
        } else {
            this.fieldValue = {
                id: value,
                caption: caption,
                description: description
            };
            this.callParent([caption]);
        }

        if (this.fieldValue && this.fieldValue.description) {
            Ext.create('Ext.tip.ToolTip', {
                target: this.inputEl,
                html: this.fieldValue.description + ' &nbsp;'
            });
        }
    },

    getValue: function () {

        return this.fieldValue.id;
    },

    /**
    *
    */
    onTrigger1Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 1, select');

            Ext.global.console.log('Dictionary link field > getOwner');
            Ext.global.console.log(this.getOwner());
        }
        //</debug>
        this.up('window') || this.up('panel');

        var idowner = null;

        //todo: слишком странно. надо прокидывать id документа более простым путем.
        var entity = this.entity;
        if (entity && entity.entity.identitytype == 4 /*ТЧ*/) {
            //Если редактируем грид или используем множетсвенное редактирование
            var parent = entity.getParent();

            //Если редактируем элемент ТЧ
            if (parent.entity.identitytype == 4)
                parent = parent.getParent();

            idowner = parent.docid;
        }
        
            
        
        if (Ext.isDefined(this.list)) {
//            this.list.parent_id = this.owner_id;
            this.list.show();
        } else {
            var listId = Ext.id();
            var config = {
                id: listId,
                parent_id: this.owner_id,
                owner_form_id: this.getOwnerFormId(),
                selection_form: true,
                field_id: this.id,
                entity_id: this.initialModel.identitylink,
                initialModel: this.initialModel,
                columnNames: this.columnNames,
                openAsWindow: true,
                iconCls: 'icon_selection',
                modal: true,
                ownerdocid: idowner,
                formType: 3, // ToDo{CORE-280} тип формы: форма выбора
                customGrid: {
                    flags: {
                        preventDeleteBtn: true
                    },
                    extendedActions: [
                        Ext.create('Ext.Action', {
                            id: listId + '-button-select',
                            text: Locale.APP_BUTTON_SELECT,
                            handler: this.onSelectItem,
                            scope: this
                        })
                    ],
                    listeners: {
                        scope: this,
                        itemdblclick: this.onSelectItem,
                        selectionchange: this.onSelectionChange
                    }
                },
                customWindow: {
                    maximized: false
                }
            };
            Ext.apply(config, {
                getParent: App.components.mixins.CommonField.prototype.getParent,
                getDependencies: App.components.mixins.CommonField.prototype.getDependencies,
                getDependencyValue: App.components.mixins.CommonField.prototype.getDependencyValue,
                getVersioning: function () {
                    return false;
                }
            });
            this.list = Ext.create('App.logic.EntitiesList', config);
        }
    },

    /**
    *
    */
    onTrigger2Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 2, open');
        }
        //</debug>
        if (this.getValue() === null)
            return;
        
        var entity = App.EntitiesMgr.getEntityById(this.initialModel.identitylink);
        Ext.create('App.logic.EntityItem', {
            entity: entity,
            docid: this.fieldValue.id
        });
    },

    /**
    *
    */
    onTrigger3Click: function () {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Dictionary link field > trigger 3, clear');
        }
        //</debug>
        this.clear();
    },

    /**
    * @private
    *
    */
    onSelectionChange: function (sender, selected, eOpts) {

        this.list.checkIsSelectable();
    }
});
