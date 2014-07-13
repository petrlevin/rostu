/**
* @class App.events.LongTermGoalProgram_Activity_Value
* Обработчик клиентских событий сущности LongTermGoalProgram_Activity_Value
*/
Ext.define('App.events.FileLinkList', {
    extend: 'App.events.CommonList',
    requires: [
       'App.components.FileUploadDialog'
    ],
    
    events: [
        { name: 'afterrender', handler: 'onAfterRender', item: null }
    ],

    onAfterRender: function () {
        var me = this;
        
        var openBtn = this.getButton("open");
        openBtn.hide();

        var createBtn = this.getButton("create");
        createBtn.handler = function () {
            Ext.create('App.components.FileUploadDialog', { successCallback: me.refreshGrid, sender: me }).show();
        };
    },
    
    refreshGrid: function() {
        this.getGrid().store.reload();
    }
});