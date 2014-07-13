Ext.define('App.exceptions.Handler', {

    requires: [
        'App.exceptions.UserFriendly',
        'App.exceptions.Default',
        'App.exceptions.Control',
        'App.exceptions.System',
        'Ext.JSON'
    ],

    /**
	* @private
	*/
    singleton: true,

    handle: function(event) {
        var ex = event.where ? Ext.JSON.decode(event.where) : null;
        if (!ex)
            return;
        var clientHandler = Ext.create("App.exceptions." + (ex.ClientHandler ? ex.ClientHandler : ex.IsUserFriendly ? "UserFriendly" : "Default"));
        clientHandler.handle(ex);

    }
})

