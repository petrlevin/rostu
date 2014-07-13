/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
* Pavel Dovgalenko (Pavel.dovgalenko@gmail.com)
*
*/
/**
* @class App.store.Navigation
* @extends Ext.data.TreeStore
*/
Ext.define('App.store.Navigation', {
	extend: 'Ext.data.TreeStore',
	alias: 'store.navigation',
	storeId: 'navigation',
	autoSync: true

});
