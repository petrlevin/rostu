/**
* Увеличиваем таймаут всех ajax запросов. 
* Мотив: на рабочем сервере Иркутска замечено медленное соединение с интернетом: 10 КБайт/сек
*/
Ext.define('App.patches.Connection', {

    override: 'Ext.data.Connection',
    timeout: 5 * 60 * 1000
});
