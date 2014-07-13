/**
* @class App.events.LimitBudgetAllocations
* Обработчик клиентских событий сущности "Предельные объемы бюджетных ассигнований"
*/
Ext.define('App.events.FinancialAndBusinessActivities', {
    extend: 'App.events.CommonItem',

    events: [
        { name: 'itemloaded', handler: 'onItemLoaded', item: null },
        { name: 'afterrender', handler: 'onAfterRender', item: null },
        { name: 'change', handler: 'onSBPChange', item: 'idSBP' },
        { name: 'dataget', handler: 'onDataget', item: null },
        { name: 'operationbegin', handler: 'onOperationBegin', item: null }
    ],
    
	docService: null,
    
	constructor: function(config){
			this.docService = SborFinancialAndBusinessActivitiesService;
			
			this.callParent([config]);
	},
	
    //M5 «Пропорционально прямым расходам по указанным КОСГУ»
    distributionMethodM1: 1,
    distributionMethodM2: 2,
    distributionMethodM3: 3,
    distributionMethodM4: 4,
    distributionMethodM5: 5,

    onOperationBegin: function () {
        this.PermissionsInputAdditionalRequirements();
        this.hideKbk();
    },
    
    onDataget: function () {
        this.PermissionsInputAdditionalRequirements();
        var sge = this.getField('tpDistributionMethodss');
        sge.grid.getSelectionModel().on('selectionchange', this.onSelectionDistributionMethod, this);
    },

    onAfterRender: function(sender) {
        
        var tbarActivities = this.getTopToolBar('tpActivity');
        this.addNewButton(tbarActivities, {
            id: 'tpActivity_FillData',
            text: 'Заполнить',
            handler: this.tpActivities_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);


        var tbarFinancialIndicatorsInstitutions = this.getTopToolBar('tpFinancialIndicatorsInstitutionss');
        
        this.addNewButton(tbarFinancialIndicatorsInstitutions, {
            id: 'tpFinancialIndicatorsInstitutionss_FillData',
            text: 'Заполнить',
            handler: this.tpFinancialIndicatorsInstitutionss_FillDataButton,
            tooltip: 'Заполнить автоматически.'
        }, 0);

        var item = tbarFinancialIndicatorsInstitutions.items.findBy(function (p) {
            if (p.text == "Создать") {
                return p;
            } else {
                return null;
            }
        });

        tbarFinancialIndicatorsInstitutions.remove(item);
        
        var sge = this.getField('tpDistributionMethodss');
        sge.grid.getSelectionModel().on('selectionchange', this.onSelectionDistributionMethod, this);
        sge.grid.searchField.hide();
        
        var tbarDistributionMethodss = this.getTopToolBar('tpDistributionMethodss');
        this.addNewButton(tbarDistributionMethodss, {
            id: 'RellocateIndirect',
            text: 'Распределить косвенные',
            handler: this.execButton,
            scope: this,
            tooltip: 'Распределить косвенные'
        }, 10);
    },
    
    onItemLoaded: function (sender) {
        this.hideKbk();
        this.PermissionsInputAdditionalRequirements();
    },

    getDocId: function () {
        return this.getField('id').getValue();
    },
    
    execButton: function () {
        this.callServiceFromGrid(this.docService.fillData_FBA_UpdateIndirect, 'tpDistributionMethodss', 'tpDistributionMethodss');
    },
    
    tpActivities_FillDataButton: function() {
        this.callServiceFromGrid(this.docService.fillData_FBA_Activity, 'tpActivity');
    },
    
    tpFinancialIndicatorsInstitutionss_FillDataButton: function () {
        this.callServiceFromGrid(this.docService.fillData_FBA_FinancialIndicatorsInstitutions, 'tpFinancialIndicatorsInstitutionss');
    },
    
    onSBPChange: function (sender) {
        this.hideKbk();
    },
    
    hideKbk: function () {
        var docId = this.getField('id').getValue();

        if (docId)
            this.docService.get_SbpblankByDocument(docId, this.onLoadSbpBlank, this);
    },

    onLoadSbpBlank: function (result, response) {
        if (result != null) {
            var hiddenFields = result.hidden;

            if (Ext.isArray(hiddenFields) && hiddenFields.length > 0) {
                //Расходы
                this.hideKbkOnGrid(hiddenFields, 'tpCostActivitiess');
                this.hideKbkOnGrid(hiddenFields, 'tpIndirectCosts');
            }
        }
    },

    hideKbkOnGrid: function (hiddenFields, gridName) {
        this.hideColumnsOnGrid(gridName, hiddenFields.concat(['idowner', 'idmaster', 'id', 'hasadditionalneed']));
    },
    
    onSelectionDistributionMethod: function (sender, selected, eOpts) {
        if (!selected) return;

        var selectedRecord = selected[0];
        if (!selectedRecord) {
            selectedRecord = this.getField('tpDistributionMethodss').grid.getSelectionModel().getSelection()[0];
        }

        if (!selectedRecord) return;

        var tbarAdditionalMethods = this.getTopToolBar('tpDistributionAdditionalParameters');

        var distributiveMethod = selectedRecord.get('idindirectcostsdistributionmethod');

        var visibleColumns = ['idfba_activity', 'activitytypecaption', 'contingentcaption'];

        switch (distributiveMethod) {
            case this.distributionMethodM1:
                break;
            case this.distributionMethodM2:
                visibleColumns = visibleColumns.concat(['ofg_direct', 'pfg1_direct', 'pfg2_direct']);
                break;
            case this.distributionMethodM3:
                visibleColumns = visibleColumns.concat(['ofg_activity', 'pfg1_activity', 'pfg2_activity']);
                break;
            case this.distributionMethodM4:
                visibleColumns = visibleColumns.concat(['factorofg', 'factorpfg1', 'factorpfg2']);
                break;
            case this.distributionMethodM5:
                visibleColumns = visibleColumns.concat(['ofg_direct', 'pfg1_direct', 'pfg2_direct']);
                break;
        }

        this.hideColumnsOnGrid('tpActivitiesDistributions', null, visibleColumns);

        if (distributiveMethod != this.distributionMethodM5)
            this.getButton(tbarAdditionalMethods, 'create').hide();
        else
            this.getButton(tbarAdditionalMethods, 'create').show();

    },

    PermissionsInputAdditionalRequirements: function () {

        var isRedact = !this.getButton(this.getTopToolBar('tpActivity'), 'create').disabled; // так я определяю режим редактирования
        if (isRedact)
            SborCommonService.getPermissionsInputAdditionalRequirements(this.getField('IdSBP').getValue(), function (result) { this.onPermissionsInputAdditionalRequirements(result); }, this);

    },
    onPermissionsInputAdditionalRequirements: function (result) {
        var HAN = this.getField('isExtraNeed');

        if (result) {
            HAN.enable();
        } else {
            HAN.disable();
        }
    }
});