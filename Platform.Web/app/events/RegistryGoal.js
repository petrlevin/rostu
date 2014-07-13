/**
* @class App.events.VcpOmStatePrg
* Обработчик клиентских событий отчета "рееестр целей"
*/
Ext.define('App.events.RegistryGoal', {
    extend: 'App.events.CommonItem',

    events: [
		{ name: 'change', handler: 'onChangeByApproved', item: 'ConstructReportApprovedData' }
        ,{ name: 'change', handler: 'onChangeSourcesDataReports', item: 'idSourcesDataReports' }
        ,{ name: 'change', handler: 'onChangeOutputGoalOperatingPeriod', item: 'OutputGoalOperatingPeriod' }
        ,{ name: 'change', handler: 'onChangeDisplayResourceProvision', item: 'DisplayResourceProvision' }
    ],

    onChangeByApproved: function (sender, newvalue, oldvalue) {
            var fdt = this.getField('DateReport');
            if (newvalue) {
                fdt.allowBlank = false;
                fdt.enable();
                fdt.setValue(new Date());
            }
            else {
                fdt.allowBlank = true;
                fdt.setValue(null);
                fdt.disable();
            }
    },
    onChangeSourcesDataReports: function (sender, newvalue, oldvalue) {
        var brad = this.getField('ConstructReportApprovedData');
        var str = this.getField('DisplayResourceSupport');
        var resurs = this.getField('DisplayResourceProvision');
        if (newvalue == 5) {
            brad.setValue(null);
            brad.disable();
            str.setValue(null);
            str.disable();
            resurs.disable();
        }
        else {
            brad.setValue(true);
            brad.enable();
            str.setValue(null);
            str.disable();
            resurs.enable();
        }
    },

    onChangeDisplayResourceProvision: function (sender, newvalue, oldvalue) {
        var brad = this.getField('DisplayResourceSupport');
        if (newvalue) {
            brad.allowBlank = false;
            brad.enable();
        }
        else {
            brad.allowBlank = true;
            brad.setValue(null);
            brad.disable();
        }
    },

    onChangeOutputGoalOperatingPeriod: function (sender, newvalue, oldvalue) {
        var dst = this.getField('DateStart');
        var den = this.getField('DateEnd');

        if (newvalue) {
            dst.allowBlank = false;
            den.allowBlank = false;
            dst.show();
            den.show();
            //dst.enable();
            //den.enable();
            var idBudget = this.getField('idBudget').getValue();
            var idBudgetEntity = -1879048154;
            var buildperiod = sender.getValue();
            if (buildperiod) {
                DataService.getItem(idBudgetEntity, idBudget, this.onLoadBudget, this);
            }
        }
        else {
            dst.allowBlank = true;
            dst.setValue(null);
            //dst.disable();
            dst.hide();
            
            den.allowBlank = true;
            den.setValue(null);
            //den.disable();
            den.hide();
        }
        //if (newvalue) {
            //    //dst.allowBlank = false;
            //    var budget = result[0];
            //    if (budget) {

            //        var budgetYear = budget.year;
            //        this.getField('Year').setValue(budgetYear);

            //    }
            //    dst.enable();
            //    den.enable();
                
            //}
            //else {
            //    dst.disable();
            //    den.disable();
            //}
        
            
            
    },
    

    onLoadBudget: function (result, response) {
       
        var budget = result.result[0];
        if (budget) {

            var budgetYear = budget.year;
            var date = new Date(budgetYear, 0, 1, 0, 0, 0, 000);
            this.getField('DateStart').setValue(date);
            budgetYear = budgetYear + 2;
            date = new Date(budgetYear, 11, 31, 0, 0, 0, 000);
            this.getField('DateEnd').setValue(date);
            
        }
    }
    
    });


//OutputGoalOperatingPeriod