/**
 * Поле типа "Общая ссылка".
 */
Ext.define('App.components.fields.GenericLink', {
    extend: 'Ext.form.FieldContainer',
    alias: 'widget.app_genericlinkfield',
    
    uses: [
        'App.components.fields.Link'
    ],
    
    layout: 'hbox',

    constructor: function(config) {

        /**
         * @cfg {Object} entitySelector
         */

        /**
         * @cfg {Object} valueSelector
         */

        Ext.apply(this, config.valueSelector);
        Ext.apply(this, {
            name: config.valueSelector.name + '_container',
            items: []
        });

        this.saveConfig = config;
        this.callParent();
    },
    
    initComponent: function() {
        this.callParent(arguments);
        this.onBeforeShow();
    },

    onBeforeShow:function(){

        this.createEntitySelector(this.saveConfig);
        this.createValueSelector(this.saveConfig);
        this.add([
            this.entitySelector,
            { xtype: 'splitter' },
            this.valueSelector
        ]);
        this.entitySelector.on('change', this.setupValueSelector, this);
    },
    
    setupValueSelector: function () {

        var entityId = this.entitySelector.getValue();

        this.valueSelector.clear();
        this.valueSelector.initialModel.identitylink = entityId;
        this.valueSelector.list = undefined;

        if (entityId) {
            this.valueSelector.enable();
        } else {
            this.valueSelector.disable();
        }
    },
    
    createEntitySelector: function (config) {

        this.entitySelector = this.createInternalField(config.entitySelector);
    },
    
    createValueSelector: function (config) {

        this.valueSelector = this.createInternalField(config.valueSelector);
        this.valueSelector.disable(); // начальное состояние
    },
    
    createInternalField: function (config) {

        var defaults = {
            hideLabel: true,
            flex: 1
        };
        return Ext.create('App.components.fields.Link', Ext.apply(config, defaults));
    },
    
    get: function() {
        var result = {};

        result[this.entitySelector.name] = this.entitySelector.getValue();
        result[this.valueSelector.name] = this.valueSelector.getValue();
        
        return result;

    }
});