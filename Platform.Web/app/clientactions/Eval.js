Ext.define('App.clientactions.Eval', {

    constructor: function (config) {
        Ext.apply(this, config);
    },
    
    // TODO:Это КРАЙНЕ опасный метод. Должно быть обязательно переписано.
    execute: function (callback, scope) {
    	// Это ОЧЕНЬ плохо!
        eval(this.code);
    }
});