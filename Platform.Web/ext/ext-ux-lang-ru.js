Ext.onReady(function() {
    if (Ext.grid.RowEditor) {
        Ext.apply(Ext.grid.RowEditor.prototype, {
            saveBtnText: "Сохранить",
            cancelBtnText: "Отменить",
            errorsText: "Некорректные данные:",
            dirtyText: "Вы должны сохранить или отменить изменения"
        });
    }
});