/**
* @class App.logic.ReportProfilesList
* Список профилей отчета.
* 
* По сути список сохраненных элементов отчета. Т.е. можно было назвать и просто ReportEntitiesList 
*/
Ext.define("App.logic.ReportProfilesList", {
    extend: 'App.logic.EntitiesList',
    
    direct_function: 'ReportProfilesService.getReportProfiles',

    onReportProfileSelect: function () {
        var value = this.getValue();
        
        if (Ext.isDefined(value.id)) {
            if (Ext.isDefined(this.reportItem)) {
                this.reportItem.docid = value.id;
                this.reportItem.refreshElement();
            } else {
                Ext.create('App.logic.ReportEntityItem', {
                    entity: this.entity,
                    docid: value.id
                });
            }
        }

        (this.grid.up('window') || this.grid.up('panel')).destroy();
    },

    getCustomConfig: function () {
        var me = this;
        return {
            preventCreateBtn: true,
            preventOpenBtn: true,
            customGrid: {
                extendedActions: [
                    Ext.create('Ext.Action', {
                        id: me.id + '-button-select',
                        text: Locale.APP_BUTTON_SELECT,
                        handler: me.onReportProfileSelect,
                        scope: me
                    })
                ],
                listeners: {
                    scope: me,
                    itemdblclick: me.onReportProfileSelect
                }
            },

            customWindow: {
                title: me.entity.caption + ': Выбор профиля'
            }
        };
    },
    
    constructor: function (config) {
        this.id = Ext.id();
        this.entity = config.entity;
        
        Ext.apply(config, this.getCustomConfig());

        this.callParent([config]);
    }
});