Ext.define("App.components.mixins.RegistryInfo", {

    uses: [
        'App.logic.EntitiesList'
    ],

    requires: [
        'Ext.menu.Item'
    ],

    constructor: function(config) {

        Ext.apply(this, config);
    },
    getActions: function() {
        return (this.entity.identitytype == 6 /*Документ*/ || this.entity.identitytype == 7 /*Инструмент*/) ?
            [
                this.Buttons.registryinfo()
            ] : [];
    },

    hasRegistryInfo: function(data) {

        return (this.entity.identitytype == 6 /*Документ*/ || this.entity.identitytype == 7 /*Инструмент*/ ) && !Ext.isEmpty(data.registryRecords);
    },

    update: function (data) {
        
        var split = this.Buttons.registryinfo();
        if (Ext.isEmpty(data) || !this.hasRegistryInfo(data)) {
            if (split)
                split.setVisible(false);
            return;
        }
        
        if (split) {
            split.menu.removeAll();
            Ext.each(data.registryRecords, function(item) {
                split.menu.add(Ext.create('Ext.menu.Item', {
                    text: item.caption + " (" + item.count + ")",
                    handler: function() {
                        this.getThoroughRegistryInfo(this.entity.id, this.docid, item.id);
                    }.bind(this)
                }));
            },this);
            split.setVisible(true);

        }
    },

    getThoroughRegistryInfo: function(entityId , docId , regEntityId) {
        //alert(entityId);
        var dialog = Ext.create('App.logic.EntitiesList', {
            direct_function: 'DataService.regInfoGridSource',
            entity_id: regEntityId,
            iconCls: 'icon_selection',
            openAsWindow: true
        });

        dialog.on('beforerequest', function (sender, proxy) {

            proxy.setExtraParam('filterValueId', docId);
            proxy.setExtraParam('filterEntityId', entityId);
        }, this);

    }
})
    

