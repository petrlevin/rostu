Ext.define("App.logic.mixins.Columns", {

    getVisibleColumns: function(selector) {
        if (!selector)
            selector = function(col) { return col; };
        if (this.grid)
            return this.getVisibleColumns.call(this.grid ,selector);
        else {
            var result = [];
            //после реконфигурации грида, в свойстве column нет добавленных колонок. Но они есть тут headerCt.items.items
            if (this.headerCt.items)
                Ext.each(this.headerCt.items.items, function (col) {
                    if (col.isVisible()) {
                        result.push(selector(col));
                    }
                });
            return result;
        }
    },
    getVisibleColumnsName: function () {
        return this.getVisibleColumns(function(col) { return col.name; });
    }
})