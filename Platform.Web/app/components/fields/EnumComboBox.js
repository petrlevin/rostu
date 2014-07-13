/**
* Выпадающий список для перечислений. 
* Особенность его в том, что все элементы перечисления загружаются в хранилище данного поля.
* Загрузка элементов происходит один раз при первом разворачивании выпадающего списка.
*/
Ext.define('App.components.fields.EnumComboBox', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.app_enumcombofield',
    
    requires: [
        'App.components.mixins.LinkField',
        'App.components.mixins.FormField',
        'App.components.mixins.CommonField',
        'Ext.data.DirectStore'
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
    * Дополнительная кнопка "X" для очистки поля.
    */
    trigger2Cls: 'x-form-clear-trigger',

    queryMode: 'local',
    displayField: 'caption',
    valueField: 'id',

    /**
     * Признак того, что в хранилище загружены элементы перечисления. 
     * В данном компоненте загрузка происходит только единожды, в момент раскрытия выпадающего списка.
     */
    isLoaded: false,

    /**
    * Конструктор, который создает новый объект данного класса
    * @param {Object} Объект с конфигурацией.
    */
    constructor: function(config) {
        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('App.components.fields.ComboBox constructor');
        }
        //</debug> 
        Ext.apply(this, config);
        this.getDependencies = App.components.mixins.CommonField.prototype.getDependencies;

        // The data store containing the list of states
        this.store = this.createStore();

        this.callParent([config]);
    },

    initComponent: function() {
        this.callParent(arguments);
        this.on('beforequery', this.onBeforeQuery, this);
    },

    onTrigger2Click: function(args) {

        this.clear();
    },

    onBeforeQuery: function() {
        if (!this.isLoaded && !this.store.isLoading()) {
            this.store.load();
        }
    },

    createStore: function() {
        var me = this;

        return Ext.create('Ext.data.DirectStore', {
            fields: ['id', 'caption'],
            autoLoad: false,
            remoteSort: true,
            proxy: {
                type: 'direct',
                directFn: DataService.gridSource,
                reader: {
                    root: 'rows',
                    totalProperty: 'count'
                },
                filterParam: undefined,
                pageParam: undefined,
                startParam: undefined,
                limitParam: undefined,
                extraParams: {
                    entityId: this.initialModel.identitylink,
                    fieldid: this.initialModel.id
                }
            },
            listeners: {
                'load': function() {
                    me.isLoaded = true;
                },
                'beforeload': {
                    fn: this.onBeforeLoad,
                    scope: me
                }
            }
        });
    },
    
    onBeforeLoad: function () {
        this.getStore().getProxy().setExtraParam('fieldvalues', this.getDependencies());
        return true;
    },

    setValue: function (value, doSelect) {

        if (Ext.isArray(value) && value.length == 1) {
            // => выбрали элемент в интерфейсе
            this.onBeforeQuery();

            this.fieldValue = {
                id: value[0].get('id'),
                caption: value[0].get('caption')
            };
            this.callParent([value, doSelect]);
        } else if (Ext.isObject(value) && value.hasOwnProperty('id') && value.hasOwnProperty('caption')) {

            this.fieldValue = value;
            this.callParent([this.fieldValue.caption]);
        } else if (Ext.isNumber(value) && Ext.isString(doSelect)) {
            
            this.fieldValue = {
                id: value,
                caption: doSelect
            };
            this.callParent([this.fieldValue.caption]);
        } else if (!Ext.isDefined(value)) {

            this.fieldValue = { id: null, caption: '' };
            this.callParent([this.fieldValue.caption]);
        } else {

            this.callParent(arguments);
        }
    },

    getValue: function () {

        return this.fieldValue.id;
    }    
});