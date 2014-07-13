Ext.define('App.logic.factory.NavigationPanel', {
    constructor: function(config) {
        Ext.apply(this, config);
        
        this.entity = App.EntitiesMgr.getEntityById(this.record.get('id'));
    },

    getBasePanelConfig: function() {
        return {
            entity_id: this.entity.id,
            idOwner: null,
            iconCls: 'icon_list'
        };
    },
    
    createPanel: function(){
        //Если отчет -- открываем форму элемента, иначе форму списка
        if (this.entity.identitytype == 9) {
            App.ModelMgr.getModel(this.entity.id, null, this.onReportEntityModel, this);
        } else {
            Ext.create('App.logic.EntitiesList', this.getBasePanelConfig());
        }
    },
    
    onReportEntityModel: function (sender, model) {

        // <debug>
        if (Ext.isDefined(Ext.global.console)) {
            Ext.global.console.log('Got into callback with model');

            if (Ext.isEmpty(model)) {
                Ext.Error.raise('Model should be defined!!!');
            }
        }
        // </debug>

        if (model.readOnly) {
            var config = Ext.apply(this.getBasePanelConfig(), {
                entity: this.entity});

            this.profileList = Ext.create('App.logic.ReportProfilesList', config);
        }
        else
            Ext.create('App.logic.ReportEntityItem', {
                entity: this.entity,
                docid: null
            });

    }
});