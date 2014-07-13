Ext.define('App.logic.ReportEntityItem', {
    extend: 'App.logic.EntityItem',

    isNew: false,

    /**
    * Конструктор, который создает новый объект данного класса
    * @param {Object} Объект с конфигурацией.
    */
    constructor: function(config) {

        this.callParent([config]);

        this.on('itemloaded', this.onItemLoaded, this);
    },

    onAfterRender: function() {
        var me = this;
        if (me.docid == null) {

            CommunicationDataService.createReportItem(me.entity.id, function(result) {
                me.docid = result;
                me.onAfterRender();
            }, this);

        } else
            this.callParent([]);
    },
    
    getActions: function() {
        var result = [
            this.Buttons.printReport(),
            this.Buttons.openReportProfile({
                handler: this.openReportProfile,
                scope: this
            })
        ];

        if (!this.model.readOnly) {
            result = result.concat([
                this.Buttons.edit({
                    handler: this.onEdit,
                    scope: this,
                    hidden: false
                }),
                this.Buttons.save({
                    handler: this.saveReportProfile,
                    scope: this
                })
            ]);
        }

        result = result.concat(
            [
                this.Buttons.close({
                handler: function() { this.close(); },
                scope: this
                })
            ]
        );

        return result;
    },

    onItemLoaded : function() {

        var isTemporary = this.getField('isTemporary').getValue();
        // При нажатии кнопки Печать происходит сохранения элемента. 
        // Чтобы предотвратить нежелательные изменения профиля он всегда открывается в режиме readonly.
        this.setDisabled(!isTemporary);
        
        var btnSave = Ext.getCmp(this.id + '-button-save');
        if (!Ext.isEmpty(btnSave) )
            btnSave.setDisabled(!isTemporary);

        var btnEdit = Ext.getCmp(this.id + '-button-edit');
        if (!Ext.isEmpty(btnEdit))
            btnEdit.setDisabled(this.elementData.readOnly);
    },

    onEdit : function() {
        this.setDisabled(false);
        Ext.getCmp(this.id + '-button-save').setDisabled(false);
    },

    saveReportProfile : function() {
        this.getField('isTemporary').setValue(false);
        this.on('aftersave', function() {
            this.setDisabled(true);
        }, this, { single: true });
        this.saveElement();
    },

    openReportProfile: function() {
        if (Ext.isDefined(this.profileList)) {
            this.profileList.show();
            return;
        } 
            
        var listId = Ext.id();

        var isSelectList = (this.docid !== null);

        var config = {
                id: listId,
                selection_form: isSelectList,
                entity: this.entity,
                entity_id: this.entity.id,
                openAsWindow: isSelectList,
                iconCls: isSelectList ? 'icon_selection' : 'icon_list',
                modal: isSelectList,
                formType: isSelectList ? 3 : 2,
                reportItem: isSelectList ? this : undefined
            };
            
        this.profileList = Ext.create('App.logic.ReportProfilesList', config);
    }
});