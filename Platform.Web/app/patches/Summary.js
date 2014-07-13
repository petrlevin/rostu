Ext.define('App.patches.Summary', {
    override: 'Ext.grid.feature.Summary',

    getSummary: function (store, type, field, group) {

        //<debug>
        if (Ext.isDefined(Ext.global.console)) {
        	Ext.global.console.log('App.patches.Summary > getSummary');
        }
        //</debug>

        if (store.proxy.reader.rawData) {
            var aggregates = store.proxy.reader.rawData.aggregates;
            if (aggregates) {
                return aggregates[field];
            }
        }
        return '';

    },
    
    createSummaryRecord: function (view) {
        var store = view.store.treeStore || view.store;
        var columns = view.headerCt.getVisibleGridColumns(),
            info = {
                records: view.store.getRange()
            },
            colCount = columns.length, i, column, 
            summaryRecord = this.summaryRecord || (this.summaryRecord = new store.model(null, view.id + '-summary-record'));

        // Set the summary field values
        summaryRecord.beginEdit();
        for (i = 0; i < colCount; i++) {
            column = columns[i];

            // In summary records, if there's no dataIndex, then the value in regular rows must come from a renderer.
            // We set the data value in using the column ID.
            if (!column.dataIndex) {
                column.dataIndex = column.id;
            }

            var summaryValue = this.getSummary(store, column.summaryType, column.dataIndex, info);
            if (Ext.isNumber(summaryValue)) {
                var renderedValue = column.renderer(summaryValue);
                summaryRecord.set(column.dataIndex, renderedValue);
            }
        }
        summaryRecord.endEdit(true);
        // It's not dirty
        summaryRecord.commit(true);
        summaryRecord.isSummary = true;

        return summaryRecord;
    }
});
