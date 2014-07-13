Ext.define('App.events.XmlExport', {
    extend: 'App.events.CommonItem',

    events: [
        {
             name: 'afterrender',
             handler: 'onAfterRender',
             item:null
        }
    ],


    

    onAfterRender : function() {
        this.addNewButton(this.getForm().panel.getDockedItems('toolbar[dock="bottom"]')[0], {
            text: 'Выполнить',
            listeners: {
                scope: this,
                click: function () {
                    // By default, "this" will be the object that fired the event.
                    var downloader = Ext.create('App.logic.HttpRequest', {
                        url: 'Services/ExportXml.ashx',
                        params: {
                            templateExportId: this.getField("idTemplateExport").getValue(),
                            templateImportId: this.getField("idTemplateImport").getValue()
                        }
                    });
                    downloader.submit();
                }
            }
        }, 0);
    }
   
   

    

});