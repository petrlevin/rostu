Ext.define('App.components.Chart', {
    
    constructor: function(config) {

        this.container = this.getContainer(config);
    },
    
    getContainer: function (config) {
        
        return {
            title: config.caption,
            bodyPadding: 5,

            getForm: function () {
                return {
                    //layout:'fit',
                    items: {
                        //height: '100%',
                        src: config.img,
                        style: {
                            'display': 'block',
                            'margin': 'auto'
                        },
                        xtype: 'image'
                    },
                    style: {
                        'display': 'table-cell',
                        'vertical-align': 'middle'
                    },
                    xtype: 'container'
                };
            },
            getWindowKey: function () {
                return config.id;
            }
        };
    },

    open: function() {

        App.WindowMgr.add(this.container, {}, true);
    }
});