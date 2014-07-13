/**
* @class App.logic.mixins.TablepartsDenormalizer
* Адаптер денормализации табличных частей. Реконфигурирует денормализованные табличные части на форме элемента.
*/
Ext.define('App.logic.mixins.TablepartsDenormalizer', {

	requires: [
		'App.logic.factory.Columns',
		'App.logic.ModelManager',
		'Ext.util.Observable'
	],

	mixins: {
		observable: 'Ext.util.Observable'
	},

	constructor: function () {

	    this.denormalizedFields = {};
	    // признаки
	    this.isServiceCallFinished = false;
	    this.isDenormFieldLoaded = false;
	    this.isFormReady = false;
	    this.isDenormalizerFinished = false;

		this.mixins.observable.constructor.call(this);

		this.addEvents(
			/**
			* @event denormalizerfinished
			* Событие возникает после того как денормализатор прошелся по всем ТЧ
			*/
			'denormalizerfinished'
		);

		this.on('denormalizerfinished', this.onDenormalizerFinished, this);
		this.on('beforeshow', this.onBeforeShow, this);

		this.on('model',         this.denormalizeTablepartsInOwner, this);
		this.on('aftersave',     this.denormalizeTablepartsInOwner, this);
		this.on('operationdone', this.denormalizeTablepartsInOwner, this);
	},
	
	/**
	 * Запрашивает данные с сервера для каждой денормализованной ТЧ и добавляет в них значимые колонки.
	 */
	denormalizeTablepartsInOwner: function () {

	    var tpModels = this.getDenormTableparts();
		if (!tpModels || tpModels.length == 0 || this.shouldBeClosed || !this.docid) {
			this.fireEvent('denormalizerfinished');
			return;
		}

		var ownerFields = this.model.result; // модели полей сущности владельца

		// Обращается к сервису за моделью значимых полей для денормализованной ТЧ
		Ext.each(tpModels, function (tp) {
			var tpField = ownerFields.filter(function (field) {
				return field.identitylink == tp.model.entityId;
			}, this)[0]; // модель поля денормализуемой ТЧ. Например: tpField.name == tpGoalIndicatorValue

			this.denormalizedFields[tpField.name] = null; // добавляем свойсво. Значением будет модель полей и периоды, полученные от сервиса

			var cb = Ext.bind(this.denormFieldLoaded, this, [tpField.name], true);
			DenormalizerService.getFields(this.entity.id, this.docid, tp.model.entityId, cb, this);
		}, this);
		this.isServiceCallFinished = true; // закончили отправлять вызовы к сервису
	},

	getDenormTableparts: function () {

	    if (this.model && this.model.hasDenormalizedTableparts) {
		    return this.model.tableParts.filter(function (tp) {
				return tp.model.isDenormalizedTablepart;
			}, this);
		}
		return [];
	},
	
	denormFieldLoaded: function (result, response, success, o, tpFieldName) {

	    this.denormalizedFields[tpFieldName] = {
			periods: result.periods,
			fields: result.fields
		};

		if (this.isComplete()) {
			this.isDenormFieldLoaded = true;
			this.tryReconfigure();
		}
	},
	
	/**
	 * Провка того, что модели полей для всех денормализованных ТЧ получены
	 */
	isComplete: function () {

	    return this.isServiceCallFinished &&
	        Ext.Array.every(Ext.Object.getValues(this.denormalizedFields), function (value) {
    	        return value != null;
	        });
	},

	onBeforeShow: function () {

	    this.isFormReady = true;
		this.tryReconfigure();
	},

	tryReconfigure: function () {

	    if (!this.isDenormFieldLoaded || !this.isFormReady)
	        return; // еще не завершено построение формы или еще не пришли модели полей для денормализованных ТЧ. Это не отмена операции вцелом, один из слудеющих вызовов прокатит

	    Ext.Object.each(this.denormalizedFields, function (tpName, settings) {
	        this.reconfigureGrid(tpName, settings);
	    }, this);
	},

	reconfigureGrid: function (fieldName, settings) {
	    var tablePart = this.getField(fieldName);

	    // В сущности могут быть денормализованные ТЧ, которые отсутствуют на форме.
	    if (!tablePart) // если поле не найдено, то просто выходим
	        return;

	    var grid = tablePart.grid;

	    // модель
	    var tpModel = Ext.clone(tablePart.list.model);
	    tpModel.result = tpModel.result.concat(settings.fields); // добавляю колонки

	    // поля
	    var fieldsCfg = Ext.create('App.logic.ModelManager').getFieldsConfig(tpModel);
	    grid.getStore().model.setFields(fieldsCfg);

	    // колонки
	    var newColumns = Ext.create('App.logic.factory.Columns', {
	        model: tpModel
	    }).get();

	    // доп параметры
	    var extraParams = grid.getStore().getProxy().extraParams;
	    extraParams.denormalizedPeriods = settings.periods;

	    // грид
	    grid.reconfigure(grid.getStore(), newColumns);

	    this.fireEvent('denormalizerfinished');
	},

	onDenormalizerFinished: function () {

	    this.isDenormalizerFinished = true;
		this.refreshGrids();
	}
});