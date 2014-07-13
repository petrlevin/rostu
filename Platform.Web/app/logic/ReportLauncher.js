/*
*
* Copyright (c) 2006-2013 R.O.S.T.U.
*
*/
/**
* @class App.components.ReportLauncher
* Запускатель отчетов
*/

Ext.define('App.logic.ReportLauncher', {
    /**
	* Используемые данным объектом классы
	*/
    requires: [
        'App.logic.HttpRequest'
    ],

    reportAddParams: function (reportType) {
        if (!reportType)
            return {};
        
        switch (reportType.toString().toLowerCase()) {
            case 'tablereport':
                return { handlerType: 'TableReport' };
            case 'wordreport':
                return { handlerType: 'WordCommonReport' };
            default:
                return {};
        }
        
    },
    

    reportUrls: function(reportType) {
        if (!reportType)
            return null;
        
        switch ( reportType.toString().toLowerCase() ) {
            case 'ordinalreport':
                return { url: 'Services/Report.aspx' };
            case 'tablereport':
                return { url: 'Services/DownloadFile.aspx'};
            case 'wordreport':
                return { url: 'Services/DownloadFile.aspx' };
            case 'printform':
                return { url: 'Services/PrintForm.aspx' };
            default:
                // <debug>
                if (Ext.isDefined(Ext.global.console)) {
                    Ext.global.console.log('Обработчик для типа ' + reportType + ' не определен');
                }
                // </debug>
                return null;
        }
        
    },

    /**
    * use as App.ReportLauncher.launch(params, reportType, url, target);
    */
    launch: function (params, reportType, url, target) {

        params = Ext.apply(params, this.reportAddParams(reportType));

        if (url && Ext.isString(url))
                url = { url: url };

        var request = Ext.create('App.logic.HttpRequest', Ext.applyIf({
            params: params,
            target: target || '_blank'
        }, url || this.reportUrls(reportType)));

        request.submit();

    }
});
