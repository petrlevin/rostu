Ext.define('App.events.Control', {
    extend: 'App.events.CommonItem',

    events: [
		{
		    name: 'dataget',
		    handler: 'onDataGet',
		    item: null
		},
        {
            name: 'change',
            handler: 'onManagedChange',
            item:'managed'
        },
        {
            name: 'afterrender',
            handler: 'onAfterRender',
            item:null
        }
    ],

    onAfterRender: function (panel, data) {
        ProfileService.isSuperUser(function(result) {
            if (!result) {
                this.getField('managed').disable();
                this.getField('name').disable();
                this.getField('caption').disable();
                this.getField('UNK').disable();
                this.getField('idEntity').disable();

            }
        },this
        );
            
        
    },

    onDataGet: function (panel,data) {


            this.getField('tpExceptions').setVisible(data.managed);


    },
    
    onManagedChange: function (sender, value) {

        
            this.getField('tpExceptions').setVisible(value);


        

    }


    

});