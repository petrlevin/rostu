Ext.define('App.components.FiltersFeature', {
    extend: 'Ext.ux.grid.FiltersFeature',

    uses: [
        'App.components.filters.LinkFilter',
        'App.components.filters.DateTimeFilter',
        'App.components.filters.BooleanFilter',
        'App.components.filters.StringFilter',
        'App.components.filters.DateFilter',
        'App.components.filters.NumericFilter'
    ],

    menuFilterText: 'Фильтр',
    paramPrefix: 'Filters',

    init: function (grid) {
        var me = this,
            view = me.view,
            headerCt = view.headerCt;

        me.bindStore(grid.getStore(), true);

        // Listen for header menu being created
        headerCt.on('menucreate', me.onMenuCreate, me);

        view.on('refresh', me.onRefresh, me);
        grid.on({
            scope: me,
            beforestaterestore: me.applyState,
            beforestatesave: me.saveState,
            beforedestroy: me.destroy
        });

        // Add event and filters shortcut on grid panel
        grid.filters = me;
        grid.addEvents('filterupdate');
    },

    getGridPanel: function () {
        return this.view.up('gridpanel') || this.view.up('treepanel');
    },

    /**
     * @private
     * Get the filter menu from the filters MixedCollection based on the clicked header
     */
    getMenuFilter: function () {
        var header = this.view.headerCt.getMenu().activeHeader,
            filter = this.filters.get(header.dataIndex),
            field;

        if (Ext.isEmpty(filter)) {
            field = Ext.Array.findBy(this.getGridPanel().columns, function(item) {
                return item.name === header.dataIndex;
            });
            this.addFilter(Ext.apply({
                dataIndex: field.dataIndex
            }, field.filter));
        }
        return this.filters.get(header.dataIndex);
    },

    /**
     * Returns an Array of the currently active filters.
     * @return {Array} filters Array of the currently active filters.
     */
    getFilterData : function () {
        var items = this.getFilterItems(),
            field = '',
            filters = [],
            tempFilters = {},
            n, nlen, item, d, i, len;

        for (n = 0, nlen = items.length; n < nlen; n++) {
            item = items[n];
            if (item.active) {
                d = [].concat(item.serialize());
                Ext.each(d, function (filter) {
                    if (filter.comparison === App.enums.ComparisionOperator.InList ||
                        filter.comparison === App.enums.ComparisionOperator.IsNotNull ||
                        filter.comparison === App.enums.ComparisionOperator.IsNull) {

                        field = item.dataIndex.substring(0, item.dataIndex.indexOf('_caption'));
                    } else {
                        field = item.dataIndex;
                    }
                    if (!Ext.isDefined(tempFilters[field])) {
                        tempFilters[field] = [];
                    }
                    tempFilters[field].push(filter);
                });
            }
        }
        Ext.iterate(tempFilters, function (name) {
            filters.push({
                field: name,
                data: tempFilters[name]
            });
        });
        return filters;
    },

    /** @private */
    reload: function () {
        var me = this,
            store = me.view.getStore();

        if (me.local) {
            store.clearFilter(true);
            store.filterBy(me.getRecordFilter());
            store.sort();
        } else {
            me.deferredUpdate.cancel();
            if (store.buffered) {
                store.data.clear();
            }

            if (Ext.getClassName(this.grid) === 'App.components.EntityTreeGrid') {
                me.grid.setRootNode({ leaf: false });
                me.grid.getRootNode().expand();
            } else {
                store.loadPage(1);
            }
        }
    },

    buildQuery: function(filters) {
        var p = {}, i, f, root, tmp,
            len = filters.length;

        tmp = [].concat(filters);
       
        if (tmp.length > 0) {
            p[this.paramPrefix] = tmp;
        }
        return p;
    },

    /**
     * Function for locating filter classes, overwrite this with your favorite
     * loader to provide dynamic filter loading.
     * @param {String} type The type of filter to load ('Filter' is automatically
     * appended to the passed type; eg, 'string' becomes 'StringFilter').
     * @return {Function} The Ext.ux.grid.filter.Class
     */
    getFilterClass: function (type) {
        // map the supported Ext.data.Field type values into a supported filter
        switch (type) {
            case 'auto':
                type = 'extstring';
                break;
            case 'int':
            case 'float':
                type = 'extnumeric';
                break;
            case 'bool':
                type = 'extboolean';
                break;
        }
        return Ext.ClassManager.getByAlias('gridfilter.' + type);
    }
});