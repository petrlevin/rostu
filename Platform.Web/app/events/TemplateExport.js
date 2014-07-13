Ext.define('App.events.TemplateExport', {
	extend: 'App.events.CommonItem',

	events: [{
        name: 'dataget',
        handler: 'onDataGet',
        item: null
    }, {
        name: 'change',
        handler: 'onSelectionTypeChange',
        item:'idSelectionType'
    }, {
        name: 'change',
        handler: 'onLinkedSelectionTypeChange',
        item: 'idLinkedSelectionType'
    }, {
        name: 'afterrender',
        handler: 'onAfterRender',
        item:null
    }],

    onDataGet: function (panel,data) {

        this.getField('EntitiesSql').setVisible((data.idselectiontype == App.enums.SelectionType.EntitiesSql) ||
            (data.idselectiontype == App.enums.SelectionType.EntitiesItemsSql));
        this.getField('tpEntities').setVisible(data.idselectiontype == App.enums.SelectionType.Entities);
        this.getField('LinkedEntitiesSql').setVisible((data.idlinkedselectiontype == App.enums.SelectionType.EntitiesSql) ||
            (data.idlinkedselectiontype == App.enums.SelectionType.EntitiesItemsSql));        
        this.getField('mlLinkedEntities').setVisible(data.idlinkedselectiontype == App.enums.SelectionType.Entities);
    },
    
    onSelectionTypeChange: function (sender, value) {

        this.getField('EntitiesSql').setVisible((value == App.enums.SelectionType.EntitiesSql) ||
            (value == App.enums.SelectionType.EntitiesItemsSql));

        this.getField('tpEntities').setVisible(value == App.enums.SelectionType.Entities);
    },
    
    onLinkedSelectionTypeChange: function (sender, value) {

        this.getField('LinkedEntitiesSql').setVisible((value == App.enums.SelectionType.EntitiesSql) ||
            (value == App.enums.SelectionType.EntitiesItemsSql));
        this.getField('mlLinkedEntities').setVisible(value == App.enums.SelectionType.Entities);
    },

    onAfterRender : function() {

        this.getForm().panel.add(Ext.create("Ext.Button", {
            text:'Выполнить',
            listeners: {
                scope: this,
                click: function() {
                    // By default, "this" will be the object that fired the event.
                    var func,
                        field = this.getField("id");

                    alert(this.name + " Begin!");

                    // Word export is reserved by ECMAScript standard
                    // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Reserved_Words
                    func = XmlExchange['export'];
                    func.call(this, field.getValue());
                }
            }
        }));
    }
});