/**
 * ControlingProvider предоставляет доступ к методам веб-сервисов по технологии ExtDirect.
 * В ControlingProvider входит клиентская часть функционала взаимодействия системы с пользователем. См. статью:
 * http://conf.rostu-comp.ru/pages/viewpage.action?pageId=13599286.
 */
Ext.define('App.direct.ControlingProvider', {
    extend: 'Ext.direct.RemotingProvider',
    alias: 'direct.remotingprovider',

    // <debug>
    maxRetries: 0, // установлено с целью предотвращения появления в отладчике C# двух потоков (от двух попыток при значении по умолчанию = 1).
    // </debug>

    /**
     * Configure a direct request
     * @private
     * @param {String} action The action being executed
     * @param {Object} method The being executed
     */
    configureRequest: function (action, method, args) {
        var me = this,
            callData, data, callback, scope, opts, transaction, params;

        if (this.isCommunicationMethod(method, args)) {
            // Первый параметр будет рассматриваться как контекст
            args = [{}].concat(args);
        }

        callData = method.getCallData(args);
        data = callData.data;
        callback = callData.callback;
        scope = callData.scope;
        opts = callData.options || {};

        params = Ext.apply({}, {
            provider: me,
            args: args,
            action: action,
            method: method.name,
            data: data,
            callbackOptions: opts,
            callback: scope && Ext.isFunction(callback) ? Ext.Function.bind(callback, scope) : callback
        });

        if (opts.timeout) {
            Ext.applyIf(params, {
                timeout: opts.timeout
            });
        };

        transaction = new Ext.direct.Transaction(params);

        if (me.fireEvent('beforecall', me, transaction, method) !== false) {
            Ext.direct.Manager.addTransaction(transaction);
            me.queueTransaction(transaction);
            me.fireEvent('call', me, transaction, method);
        }
    },

    /**
    * @private
    * @param method {Ext.Direct.RemotingMethod}
    * Определяет, нужен ли методу объект CommunicationContext
    */
    isCommunicationMethod: function (method, args) {
        var len = method.len;
        var data = args.slice(0, len);
        if (args.length + 1 == len || typeof(data[data.length - 1]) == 'function')
            return true;
        return false;
    },

    /**
     * React to the ajax request being completed
     *
     * @private
     */
    onData: function (options, success, response) {
        var me = this, exception,
            i, len, events, event, transaction, transactions;

        if (success) {
            events = me.createEvents(response);

            for (i = 0, len = events.length; i < len; ++i) {
                event = events[i];
                transaction = me.getTransaction(event);
                if(!Ext.isEmpty(event.where)) {
                    try {
                        exception = Ext.decode(event.where);
                    } catch (e) {
                    }
                }
            
                if (!Ext.isEmpty(exception) && exception.IsInteractive === true) {

                    this.interactWithUser(exception, function(userAnswer, skipRetry) {

                        var context = transaction.data[0];
                        context.answers = context.answers || {};
                        context.answers[exception.InteractionId] = userAnswer;

                        if (skipRetry === true) {
                            me.fireEvent('data', me, event);

                            if (transaction && me.fireEvent('beforecallback', me, event, transaction) !== false) {
                                me.runCallback(transaction, event, true);
                                Ext.direct.Manager.removeTransaction(transaction);
                            }
                        } else {
                            transaction.retry();
                        }
                    }, this);
                } else {
                    me.fireEvent('data', me, event);

                    if (transaction && me.fireEvent('beforecallback', me, event, transaction) !== false) {
                        me.runCallback(transaction, event, true);
                        Ext.direct.Manager.removeTransaction(transaction);
                    }
                }
            }
        }
        else {
            transactions = [].concat(options.transaction);

            for (i = 0, len = transactions.length; i < len; ++i) {
                transaction = me.getTransaction(transactions[i]);

                if (transaction && transaction.retryCount < me.maxRetries) {
                    transaction.retry();
                }
                else {
                    event = new Ext.direct.ExceptionEvent({
                        data: null,
                        transaction: transaction,
                        code: Ext.direct.Manager.exceptions.TRANSPORT,
                        message: 'Unable to connect to the server.',
                        xhr: response
                    });

                    me.fireEvent('data', me, event);

                    if (transaction && me.fireEvent('beforecallback', me, transaction) !== false) {
                        me.runCallback(transaction, event, false);
                        Ext.direct.Manager.removeTransaction(transaction);
                    }
                }
            }
        }
    },

    interactWithUser: function (exception, callback, scope) {

        Ext.each(exception.ClientActions, function (action) {

            Ext.create("App.clientactions." + action.ClientHandler, action).execute(callback, scope);
        }, this);


        //// Для отправки клиенту ответа
        //// Ext.callback(callback, scope, [ answer ]);
        
        

        //Ext.Msg.show({
        //    title: 'Вопрос от сервера',
        //    buttons: Ext.MessageBox.YESNO,
        //    icon: Ext.Msg.QUESTION,
        //    msg: exception.Message,
        //    fn: function (btn) {
        //        var answer = btn;
        //        if (btn == 'no') {
        //            answer = {
        //                SBPType: 1,
        //                Price: 12
        //            };
        //            Ext.callback(callback, scope, [ answer ]);
        //        }
        //    }
        //});
    },
    
    reportError: function (event, exception) {
        //<debug>
	    Ext.Error.raise('Произошла ошибка при обработке конфигурации интерактивного исключения!');
	    if (Ext.isDefined(Ext.global.console)) {
		    Ext.global.console.log('Произошло исключение!');
		    Ext.global.console.dir(event);
		    Ext.global.console.dir(exception);
	    }
        //</debug>
    }
});