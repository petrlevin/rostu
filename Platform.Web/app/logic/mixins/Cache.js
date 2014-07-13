Ext.define("App.logic.mixins.Cache", {

	requires: [
		'Ext.util.MixedCollection'
	],

	mixins: {
	    observable: 'Ext.util.Observable'
	},

	/**
	* Кэш моделей
	*/
	cache: null,

	/**
	* Список текущих транзакций с коллбэками
	*/
	progress: null,

	/**
	*
	*/
	fromCache: function (id, fetcher, callback, scope) {
	    // <debug>
	    if (Ext.isDefined(Ext.global.console)) {
	        Ext.global.console.log('fromCache: ' + id);
	    }
	    // </debug>
	    if (this.cache.containsKey(id)) {
	        var temp = Ext.clone(this.cache.get(id));
			if (callback) {
				Ext.callback(callback, scope || this, [this, temp]);
			} else {
				return temp;
			}
		} else {
			if (typeof (callback) !== 'function') {
				// <debug>
				if (Ext.isDefined(Ext.global.console)) {
					Ext.global.console.warn('Не указана callback функция при получении объекта и объекта нет в кэше!');
				}
				// </debug>
				Ext.Error.raise('При запросе объекта нужно указать callback функцию!');
				return false;
			}

		    this.addCallback(id, callback, scope);

			fetcher();
			
			return false;
		}
	},

	addCallback: function (id, callback, scope) {

	    var cb = {
	        cb: callback,
	        sc: scope
	    };
	    var callbackList = this.progress.getByKey(id);
	    if (callbackList) {
	        callbackList.push(cb);
	    } else {
	        this.progress.add(id, [cb]);
	    }
    },

    /**
	*
	*/
	registerInCache: function (id, value, response) {

	    this.cache.add(id, Ext.clone(value));
		if (response) {
			var trans = response.getTransaction(response);
			var callbackList = this.progress.removeAtKey(this.getKey(trans));
			if (callbackList) {
			    Ext.each(callbackList, function(cb) {
			        Ext.callback(cb.cb, cb.sc || this, [this, value]);
			    }, this);
			}
		}
	},

	/**
	* Конструктор, который создает новый объект данного класса
	* @param {Object} Объект с конфигурацией.
	*/
	constructor: function (config) {
		// <debug>
		if (Ext.isDefined(Ext.global.console)) {
			Ext.global.console.log('Cache constructor');
		}
		// </debug>


        this.cache = new Ext.util.MixedCollection();
        this.progress = new Ext.util.MixedCollection();
	    
        this.mixins.observable.constructor.call(this, config);

	    this.addEvents('fetched');
	}

});