Ext.define('App.events.TestFileList', {
    extend: 'App.events.CommonList',
    
    events: [
        { name: 'storeload', handler: 'onStoreLoad', item: null }
    ],

    onStoreLoad: function () {
        //alert("store load");
    }//,

    /*refreshGrid: function () {
        this.getGrid().store.reload();
    }*/
});