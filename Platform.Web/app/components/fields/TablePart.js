/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.components.fields.TablePart
* @extends Ext.panel.Panel
* Табличная часть
*/

Ext.define('App.components.fields.TablePart', {
    extend: 'App.components.fields.TableField',
    alias: 'widget.app_tablepart',

    uses: [
        'App.logic.EntitiesList',
        'App.components.Maximize'
    ],

    requires: [
        'App.components.mixins.CommonField'
    ],
    
    mixins: {
        commonfield: 'App.components.mixins.CommonField'
    },

    ownerfield: null,

    parentEntityId : null,

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
    constructor: function (config) {

        config.id = config.id || Ext.id();
        Ext.apply(this, config);

        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('config.owner_id:' + config.owner_id);
        }
        //</debug>
        

        this.list = Ext.create('App.logic.EntitiesList', {
            preventLoad:true,
            field_id: config.id,
            parent_id: config.owner_id,
            hasAggregates: this.entity.formCfg && this.entity.formCfg.tableFieldAggregates[this.name],
            autoShow: false,
            columnNames: config.columnNames,
            model_id: Ext.String.format('tp-{0}-{1}', config.initialModel.identity, config.initialModel.id),
            entity_id: config.initialModel.identity,
            getDefaults: this.getDefaults.bind(this),
            refresh: this.refresh.bind(this),
            getTools: function() {
                return [
                    Ext.create('App.components.Maximize', {
                        parentForm: this
                    })
                ]
            },
            getVersioning: function () {
                return false;
            }
        });
        this.grid = this.list.getConfig();
        this.grid.on('afterrender', this.initWatch, this);
        this.grid.on('afterrender', function() { this.list.owner_form_id = this.getOwnerFormId(); }, this);

        Ext.each(this.list.model.result, function(field) {
            if (field.id === config.initialModel.idownerfield) {
                this.ownerfield = field;
                return false;
            }
        }, this);
        // Check if OwnerField is empty
        if (this.ownerfield === null) {
            Ext.Error.raise(Ext.String.format('У табличной части обязательно наличие поля OwnerField!'));
        }

        config = Ext.apply(config, {
            border: false,
            height: 320,
            layout: 'fit',
            labelAlign: 'top',
            items: [
                this.grid
            ]
        });

        this.callParent([config]);
    },

    refresh: function (forced, pageNumber) {
        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log(Ext.String.format('Обновление табличной части, поле {0}', this.name));
        }
        // </debug>

        this.list.refreshList([
            { name: 'fieldid', value: this.initialModel.id },
            { name: 'docid', value: this.docid },
            { name: 'ownerfieldName', value: this.ownerfield.name },
            { name: 'fieldvalues', value: this.getDependencies() }
        ], forced, pageNumber);
    },

    getDefaults: function () {
        var result = {};

        result[this.ownerfield.name.toLowerCase()] = this.docid;
        result[this.ownerfield.name.toLowerCase() + '_caption'] =
            this.entity.elementData.result[0][this.model.captionField];

        return result;
    }
});
